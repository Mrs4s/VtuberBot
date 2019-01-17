using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using MongoDB.Driver;
using QQ.Framework;
using QQ.Framework.Domains;
using QQ.Framework.HttpEntity;
using QQ.Framework.Utils;
using VtuberBot.Database;
using VtuberBot.Database.Entities;
using VtuberBot.Network.Twitter;
using VtuberBot.Network.Youtube;
using VtuberBot.Tools;
using StringTools = OfflineServer.Lib.Tools.StringTools;

namespace VtuberBot.Robots
{
    public class VtuberBot : CustomRobot
    {
        public List<IRobotCommand> Commands { get; } = new List<IRobotCommand>();

        public CommentDatabase CommentDatabase { get; } = new CommentDatabase(Path.Combine(Directory.GetCurrentDirectory(), "Comments"));

        private long _lastMessage = 0;
        private DateTime _lastTime;
        private readonly IMongoCollection<YoutubeLiveInfo> _youtubeLiveCollection;
        private readonly IMongoCollection<BiliBiliLiveInfo> _biliLiveCollection;
        private readonly IMongoCollection<TweetInfo> _tweetCollection;


        public VtuberBot(ISendMessageService service, IServerMessageSubject transponder, QQUser user) : base(service, transponder, user)
        {
            _youtubeLiveCollection = Program.Database.GetCollection<YoutubeLiveInfo>("youtube-live-details");
            _biliLiveCollection = Program.Database.GetCollection<BiliBiliLiveInfo>("bili-live-details");
            _tweetCollection = Program.Database.GetCollection<TweetInfo>("tweet-details");
            CacheManager.Manager.VtuberBeginLiveEvent += (vtuber, video) =>
            {
                var info = YoutubeApi.GetYoutubeVideo(video.VideoId);
                LogHelper.Info($"Vtuber {vtuber.OriginalName} 开始了直播 {video.VideoLink}");
                foreach (var (key, value) in Config.DefaultConfig.Subscribes)
                {
                    var config = value.FirstOrDefault(v =>
                        string.Equals(v.VtuberName, vtuber.OriginalName, StringComparison.CurrentCultureIgnoreCase));
                    if (config?.BeginLive ?? false)
                        _service.SendToGroup(key, $"{vtuber.OriginalName} 在 {info.LiveDetails.ActualStartTime.ToUniversalTime().AddHours(8):yyyy-MM-dd HH:mm:ss} 开始直播 {video.Title}\r\n" +
                                                  $"链接: {video.VideoLink}\r\n当前观众数量: {info.LiveDetails.ViewersCount}\r\n" +
                                                  $"原定直播时间： {info.LiveDetails.ScheduledStartTime.ToUniversalTime().AddHours(8):yyyy-MM-dd HH:mm:ss}\r\n" +
                                                  $"实际开播时间:  {info.LiveDetails.ActualStartTime.ToUniversalTime().AddHours(8):yyyy-MM-dd HH:mm:ss}");
                }

                var recorder = new LiveChatRecorder(info.LiveDetails.LiveChatId, vtuber,video.VideoId);
                recorder.StartRecord();
                recorder.LiveStoppedEvent += (id, recder) =>
                {
                    LogHelper.Info($"{vtuber.OriginalName} 已停止直播, 正在保存评论数据...");
                    CommentDatabase.SaveLiveChatComments(info.ChannelId, info.VideoId, recorder.RecordedComments);
                    info = YoutubeApi.GetYoutubeVideo(video.VideoId) ?? info;
                    var live = new YoutubeLiveInfo()
                    {
                        Title = info.Title,
                        Channel = info.ChannelId,
                        BeginTime = info.LiveDetails?.ActualStartTime ?? default(DateTime),
                        EndTime = info.LiveDetails?.ActualEndTime ?? DateTime.Now,
                        VideoId = video.VideoId
                    };
                    CommentDatabase.AddChannelLiveInfo(live);
                    _youtubeLiveCollection.InsertOne(live);
                    LogHelper.Info("保存完毕");
                };
            };

            CacheManager.Manager.VtuberUploadVideoEvent += (vtuber, video) =>
            {
                LogHelper.Info($"Vtuber {vtuber.OriginalName} 上传了视频 {video.Title}");
                foreach (var (key, value) in Config.DefaultConfig.Subscribes)
                {
                    var config = value.FirstOrDefault(v =>
                        string.Equals(v.VtuberName, vtuber.OriginalName, StringComparison.CurrentCultureIgnoreCase));
                    if (config?.UploadVideo ?? false)
                        _service.SendToGroup(key, $"{vtuber.OriginalName} 在 {video.PublishTime.ToUniversalTime().AddHours(8):yyyy-MM-dd HH:mm:ss} 上传了视频 {video.Title}\r\n" +
                                                  $"链接: {video.VideoLink}");
                }
            };

            CacheManager.Manager.VtuberPublishTweetEvent += (vtuber, tweet) =>
            {
                LogHelper.Info($"Vtuber {vtuber.OriginalName} 发布了新的推特 {tweet.Content.Substring(0, 5)}....");
                foreach (var (key, value) in Config.DefaultConfig.Subscribes)
                {
                    var config = value.FirstOrDefault(v =>
                        string.Equals(v.VtuberName, vtuber.OriginalName, StringComparison.CurrentCultureIgnoreCase));
                    if (config?.PublishTweet ?? false)
                        _service.SendToGroup(key, $"{vtuber.OriginalName} 在 {tweet.CreateTime.ToUniversalTime().AddHours(8):yyyy-MM-dd HH:mm:ss} 发布了:\r\n" +
                                                       $"{(tweet.Content.Length > 255 ? tweet.Content.Substring(0, 255) : tweet.Content)}");
                }

                try
                {
                    _tweetCollection.InsertOne(tweet);
                }
                catch (Exception ex)
                {
                    LogHelper.Error("Insert object error", true, ex);
                }
            };

            CacheManager.Manager.VtuberReplyTweetEvent += (vtuber, tweet) =>
            {
                LogHelper.Info($"Vtuber {vtuber.OriginalName} 回复了 {tweet.ReplyScreenname} 的推特 {tweet.Content.Substring(0, 5)}....");
                foreach (var (key, value) in Config.DefaultConfig.Subscribes)
                {
                    var config = value.FirstOrDefault(v =>
                        string.Equals(v.VtuberName, vtuber.OriginalName, StringComparison.CurrentCultureIgnoreCase));
                    if (config?.ReplyTweet ?? false)
                        _service.SendToGroup(key, $"{vtuber.OriginalName} 在 {tweet.CreateTime.ToUniversalTime().AddHours(8):yyyy-MM-dd HH:mm:ss} 回复了 {tweet.RetweetedTweet}:\r\n" +
                                                  $"{(tweet.Content.Length > 255 ? tweet.Content.Substring(0, 255) : tweet.Content)}");
                }
                try
                {
                    _tweetCollection.InsertOne(tweet);
                }
                catch (Exception ex)
                {
                    LogHelper.Error("Insert object error", true, ex);
                }
            };

            CacheManager.Manager.VtuberRetweetedEvent += (vtuber, tweet) =>
            {
                LogHelper.Info($"Vtuber {vtuber.OriginalName} 转发了 {tweet.RetweetedTweet.User.Name} 的推特: {tweet.Content.Substring(0, 5)}....");
                foreach (var (key, value) in Config.DefaultConfig.Subscribes)
                {
                    var config = value.FirstOrDefault(v =>
                        string.Equals(v.VtuberName, vtuber.OriginalName, StringComparison.CurrentCultureIgnoreCase));
                    if (config?.Retweeted ?? false)
                        _service.SendToGroup(key, $"{vtuber.OriginalName} 在 {tweet.CreateTime.ToUniversalTime().AddHours(8):yyyy-MM-dd HH:mm:ss} 转发了 {tweet.RetweetedTweet.User.Name} 的推:\r\n" +
                                                  $"{(tweet.Content.Length > 255 ? tweet.Content.Substring(0, 255) : tweet.Content)}");
                }
                try
                {
                    _tweetCollection.InsertOne(tweet);
                }
                catch (Exception ex)
                {
                    LogHelper.Error("Insert object error", true, ex);
                }
            };

            CacheManager.Manager.VtuberBeginLiveBilibiliEvent += (vtuber, bUser) =>
            {
                var beginTime = DateTime.Now;
                LogHelper.Info($"Vtuber {vtuber.OriginalName} 在B站开始了直播 {bUser.LiveUrl}");
                foreach (var (key, value) in Config.DefaultConfig.Subscribes)
                {
                    var config = value.FirstOrDefault(v =>
                        string.Equals(v.VtuberName, vtuber.OriginalName, StringComparison.CurrentCultureIgnoreCase));
                    if (config?.BilibiliBeginLive ?? false)
                        _service.SendToGroup(key, $"{vtuber.OriginalName} 在B站开始了直播 {bUser.LiveTitle}\r\n" +
                                                  $"链接: {bUser.LiveUrl}");
                }

                var liveId = StringTools.RandomString;
                var liveInfo = new BiliBiliLiveInfo()
                {
                    LiveId = liveId,
                    Title = bUser.LiveTitle,
                    BeginTime = beginTime,
                    EndTime = beginTime,
                    MaxPopularity = 0
                };
                var live = _biliLiveCollection.FindAsync(v => v.MaxPopularity == 0 && v.Title == bUser.LiveTitle)
                    .GetAwaiter().GetResult().FirstOrDefault();
                if (live != null)
                    liveId = live.LiveId;
                else
                    _biliLiveCollection.InsertOne(liveInfo);
                var recorder = new DanmakuRecorder(bUser.LiveRoomId, liveId, vtuber);
                recorder.StartRecord();
                recorder.LiveStoppedEvent += info =>
                {
                    LogHelper.Info($"{vtuber.OriginalName} 已停止在B站的直播.");
                    liveInfo = new BiliBiliLiveInfo()
                    {
                        LiveId = liveId,
                        Title = bUser.LiveTitle,
                        BeginTime = beginTime,
                        EndTime = DateTime.Now,
                        MaxPopularity = recorder.Client.MaxPopularity
                    };
                    _biliLiveCollection.UpdateOne(v => v.LiveId == liveId, "{$set:" + liveInfo + "}");
                };
            };

        }

        public override void ReceiveFriendMessage(long friendNumber, Richtext content)
        {
            var friends = new List<FriendItem>();
            if (_user.Friends == null)
                return;
            foreach (var items in _user.Friends.Result.Where(v => v.Mems != null).Select(v => v.Mems))
                friends.AddRange(items);
            var friend = friends.FirstOrDefault(v => v.Uin != null && v.Uin == friendNumber);
            LogHelper.Info($"收到好友 [{friend?.Name}({friendNumber})] 的消息：{content}");
            var cmd = Commands.FirstOrDefault(v => v.Names.Any(name => content.ToString().Trim().ToLower().StartsWith(name.ToLower())));
            try
            {
                cmd?.Process(_service, new MessageInfo()
                {
                    IsGroupMessage = false,
                    UserMember = friendNumber,
                    FriendInfo = friend,
                    Content = content
                });
            }
            catch (Exception ex)
            {
                LogHelper.Error("处理消息时出现未知异常 包名：" + cmd?.GetType(), true, ex);
            }
        }

        public override void ReceiveGroupMessage(long groupNumber, long fromNumber, Richtext content)
        {
            if (content == null || content.ToString().Trim() == string.Empty || fromNumber == 0 || _user.Groups == null)
                return;
            if (_lastMessage == fromNumber && _lastTime != default(DateTime) && (DateTime.Now - _lastTime).Seconds <= 1)
                return;
            _lastMessage = fromNumber;
            _lastTime = DateTime.Now;
            var groups = new List<GroupItem>();
            if (_user.Groups.Create != null)
                groups.AddRange(_user.Groups.Create);
            if (_user.Groups.Join != null)
                groups.AddRange(_user.Groups.Join);
            if (_user.Groups.Manage != null)
                groups.AddRange(_user.Groups.Manage);
            var group = groups.FirstOrDefault(v => v.Gc == groupNumber);
            var member = group?.Members.Mems.FirstOrDefault(v => v.Uin == fromNumber);
            LogHelper.Info($"收到来自群 [{group?.Gn}({groupNumber})] 的 [{member?.Card}({fromNumber})] 的消息：{content}", Config.DefaultConfig.LogGroupMessage);
            var cmd = Commands.FirstOrDefault(v => v.Names.Any(name => content.ToString().Trim().ToLower().StartsWith(name.ToLower())));
            new Thread(() =>
            {
                try
                {
                    cmd?.Process(_service, new MessageInfo()
                    {
                        IsGroupMessage = true,
                        GroupNumber = groupNumber,
                        GroupInfo = group,
                        GroupMemberInfo = member,
                        UserMember = fromNumber,
                        Content = content.ToString().Trim()
                    });
                }
                catch (Exception ex)
                {
                    LogHelper.Error("处理消息时出现未知异常 包名：" + cmd?.GetType(), true, ex);
                }
            }).Start();
        }
    }

    public interface IRobotCommand
    {
        string[] Names { get; }

        void Process(ISendMessageService service, MessageInfo message);
    }

    public abstract class RobotCommandBase : IRobotCommand
    {
        public abstract string[] Names { get; }

        protected ISendMessageService _service;
        protected Action NextPageAction;


        protected RobotCommandBase(ISendMessageService service)
        {
            _service = service;
        }

        public virtual void Process(ISendMessageService service, MessageInfo message)
        {
            var methods = GetType().GetMethods().Where(v => v.IsDefined(typeof(RobotCommandAttribute), false));
            var atts = methods.Select(v =>
                    v.GetCustomAttributes(false).First(att => att.GetType() == typeof(RobotCommandAttribute)))
                .Select(v => (RobotCommandAttribute)v);
            var handled = false;
            var args = message.Content.ToString().Trim().Split(' ');
            foreach (var method in methods)
            {
                var attr = method.GetCustomAttributes(false);
                var commandAttr = attr.FirstOrDefault(v => v.GetType() == typeof(RobotCommandAttribute));
                if (commandAttr != null)
                {
                    var info = (RobotCommandAttribute)commandAttr;
                    if (args.Length == info.ProcessLength || info.ProcessLength == 0)
                    {
                        if (!string.IsNullOrEmpty(info.SubCommandName))
                        {
                            if (args.Length <= info.SubCommandOffset)
                                continue;
                            if (!StringTools.EqualsIgnoreCase(args[info.SubCommandOffset], info.SubCommandName))
                                continue;
                        }
                        else
                        {
                            if (atts.Any(v => !string.IsNullOrEmpty(v.SubCommandName) && args.Length > v.SubCommandOffset && args[v.SubCommandOffset] == v.SubCommandName))
                                continue;
                        }
                        try
                        {
                            handled = true;
                            method.Invoke(this, new object[]
                            {
                                message,
                                args
                            });
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error("处理消息时出现未知异常 包名：" + GetType() + " 函数名: " + method.Name, true, ex);
                            service.SendToGroup(message.GroupNumber,$"处理请求时出现未知异常: {ex.Message} 处理函数: {method.Name}");
                        }
                    }
                }
            }
            if (!handled)
                ShowHelpMessage(message, args);
        }

        [RobotCommand(processLength:2,offset:1,subCommandName:"下一页")]
        public void NextPageCommand(MessageInfo message,string[] args)
        {
            if (NextPageAction == null)
            { 
                _service.SendToGroup(message.GroupNumber,"已经到底了~");
                return;
            }
            NextPageAction();
        }


        public abstract void ShowHelpMessage(MessageInfo message, string[] args);
    }

    public class MessageInfo
    {

        public bool IsGroupMessage { get; set; }

        public long GroupNumber { get; set; }

        public GroupMember GroupMemberInfo { get; set; }

        public GroupItem GroupInfo { get; set; }

        public long UserMember { get; set; }

        public FriendItem FriendInfo { get; set; }

        public Richtext Content { get; set; }
    }

}

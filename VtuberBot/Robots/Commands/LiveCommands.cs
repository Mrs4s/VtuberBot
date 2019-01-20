using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using QQ.Framework.Domains;
using QQ.Framework.Utils;
using VtuberBot.Database;
using VtuberBot.Database.Entities;
using VtuberBot.Network.Youtube;
using VtuberBot.Tools;

namespace VtuberBot.Robots.Commands
{
    public class LiveCommand : RobotCommandBase
    {
        public override string[] Names { get; } = { "!live", "！live", "!直播", "！直播" };

        private readonly IMongoCollection<YoutubeLiveChat> _chatCollection;
        private readonly IMongoCollection<YoutubeLiveInfo> _liveCollection;


        public override void Process(ISendMessageService service, MessageInfo message)
        {
            if (!message.IsGroupMessage)
            {
                service.SendToFriend(message.UserMember, "暂不支持好友互动！");
                return;
            }
            base.Process(service, message);
            var args = message.Content.ToString().Split(' ');
            if (args.Length == 1)
            {
                var str = "当前直播列表: \r\n";
                str += string.Join("\r\n",
                    CacheManager.Manager.LastCheckLiveStatus.ToArray().Where(v => v.Value.IsLive).Select(video =>
                        video.Key.OriginalName + " 在 " + video.Value.VideoLink + " 直播中"));
                str += "\r\n";
                str += string.Join("\r\n",
                    CacheManager.Manager.LastCheckLiveBStatus.ToArray().Where(v => v.Value.AreLive)
                        .Select(v => v.Value.Username + " 在 " + v.Value.LiveUrl + " 直播中"));
                service.SendToGroup(message.GroupNumber, str);
            }
        }

        public override void ShowHelpMessage(MessageInfo message, string[] args)
        {
            _service.SendToGroup(message.GroupNumber, "使用方法:" +
                                                     "\r\n!直播                          -查看当前直播列表" +
                                                     "\r\n!直播 历史 <Vtuber名称>         -查看该Vtuber直播历史" +
                                                     "\r\n!直播 历史 <Vtuber名称> <序号>  -查看该次直播详细信息" +
                                                     "\r\n!直播 评论 <Vtuber名称> <序号>  -查看该次直播评论信息" +
                                                     "\r\n!直播 评论 <Vtuber名称> <序号> 复读排序 -查看该次直播复读统计");
        }

        public LiveCommand(ISendMessageService service) : base(service)
        {
            _chatCollection = Program.Database.GetCollection<YoutubeLiveChat>("youtube-live-chats");
            _liveCollection = Program.Database.GetCollection<YoutubeLiveInfo>("youtube-live-details");
        }


        [RobotCommand(offset: 1, subCommandName: "历史")]
        public void LiveHistoryCommand(MessageInfo message, string[] args)
        {
            var vtuber = Config.DefaultConfig.GetVtuber(args[2]);
            if (vtuber == null)
            {
                _service.SendToGroup(message.GroupNumber, "数据库中不存在" + args[2]);
                return;
            }

            var history = _liveCollection.FindAsync(doc => doc.Channel == vtuber.YoutubeChannelId).GetAwaiter().GetResult().ToList();
            if (args.Length == 3)
            {
                var str = $"关于 {vtuber.OriginalName} 的直播历史:";
                var num = 1;
                foreach (var liveInfo in history)
                {
                    str +=
                        $"\r\n{num++}: {liveInfo.BeginTime.ToUniversalTime().AddHours(8):yyyy-MM-dd HH:mm:ss} - {liveInfo.Title}";
                    if (num % 5 == 0)
                    {
                        _service.SendToGroup(message.GroupNumber, str);
                        str = string.Empty;
                    }
                }
                if (num % 5 != 0)
                    _service.SendToGroup(message.GroupNumber, str);
                return;
            }

            if (args.Length == 4)
            {
                _service.SendToGroup(message.GroupNumber, "正在查询..");
                if (!history.Any())
                {
                    _service.SendToGroup(message.GroupNumber, "未找到任何记录，请等待下次直播，机器人将自动记录");
                    return;
                }
                if (int.Parse(args[3]) > history.Count)
                {
                    _service.SendToGroup(message.GroupNumber, $"序号超出范围 (1-{history.Count + 1})");
                    return;
                }

                var info = history[int.Parse(args[3]) - 1];
                var comments = _chatCollection.FindAsync(doc => doc.VideoId == info.VideoId).GetAwaiter().GetResult()
                    .ToList();
                var msg = $"关于 {vtuber.OriginalName} 的直播历史" +
                          $"\r\n标题: {info.Title}" +
                          $"\r\n直播时间: {info.BeginTime.ToUniversalTime().AddHours(8):yyyy-MM-dd HH:mm:ss} - {info.EndTime.ToUniversalTime().AddHours(8):yyyy-MM-dd HH:mm:ss} ({(info.EndTime - info.BeginTime).TotalMinutes:f}分钟)" +
                          $"\r\n链接: https://www.youtube.com/watch?v={info.VideoId}" +
                          $"\r\n数据库中的聊天总数: {comments.Count}" +
                          $"\r\nSuperChat数量: {comments.Count(v => v.IsSuperChat)}" +
                          $"\r\nSuperChat总金额: {comments.Where(v => v.IsSuperChat).Sum(v => v.SuperChatDetails["amountMicros"].ToObject<long>() / 1000000)}JPY(暂未处理其他货币)" +
                          $"\r\n-----------------------";
                _service.SendToGroup(message.GroupNumber, msg);
            }
        }

        [RobotCommand(offset: 1, subCommandName: "评论")]
        public void CommentInfoCommand(MessageInfo message, string[] args)
        {
            if (args.Length != 4 && args.Length != 5)
            {
                _service.SendToGroup(message.GroupNumber, "使用方法: !直播 评论 <Vtuber名字> <序号> [复读排序]");
                return;
            }
            var vtuber = Config.DefaultConfig.GetVtuber(args[2]);
            if (vtuber == null)
            {
                _service.SendToGroup(message.GroupNumber, "数据库中不存在" + args[2]);
                return;
            }
            var history = _liveCollection.FindAsync(doc => doc.Channel == vtuber.YoutubeChannelId).GetAwaiter().GetResult().ToList();
            if (!history.Any())
            {
                _service.SendToGroup(message.GroupNumber, "未找到任何记录，请等待下次直播，机器人将自动记录");
                return;
            }
            _service.SendToGroup(message.GroupNumber, "正在查询..");
            var info = history[int.Parse(args[3]) - 1];
            var comments = _chatCollection.FindAsync(doc => doc.VideoId == info.VideoId).GetAwaiter().GetResult().ToList();
            var dic = new Dictionary<string, int>();
            foreach (var comment in comments)
            {
                if (!dic.ContainsKey(comment.DisplayMessage))
                    dic.Add(comment.DisplayMessage, 0);
                dic[comment.DisplayMessage]++;
            }

            var msg = string.Empty;
            if (args.Length == 5 && args.Last() == "复读排序")
            {
                var dicDesc = dic.OrderByDescending(v => v.Value);
                var num = 0;
                msg = $"关于 {vtuber.OriginalName} 的评论分析:";
                foreach (var pair in dicDesc)
                {
                    msg += $"\r\n{pair.Key} 被复读 {pair.Value} 次";
                    if (num++ >= 10)
                        break;
                }
                _service.SendToGroup(message.GroupNumber, msg);
                return;
            }
            var count = comments.Where(v => v.TextMessageDetails?.HasValues ?? false).Count(v =>
                v.TextMessageDetails["messageText"].ToObject<string>().ChineseRatio() > 80 &&
                v.DisplayMessage.Length >= 2 && v.DisplayMessage.ToCharArray()
                    .All(c => !c.IsHinaganaOrKatakana() && c.IsSimplifiedChinese()));
            msg = $"关于 {vtuber.OriginalName} 直播的评论分析:" +
                      $"\r\n标题: {info.Title}" +
                      $"\r\n评论总数: {comments.Count}" +
                      $"\r\n平均每分钟评论数: {(comments.Count / (info.EndTime - info.BeginTime).TotalMinutes):F}" +
                      $"\r\n最多复读的词: {dic.First(v => v.Value == dic.Max(d => d.Value)).Key} (复读 {dic.Max(v => v.Value)} 次)" +
                      $"\r\n疑似大陆天狗评论数: {count} (估算)";
            _service.SendToGroup(message.GroupNumber, msg);
        }
    }
}

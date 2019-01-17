using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using OfflineServer.Lib.Tools;
using QQ.Framework.Domains;
using VtuberBot.Database;
using VtuberBot.Database.Configs;
using VtuberBot.Network.BiliBili;
using VtuberBot.Network.Hiyoko;
using VtuberBot.Tools;

namespace VtuberBot.Robots.Commands
{
    public class MenuCommand : IRobotCommand
    {
        public string[] Names => new[] { "!Menu", "！Menu", "!菜单", "菜单", "指令列表", "!help", "help" };
        public void Process(ISendMessageService service, MessageInfo message)
        {
            if (!message.IsGroupMessage)
            {
                service.SendToFriend(message.UserMember, "暂不支持好友互动！");
                return;
            }
            service.SendToGroup(message.GroupNumber, "====== Vtuber天狗机器人功能菜单 ====== \r\n!Timeline 查看放送时间表\r\n" +
                                                     "!订阅 添加群内事件订阅\r\n" +
                                                     "!vtuber 查看数据库中的vtuber信息\r\n" +
                                                     "!直播 查看直播信息");
        }
    }

    public class VtuberInfoCommand : RobotCommandBase
    {
        public override string[] Names { get; } = { "!Vtuber", "！Vtuber" };

        public VtuberInfoCommand(ISendMessageService service) : base(service)
        {
        }

        public override void Process(ISendMessageService service, MessageInfo message)
        {
            if (!message.IsGroupMessage)
            {
                service.SendToFriend(message.UserMember, "暂不支持好友互动！");
                return;
            }
            base.Process(service, message);
        }
        public override void ShowHelpMessage(MessageInfo message, string[] args)
        {
            _service.SendToGroup(message.GroupNumber, "帮助: " +
                                                     "\r\n!Vtuber list          -查看数据库中的Vtuber列表" +
                                                     "\r\n!Vtuber <Vtuber名称>  -查看Vtuber详细信息" +
                                                     "\r\n!Vtuber add <Vtuber日文名> -使用互联网数据添加Vtuber" +
                                                     "\r\n!Vtuber 设置中文名 <Vtuber原名> <中文名>  -设置中文名" +
                                                     "\r\n!Vtuber 添加昵称 <Vtuber> <昵称>  -添加昵称" +
                                                     "\r\n!Vtuber 删除昵称 <Vtuber> <昵称>  -删除昵称");
        }

        [RobotCommand(processLength: 2)]
        public void VtuberProfileCommand(MessageInfo message, string[] args)
        {
            var vtuber = Config.DefaultConfig.GetVtuber(args[1]);
            if (vtuber == null && args[1] != "list")
            {
                _service.SendToGroup(message.GroupNumber, $"数据库中不存在{args[1]}!");
                return;
            }
            _service.SendToGroup(message.GroupNumber, $"Vtuber相关信息：\r\n" +
                                                     $"原名: {vtuber.OriginalName}\r\n" +
                                                     $"中文名: {vtuber.ChineseName}\r\n" +
                                                     $"昵称: {string.Join(',', vtuber.NickNames)}\r\n" +
                                                     $"Youtube频道: https://www.youtube.com/channel/{vtuber.YoutubeChannelId}\r\n" +
                                                     $"推特主页: https://twitter.com/{vtuber.TwitterProfileId}\r\n" +
                                                     $"B站搬运: https://space.bilibili.com/{vtuber.BilibiliUserId}");
        }

        [RobotCommand(processLength: 2, offset: 1, subCommandName: "list")]
        public void VtuberListCommand(MessageInfo message, string[] args)
        {
            var list = Config.DefaultConfig.Vtubers.Select(v => v.OriginalName).ToList();
            if (list.Count <= 10)
            {
                _service.SendToGroup(message.GroupNumber,
                    $"当前数据库中的Vtuber名单:\r\n{string.Join(',', list)}");
                return;
            }

            var page = 1;
            _service.SendToGroup(message.GroupNumber,
                $"当前数据库中的Vtuber名单 ({page}/{list.Count / 20 + 1}): \r\n{string.Join(',', list.Take(20))}");
            _service.SendToGroup(message.GroupNumber,"可使用!Vtuber 下一页 来翻页");
            NextPageAction = () =>
            {
                page++;
                _service.SendToGroup(message.GroupNumber,
                    $"当前数据库中的Vtuber名单 ({page}/{list.Count / 20 + 1}): \r\n{string.Join(',', list.Skip(page * 20).ToList().Take(20))}");
            };

        }

        [RobotCommand(offset: 1, subCommandName: "set")]
        public void SetVtuberCommand(MessageInfo message, string[] args)
        {
            if (args.Length != 5)
            {
                _service.SendToGroup(message.GroupNumber, "使用方法： !Vtuber set type name data");
                return;
            }
            var name = args[3];
            var data = args[4];
            var vtb = Config.DefaultConfig.GetVtuber(name);
            switch (args[2].ToLower())
            {
                case "name":
                    if (vtb != null)
                    {
                        _service.SendToGroup(message.GroupNumber, "该Vtuber已存在");
                        return;
                    }
                    Config.DefaultConfig.Vtubers.Add(new VtuberInfo()
                    {
                        OriginalName = name
                    });
                    Config.SaveToDefaultFile(Config.DefaultConfig);
                    _service.SendToGroup(message.GroupNumber, "已添加: " + name);
                    return;
                case "ytb":
                    if (vtb == null)
                    {
                        _service.SendToGroup(message.GroupNumber, "该Vtuber不存在");
                        return;
                    }

                    vtb.YoutubeChannelId = data;
                    Config.SaveToDefaultFile(Config.DefaultConfig);
                    _service.SendToGroup(message.GroupNumber, "已设置");
                    return;
                case "tw":
                    if (vtb == null)
                    {
                        _service.SendToGroup(message.GroupNumber, "该Vtuber不存在");
                        return;
                    }

                    vtb.TwitterProfileId = data;
                    Config.SaveToDefaultFile(Config.DefaultConfig);
                    _service.SendToGroup(message.GroupNumber, "已设置");
                    return;
                case "cname":
                    if (vtb == null)
                    {
                        _service.SendToGroup(message.GroupNumber, "该Vtuber不存在");
                        return;
                    }

                    vtb.ChineseName = data;
                    Config.SaveToDefaultFile(Config.DefaultConfig);
                    _service.SendToGroup(message.GroupNumber, "已设置");
                    return;
                case "ul":
                    if (vtb == null)
                    {
                        _service.SendToGroup(message.GroupNumber, "该Vtuber不存在");
                        return;
                    }

                    vtb.UserlocalProfile = data;
                    Config.SaveToDefaultFile(Config.DefaultConfig);
                    _service.SendToGroup(message.GroupNumber, "已设置");
                    return;
                case "bilibili":
                    if (vtb == null)
                    {
                        _service.SendToGroup(message.GroupNumber, "该Vtuber不存在");
                        return;
                    }

                    vtb.BilibiliUserId = long.Parse(data);
                    Config.SaveToDefaultFile(Config.DefaultConfig);
                    _service.SendToGroup(message.GroupNumber, "已设置");
                    return;
                default:
                    _service.SendToGroup(message.GroupNumber, "未知属性");
                    return;
            }
        }

        [RobotCommand(processLength: 3, offset: 1, subCommandName: "add")]
        public void AddVtuberCommand(MessageInfo message, string[] args)
        {
            var vtb = Config.DefaultConfig.GetVtuber(args[2]);
            if (vtb != null)
            {
                _service.SendToGroup(message.GroupNumber, "已存在该Vtuber");
                return;
            }

            var streamers = HiyokoApi.SearchStreamer(args[2]);
            if (streamers.Count != 1)
            {
                _service.SendToGroup(message.GroupNumber, "无法找到" + args[2]);
                return;
            }

            var streamer = streamers.First();
            vtb = new VtuberInfo()
            {
                OriginalName = streamer.Name,
                TwitterProfileId = streamer.TwitterId,
                YoutubeChannelId = streamer.Channels.FirstOrDefault(v => v.ChannelType == 1)?.ChannelId,
                HiyokoProfileId = streamer.Name
            };
            Config.DefaultConfig.Vtubers.Add(vtb);
            Config.SaveToDefaultFile(Config.DefaultConfig);
            _service.SendToGroup(message.GroupNumber, "已根据互联网相关资料添加: " + args[2] + "\r\n可使用!Vtuber set修改");
            _service.SendToGroup(message.GroupNumber, $"Vtuber相关信息: \r\n" +
                                                     $"原名: {streamer.Name}\r\n" +
                                                     $"推特主页: https://twitter.com/{streamer.TwitterId}\r\n" +
                                                     $"Youtube频道: https://www.youtube.com/channel/{streamer.Channels.FirstOrDefault(v => v.ChannelType == 1)?.ChannelId}");
        }

        [RobotCommand(processLength: 4, offset: 1, subCommandName: "设置中文名")]
        public void SetChineseNameCommand(MessageInfo message, string[] args)
        {
            var vtb = Config.DefaultConfig.GetVtuber(args[2]);
            if (vtb == null)
            {
                _service.SendToGroup(message.GroupNumber, "未找到Vtuber");
                return;
            }

            vtb.ChineseName = args[3];
            Config.SaveToDefaultFile(Config.DefaultConfig);
            _service.SendToGroup(message.GroupNumber, "已设置");
            var bUsers = BiliBiliApi.SearchBiliBiliUsers(vtb.ChineseName);
            var uploader = bUsers.OrderByDescending(v => v.Follower).FirstOrDefault(v => v.IsUploader);
            if (uploader != null && string.IsNullOrEmpty(vtb.ChineseName))
            {
                vtb.BilibiliUserId = uploader.Id;
                Config.SaveToDefaultFile(Config.DefaultConfig);
                _service.SendToGroup(message.GroupNumber, $"已根据中文名自动查找B站搬运组:" +
                                                         $"\r\n用户名: {uploader.Username}" +
                                                         $"\r\n主页: https://space.bilibili.com/{uploader.Id}" +
                                                         $"\r\n粉丝数: {uploader.Follower}");
                _service.SendToGroup(message.GroupNumber, "可使用!Vtuber 设置B站 <Vtuber名字> <B站空间ID> 来修改");
            }
        }

        [RobotCommand(processLength: 4, offset: 1, subCommandName: "添加昵称")]
        public void SetNicknameCommand(MessageInfo message, string[] args)
        {
            var vtuber = Config.DefaultConfig.GetVtuber(args[2]);
            if (vtuber == null)
            {
                _service.SendToGroup(message.GroupNumber, "未找到Vtuber");
                return;
            }

            if (vtuber.NickNames.Any(v => v.EqualsIgnoreCase(args[3])))
            {
                _service.SendToGroup(message.GroupNumber, "已存在该昵称");
                return;
            }
            vtuber.NickNames.Add(args[3]);
            Config.SaveToDefaultFile(Config.DefaultConfig);
            _service.SendToGroup(message.GroupNumber, "添加完成");
        }

        [RobotCommand(processLength: 4, offset: 1, subCommandName: "删除昵称")]
        public void RemoveNicknameCommand(MessageInfo message, string[] args)
        {
            var vtuber = Config.DefaultConfig.GetVtuber(args[2]);
            if (vtuber == null)
            {
                _service.SendToGroup(message.GroupNumber, "未找到Vtuber");
                return;
            }

            if (vtuber.NickNames.All(v => !v.EqualsIgnoreCase(args[3])))
            {
                _service.SendToGroup(message.GroupNumber, "未存在该昵称");
                return;
            }
            vtuber.NickNames.RemoveAll(v=>v.EqualsIgnoreCase(args[3]));
            Config.SaveToDefaultFile(Config.DefaultConfig);
            _service.SendToGroup(message.GroupNumber, "删除完成");
        }

        [RobotCommand(processLength: 4, offset: 1, subCommandName: "设置B站")]
        public void SetBilibiliCommand(MessageInfo message, string[] args)
        {
            var vtb = Config.DefaultConfig.GetVtuber(args[2]);
            if (vtb == null)
            {
                _service.SendToGroup(message.GroupNumber, "未找到Vtuber");
                return;
            }
            var spaceId = long.Parse(args[3]);
            var info = BiliBiliApi.GetBiliBiliUser(spaceId);
            if (info == null)
            {
                _service.SendToGroup(message.GroupNumber, "未找到" + spaceId);
                return;
            }
            vtb.BilibiliUserId = spaceId;
            Config.SaveToDefaultFile(Config.DefaultConfig);
            _service.SendToGroup(message.GroupNumber, $"保存完成:" +
                                                      $"\r\n用户名: {info.Username}" +
                                                      $"\r\n主页: https://space.bilibili.com/{info.Id}" +
                                                      $"\r\n粉丝数: {info.Follower}");
        }





    }

    public class SubscribeCommand : IRobotCommand
    {
        public string[] Names { get; } = { "!订阅", "！订阅", "添加订阅", "subscribe" };
        public void Process(ISendMessageService service, MessageInfo message)
        {
            if (!message.IsGroupMessage)
            {
                service.SendToFriend(message.UserMember, "暂不支持好友互动！");
                return;
            }
            var args = message.Content.ToString().Split(' ');
            if (args.Length != 3 && args.Length != 4 && args.Length != 2)
            {
                service.SendToGroup(message.GroupNumber, "使用方法： !订阅 <添加/查看/取消> <发推/转推/回推/油管开播/油管上传/B站开播>  <Vtuber名字>");
                return;
            }

            if (!Config.DefaultConfig.Subscribes.ContainsKey(message.GroupNumber))
                Config.DefaultConfig.Subscribes.Add(message.GroupNumber, new List<SubscribeConfig>());
            var configs = Config.DefaultConfig.Subscribes[message.GroupNumber];
            if (args.Last() == "查看")
            {
                var str = "本群订阅列表:\r\n";
                str += string.Join(',',
                    configs.Where(v => v.BeginLive || v.PublishTweet || v.ReplyTweet || v.Retweeted || v.UploadVideo).Select(v => v.VtuberName));
                service.SendToGroup(message.GroupNumber, str);
                return;
            }
            var vtuberName = args[1] == "查看" ? args[2] : args[3];
            var vtuber = Config.DefaultConfig.GetVtuber(vtuberName);
            if (vtuber == null)
            {
                service.SendToGroup(message.GroupNumber, $"未知Vtuber {vtuberName} !");
                return;
            }

            vtuberName = vtuber.OriginalName;
            if (configs.All(v => !v.VtuberName.EqualsIgnoreCase(vtuberName)))
            {
                configs.Add(new SubscribeConfig()
                {
                    VtuberName = vtuberName
                });
            }

            var config = configs.First(v => string.Equals(v.VtuberName, vtuber.OriginalName, StringComparison.CurrentCultureIgnoreCase));
            if (args[1] == "查看")
            {

                service.SendToGroup(message.GroupNumber, $"==== 本群{config.VtuberName}订阅状态 ====\r\n" +
                                                        $"推特发推: {config.PublishTweet}\r\n" +
                                                        $"推特转推: {config.Retweeted}\r\n" +
                                                        $"推特回推: {config.ReplyTweet}\r\n" +
                                                        $"油管上传: {config.UploadVideo}\r\n" +
                                                        $"油管开播: {config.BeginLive}\r\n" +
                                                        $"B站开播: {config.BilibiliBeginLive}");
                return;
            }

            if (args[1] == "添加")
            {
                switch (args[2].ToLower())
                {
                    case "发推":
                        config.PublishTweet = true;
                        break;
                    case "转推":
                        config.Retweeted = true;
                        break;
                    case "回推":
                        config.ReplyTweet = true;
                        break;
                    case "油管开播":
                        config.BeginLive = true;
                        break;
                    case "油管上传":
                        config.UploadVideo = true;
                        break;
                    case "b站开播":
                        if (vtuber.BilibiliUserId == default(long))
                        {
                            service.SendToGroup(message.GroupNumber, "该Vtuber未绑定B站搬运，请使用!Vtuber 设置中文名 来绑定");
                            return;
                        }

                        config.BilibiliBeginLive = true;
                        break;
                    default:
                        service.SendToGroup(message.GroupNumber, $"未知订阅");
                        return;
                }
                service.SendToGroup(message.GroupNumber, $"成功订阅");
                Config.SaveToDefaultFile(Config.DefaultConfig);
                return;
            }

            if (args[1] == "取消")
            {
                switch (args[2].ToLower())
                {
                    case "发推":
                        config.PublishTweet = false;
                        break;
                    case "转推":
                        config.Retweeted = false;
                        break;
                    case "回推":
                        config.ReplyTweet = false;
                        break;
                    case "油管开播":
                        config.BeginLive = false;
                        break;
                    case "油管上传":
                        config.UploadVideo = false;
                        break;
                    case "b站开播":
                        config.BilibiliBeginLive = false;
                        break;
                    default:
                        service.SendToGroup(message.GroupNumber, $"未知订阅");
                        return;
                }
                service.SendToGroup(message.GroupNumber, $"成功取消");
                Config.SaveToDefaultFile(Config.DefaultConfig);
                return;
            }
        }
    }
}

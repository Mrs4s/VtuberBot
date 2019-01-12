using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QQ.Framework.Domains;
using VtuberBot.Database;
using VtuberBot.Network.UserLocal;

namespace VtuberBot.Robots.Commands
{
    public class TimeLineCommand : IRobotCommand
    {
        public string[] Names { get; } = {"!TimeLine", "！TimeLine", "!放送时间表", "！放送时间表", "放送时间表"};
        public void Process(ISendMessageService service, MessageInfo message)
        {
            if (!message.IsGroupMessage)
            {
                service.SendToFriend(message.UserMember, "暂不支持好友互动！");
                return;
            }

            var start = 0;
            var end = 3;
            if (message.Content.ToString().Trim().Split(' ').Length == 2)
            {
                start = int.Parse(message.Content.ToString().Split(' ').Last().Split('-').First())-1;
                end = int.Parse(message.Content.ToString().Split(' ').Last().Split('-').Last())-1;
                if (start == -1)
                    start = 0;
            }
            var timeLine = CacheManager.Manager.LastCheckTimeline;
            if (end> timeLine.Count-1)
                end = timeLine.Count-1;
            if (end - start > 5)
                end = start + 5;
            var msg = $"时间线({start+1}-{end+1}) 共 {timeLine.Count} 条记录";
            for (int i = start; i <= end; i++)
            {
                var info = timeLine[i];
                msg +=
                    $"\r\n放送时间: {info.LiveTime.Month}月{info.LiveTime.Day}日 {info.LiveTime.Hour}时{info.LiveTime.Minute}分 (预定)";
                msg += $"\r\n标题: {info.Title} 放送者: {info.VTuber}";
            }

            service.SendToGroup(message.GroupNumber, msg);
        }
    }

    public class OfficeInfoCommand : IRobotCommand
    {
        public string[] Names { get; } = { "!office" , "查询会社" };
        public void Process(ISendMessageService service, MessageInfo message)
        {
            if (!message.IsGroupMessage)
            {
                service.SendToFriend(message.UserMember, "暂不支持好友互动！");
                return;
            }

            var args = message.Content.ToString().Split(' ');
            if (args.Length == 1)
            {
                service.SendToGroup(message.GroupNumber,"使用方法： 查询会社 <会社名>");
                return;
            }
            //TODO: 会社名->OfficeName 从文件读取
            var officeName = string.Empty;
            switch (args[1])
            {
                case "hololive":
                case "homolive":
                case "ホロライブ":
                    officeName = "cover";
                    break;
            }

            if (string.IsNullOrEmpty(officeName))
            {
                service.SendToGroup(message.GroupNumber, "未知会社");
                return;
            }

            var info = UserLocalApi.GetOfficeInfo(officeName);
            service.SendToGroup(message.GroupNumber, $"会社名: {info.OfficeName}\r\n" +
                                                     $"旗下Vtuber数: {info.ChannelCount}\r\n" +
                                                     $"总关注量: {info.TotalFanCount}\r\n" +
                                                     $"平均关注: {info.AvgFanCount}");
        }
    }
}

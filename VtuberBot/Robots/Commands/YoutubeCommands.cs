using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QQ.Framework.Domains;
using VtuberBot.Network.Youtube;

namespace VtuberBot.Robots.Commands
{
    public class YoutubeSearchCommand : IRobotCommand
    {
        public string[] Names { get; } = { "!查询" , "！查询" ," Search" };
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
                service.SendToGroup(message.GroupNumber, "使用方法： 查询 <Youtube Id>");
                return;
            }

            var videos = YoutubeApi.GetVideosByChannelId(args[1]);
            if (videos.All(v => !v.IsLive))
            {
                service.SendToGroup(message.GroupNumber, "该频道目前没有直播");
                return;
            }

            var live = videos.First(v => v.IsLive);
            service.SendToGroup(message.GroupNumber,$"频道 {live.ChannelTitle} 当前正在直播中\r\n" +
                                                    $"Title: {live.Title}\r\n" +
                                                    $"Description: {live.Description}\r\n" +
                                                    $"Link: {live.VideoLink}");
        }
    }
}

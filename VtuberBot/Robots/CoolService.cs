using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QQ.Framework;
using QQ.Framework.Domains;
using QQ.Framework.Domains.Observers;
using QQ.Framework.Utils;
using Sisters.WudiLib;

namespace VtuberBot.Robots
{
    public class CoolService : ISendMessageService , IServerMessageSubject
    {
        public void SendToFriend(long friendNumber, Richtext content)
        {
            Program.Client.SendPrivateMessageAsync(friendNumber, content).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Only support one image sending.
        /// </summary>
        /// <param name="groupNumber"></param>
        /// <param name="content"></param>
        public void SendToGroup(long groupNumber, Richtext content)
        {
            if (content.Snippets.Any(v => v.Type == MessageType.Picture))
            {
                var image = content.Snippets.First(v => v.Type == MessageType.Picture);
                var bytes = image.Get<byte[]>("data");
                Program.Client.SendGroupMessageAsync(groupNumber, SendingMessage.ByteArrayImage(bytes)).GetAwaiter().GetResult();
                return;
            }

            Program.Client.SendGroupMessageAsync(groupNumber, content).GetAwaiter().GetResult();
        }

        public void ReceiveFriendMessage(long friendNumber, Richtext content)
        {
            //
        }

        public void ReceiveGroupMessage(long groupNumber, long fromNumber, Richtext content)
        {
            //
        }

        public void AddCustomRoBot(IServerMessageObserver robot)
        {
            //
        }

        public void RemoveCustomRoBot(IServerMessageObserver robot)
        {
            //
        }
    }
}

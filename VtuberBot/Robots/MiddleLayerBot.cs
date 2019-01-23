using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QQ.Framework;
using QQ.Framework.Domains;
using QQ.Framework.HttpEntity;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using Sisters.WudiLib.Responses;
using VtuberBot.Tools;

namespace VtuberBot.Robots
{
    public abstract class MiddleLayerBot : CustomRobot
    {
        private readonly Dictionary<long, GroupMemberInfo[]> _membersCache = new Dictionary<long, GroupMemberInfo[]>();


        protected MiddleLayerBot(ISendMessageService service, IServerMessageSubject transponder, QQUser user) : base(service, transponder, user)
        {
            if (!Config.DefaultConfig.UseLocalClient)
            {
                Program.Listener.MessageEvent += (api, message) =>
                {
                    if (message is GroupMessage)
                    {
                        var groupMessage = (GroupMessage) message;
                        ReceiveGroupMessage(groupMessage.GroupId,groupMessage.Sender.UserId,groupMessage.Content.Text);
                        return;
                    }

                    if (message is PrivateMessage)
                    {
                        var privateMessage = (PrivateMessage) message;
                        ReceiveFriendMessage(privateMessage.UserId,privateMessage.Content.Text);
                    }
                };
            }
        }

        protected string GetGroupCard(long groupId, long userId)
        {
            if (!Config.DefaultConfig.UseLocalClient)
            {
                if (!_membersCache.ContainsKey(groupId))
                    _membersCache.Add(groupId,
                        Program.Client.GetGroupMemberListAsync(groupId).GetAwaiter().GetResult());
                var user = _membersCache[groupId].FirstOrDefault(v => v.UserId == userId);
                return user?.Nickname;
            }
            
            return null;
        }

        


    }
}

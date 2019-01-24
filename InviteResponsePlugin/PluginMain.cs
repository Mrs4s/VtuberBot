using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using VtuberBot;
using VtuberBot.Plugin;
using VtuberBot.Tools;

namespace InviteResponsePlugin
{
    public class PluginMain :PluginBase
    {
        public override string Name { get; } = "InviteResponsePlugin";
        public override void OnLoad()
        {
            if(Config.DefaultConfig.UseLocalClient)
                throw new NotSupportedException("Please use coolq client.");
            Program.Listener.GroupInviteEvent += Listener_GroupInviteEvent;
            Program.Listener.GroupAddedEvent += Listener_GroupAddedEvent;
            LogInfo("Loaded.");
        }

        private async void Listener_GroupAddedEvent(HttpApiClient arg1, GroupMemberIncreaseNotice arg2)
        {
            await Task.Delay(1000);
            await arg1.SendGroupMessageAsync(arg2.GroupId, "欢迎使用Vtuber-Bot 查看帮助请前往 https://github.com/Mrs4s/VtuberBot");
        }

        private GroupRequestResponse Listener_GroupInviteEvent(HttpApiClient api, GroupRequest request) => true;

        public override void Destroy()
        {
            Program.Listener.GroupInviteEvent -= Listener_GroupInviteEvent;
            Program.Listener.GroupAddedEvent -= Listener_GroupAddedEvent;
        }
    }
}

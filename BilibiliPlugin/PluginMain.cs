using System;
using System.Collections.Generic;
using System.Text;
using BilibiliPlugin.Commands;
using VtuberBot;
using VtuberBot.Plugin;

namespace BilibiliPlugin
{
    public class PluginMain : PluginBase
    {
        public override string Name { get; } = "Bilibili";

        private BilibiliLiveCommand _bilibiliLiveCommand;

        public override void OnLoad()
        {
            _bilibiliLiveCommand = new BilibiliLiveCommand(Program.SendService);
            Program.Bot.Commands.Add(_bilibiliLiveCommand);
            LogInfo("插件加载..");
        }

        public override void Destroy()
        {
            Program.Bot.Commands.Remove(_bilibiliLiveCommand);
            LogInfo("插件卸载..");
        }
    }
}

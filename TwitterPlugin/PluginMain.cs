using System;
using System.Collections.Generic;
using System.Text;
using TwitterPlugin.Commands;
using VtuberBot;
using VtuberBot.Plugin;

namespace TwitterPlugin
{
    public class PluginMain : PluginBase
    {
        public override string Name { get; } = "TwitterPlugin";

        private TwitterCommand _command;

        public override void OnLoad()
        {
            _command=new TwitterCommand(Program.SendService);
            Program.Bot.Commands.Add(_command);
            LogInfo("插件加载..");
        }

        public override void Destroy()
        {
            Program.Bot.Commands.Remove(_command);
            LogInfo("插件卸载..");
        }
    }
}

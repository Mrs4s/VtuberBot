using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using QQ.Framework.Domains;
using VtuberBot.Plugin;

namespace VtuberBot.Robots.Commands
{
    public class PluginManagerCommand : RobotCommandBase
    {
        public override string[] Names { get; } = { "!plugin", "!插件", "！插件" };
        public PluginManagerCommand(ISendMessageService service) : base(service)
        {
        }

        public override void Process(ISendMessageService service, MessageInfo message)
        {
            //Admin
            if (message.UserMember != 1844812067)
            {
                _service.SendToGroup(message.GroupNumber, "您没有权限执行这个命令");
                return;
            }
            base.Process(service, message);
        }

        public override void ShowHelpMessage(MessageInfo message, string[] args)
        {
            _service.SendToGroup(message.GroupNumber, "Plugin command help:" +
                                                     "\r\n!plugin list   --get plugin list" +
                                                     "\r\n!plugin load <FileName>  --load new plugin" +
                                                     "\r\n!plugin unload <PluginName> --unload plugin" +
                                                     "\r\n!plugin reload <PluginName> --reload plugin");
        }

        [RobotCommand(processLength: 3, offset: 1, subCommandName: "load")]
        public void LoadCommand(MessageInfo message, string[] args)
        {
            var pluginPath = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
            var plugin = PluginManager.Manager.LoadPlugin(Path.Combine(pluginPath, args[2]));
            _service.SendToGroup(message.GroupNumber, plugin == null ? "Failed." : "Success.");
        }

        [RobotCommand(processLength: 3, offset: 1, subCommandName: "unload")]
        public void UnloadCommand(MessageInfo message, string[] args)
        {
            PluginManager.Manager.UnloadPlugin(args[2]);
            _service.SendToGroup(message.GroupNumber, "Success.");
        }

        [RobotCommand(processLength: 2, offset: 1, subCommandName: "list")]
        public void ListCommand(MessageInfo message, string[] args)
        {
            _service.SendToGroup(message.GroupNumber,
                string.Join(",", PluginManager.Manager.Plugins.Select(v => v.Name)));
        }
        [RobotCommand(processLength: 3, offset: 1, subCommandName: "reload")]
        public void ReloadCommand(MessageInfo message, string[] args)
        {
            var plugin = PluginManager.Manager.GetPlugin(args[2]);
            if (plugin == null)
            {
                _service.SendToGroup(message.GroupNumber, "Plugin not found.");
                return;
            }
            PluginManager.Manager.UnloadPlugin(plugin);
            PluginManager.Manager.LoadPlugin(plugin.DllPath);
            _service.SendToGroup(message.GroupNumber, "Success.");

        }


    }
}

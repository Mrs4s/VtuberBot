using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using OfflineServer.Lib.Tools;

namespace VtuberBot.Plugin
{
    public class PluginManager
    {
        public List<PluginBase> Plugins { get; } = new List<PluginBase>();


        public void LoadPlugins(string path)
        {

        }

        public void LoadPlugins()
        {

        }

        public void UnloadPlugin(string pluginName)
        {

        }

        public void UnloadPlugin(PluginBase plugin)
        {
            plugin.OnDestroy();
            Plugins.RemoveAll(v => v == plugin);
        }

        public PluginBase LoadPlugin(string dllPath)
        {
            if (!File.Exists(dllPath))
                return null;
            var domain = AppDomain.CreateDomain(StringTools.RandomString);
            var assembly = Assembly.LoadFrom(dllPath);
            var pluginMain = assembly.GetExportedTypes().FirstOrDefault(v => v.BaseType == typeof(PluginBase));
            if (pluginMain == null)
                return null;
            var plugin = Activator.CreateInstance(pluginMain) as PluginBase;
            plugin?.OnLoad();
            return plugin;
        }
    }

    public class PluginLoader
    {

    }
}

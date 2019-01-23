using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using VtuberBot.Tools;

namespace VtuberBot.Plugin
{
    public class PluginManager
    {

        public static PluginManager Manager { get; } = new PluginManager();

        private PluginManager()
        {
        }

        public List<PluginBase> Plugins { get; } = new List<PluginBase>();


        public void LoadPlugins(string path)
        {
            if(!Directory.Exists(path))
                return;
            foreach (var file in Directory.GetFiles(path))
            {
                if (Path.GetExtension(file) == ".dll")
                {
                    var plugin=LoadPlugin(file);
                    if (plugin == null)
                        LogHelper.Error("Cannot load plugin " + file);
                    else
                        LogHelper.Info("Loaded plugin: " + plugin.Name);
                }
            }
        }

        public PluginBase GetPlugin(string pluginName)
        {
            return Plugins.FirstOrDefault(v => v.Name == pluginName);
        }

        public void LoadPlugins()
        {
            var pluginPath = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
            if (!Directory.Exists(pluginPath))
                Directory.CreateDirectory(pluginPath);
            LoadPlugins(pluginPath);
        }

        public void UnloadPlugin(string pluginName)
        {
            var plugin = Plugins.FirstOrDefault(v => v.Name == pluginName);
            if (plugin != null)
                UnloadPlugin(plugin);
        }

        public void UnloadPlugin(PluginBase plugin)
        {
            plugin?.Destroy();
            LogHelper.Info("Destroy plugin: " + plugin?.Name);
            Plugins.RemoveAll(v => v == plugin);
        }

        public PluginBase LoadPlugin(string dllPath)
        {
            try
            {
                if (!File.Exists(dllPath))
                    return null;
                var bytes = File.ReadAllBytes(dllPath);
                var assembly = Assembly.Load(bytes);
                var pluginMain = assembly.GetExportedTypes().FirstOrDefault(v => v.BaseType == typeof(PluginBase));
                if (pluginMain == null)
                    return null;
                var plugin = Activator.CreateInstance(pluginMain) as PluginBase;
                plugin.OnLoad();
                plugin.DllPath = dllPath;
                Plugins.Add(plugin);
                return plugin;
            }
            catch (Exception ex)
            {
                LogHelper.Error("Cannot load plugin " + dllPath, true, ex);
                return null;
            }

        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using VtuberBot.Tools;

namespace VtuberBot.Plugin
{

    public abstract class PluginBase 
    {
        public string DllPath { get; set; }

        public abstract string Name { get; }
        public abstract void OnLoad();
        public abstract void Destroy();


        protected void LogInfo(object obj)
        {
            LogHelper.Info($"[{Name}] {obj}");
        }
    }
}

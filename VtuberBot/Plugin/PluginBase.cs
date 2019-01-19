using System;
using System.Collections.Generic;
using System.Text;

namespace VtuberBot.Plugin
{
    public abstract class PluginBase
    {
        public abstract void OnLoad();

        public abstract void OnDestroy();

        public string Name { get; protected set; }

        public AppDomain PluginDomain { get; set; }
    }
}

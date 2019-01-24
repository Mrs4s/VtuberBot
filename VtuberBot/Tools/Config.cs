using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using OfflineServer.Lib.Tools;
using VtuberBot.Database;
using VtuberBot.Database.Configs;

namespace VtuberBot.Tools
{
    public class Config
    {
        public static Config DefaultConfig => _defaultConfig ?? (_defaultConfig = LoadDefaultConfig());

        private static Config _defaultConfig;

        /// <summary>
        /// 是否使用本地QQ客户端
        /// </summary>
        public bool UseLocalClient { get; set; } = false;

        public string CoolQApi { get; set; } = "http://localhost:5700/";

        public string CoolQAccessToken { get; set; } = "Your access token";

        public string CoolQListenUrl { get; set; } = "http://+:8888/";

        public string ProxyUrl { get; set; }

        public long Id { get; set; }

        public string Password { get; set; }

        public List<VtuberInfo> Vtubers { get; set; } = new List<VtuberInfo>();

        public Dictionary<long, List<SubscribeConfig>> Subscribes { get; set; } = new Dictionary<long, List<SubscribeConfig>>();

        public bool LogGroupMessage { get; set; } = true;

        public string DatabaseConnectionString { get; set; }

        public string YoutubeDataApiKey { get; set; }

        public string TwitterApiKey { get; set; }

        public Dictionary<string, object> PluginConfig { get; set; } = new Dictionary<string, object>();

        public T GetPluginValue<T>(string key)
        {
            if (!PluginConfig.ContainsKey(key))
                return default(T);
            return (T) PluginConfig[key];
        }

        public void SetPluginValue<T>(string key, T value)
        {
            if (PluginConfig.ContainsKey(key))
                PluginConfig[key] = value;
            else
                PluginConfig.Add(key, value);
        }



        public VtuberInfo GetVtuber(string nameOrNickName)
        {
            return Vtubers.FirstOrDefault(v =>
                v.OriginalName.EqualsIgnoreCase(nameOrNickName) || v.ChineseName.EqualsIgnoreCase(nameOrNickName) ||
                v.NickNames.Any(name => name.EqualsIgnoreCase(nameOrNickName)));
        }

        public static Config LoadDefaultConfig()
        {
            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Config.json")))
                File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "Config.json"),
                    JsonConvert.SerializeObject(new Config()));
            return
                JsonConvert.DeserializeObject<Config>(
                    File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Config.json")));
        }
        public static void SaveToDefaultFile(Config config)
        {
            lock (config)
            {
                File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "Config.json"),
                    JsonConvert.SerializeObject(config, Formatting.Indented));
            }
        }
    }
}

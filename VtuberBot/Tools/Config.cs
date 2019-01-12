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

        public long Id { get; set; }

        public string Password { get; set; }

        public List<VtuberInfo> Vtubers { get; set; } = new List<VtuberInfo>();

        public Dictionary<long, List<SubscribeConfig>> Subscribes { get; set; } = new Dictionary<long, List<SubscribeConfig>>();

        public bool LogGroupMessage { get; set; } = true;

        public string DatabaseConnectionString { get; set; }

        public string YoutubeDataApiKey { get; set; }

        public string TwitterApiKey { get; set; }


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

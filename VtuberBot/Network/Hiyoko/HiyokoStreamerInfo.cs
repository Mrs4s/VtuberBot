using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace VtuberBot.Network.Hiyoko
{
    public class HiyokoStreamerInfo
    {

        public string Name { get; set; }

        public HiyokoChannelInfo[] Channels { get; set; }
        
        public string TwitterId { get; set; }
    }

    public class HiyokoChannelInfo
    {
        [JsonProperty("ch_id")]
        public string ChannelId { get; set; }

        [JsonProperty("ch_type")]
        public int ChannelType { get; set; }

        [JsonProperty("name")]
        public string ChannelName { get; set; }

        [JsonProperty("subscriber_count")]
        public int SubscriberCount { get; set; }
    }
}

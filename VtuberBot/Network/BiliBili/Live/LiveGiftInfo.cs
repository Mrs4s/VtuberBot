using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace VtuberBot.Network.BiliBili.Live
{
    public class LiveGiftInfo
    {
        [JsonProperty("giftName")]
        public string GiftName { get; set; }

        [JsonProperty("num")]
        public int Count { get; set; }

        [JsonProperty("uname")]
        public string Username { get; set; }

        [JsonProperty("uid")]
        public long Userid { get; set; }

        [JsonProperty("face")]
        public string FaceLink { get; set; }

        [JsonProperty("coin_type")]
        public string CoinType { get; set; }

        [JsonProperty("total_coin")]
        public long CostCoin { get; set; }
    }
}

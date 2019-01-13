using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace VtuberBot.Database
{
    public class BiliBiliLiveInfo
    {

        [BsonElement("_id")]
        [JsonProperty("_id")]
        public string LiveId { get; set; }

        [BsonElement("title")]
        [JsonProperty("title")]
        public string Title { get; set; }

        [BsonElement("beginTime")]
        [JsonProperty("beginTime")]
        public DateTime BeginTime { get; set; }

        [BsonElement("endTime")]
        [JsonProperty("endTime")]
        public DateTime EndTime { get; set; }

        [BsonElement("maxPopularity")]
        [JsonProperty("maxPopularity")]
        public int MaxPopularity { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }
    }
}

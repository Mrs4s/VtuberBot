using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace VtuberBot.Database
{
    public class BiliBiliLiveInfo
    {

        [BsonElement("_id")]
        public string LiveId { get; set; }

        [BsonElement("title")]
        public string Title { get; set; }

        [BsonElement("beginTime")]
        public DateTime BeginTime { get; set; }

        [BsonElement("endTime")]
        public DateTime EndTime { get; set; }

        [BsonElement("maxPopularity")]
        public int MaxPopularity { get; set; }

        

    }
}

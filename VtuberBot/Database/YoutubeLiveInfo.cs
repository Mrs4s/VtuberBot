using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace VtuberBot.Database
{
    public class YoutubeLiveInfo
    {
        [BsonElement("title")]
        public string Title { get; set; }

        [BsonElement("channelId")]
        public string Channel { get; set; }

        [BsonElement("beginTime")]
        public DateTime BeginTime { get; set; }

        [BsonElement("endTime")]
        public DateTime EndTime { get; set; }

        [BsonElement("_id")]
        public string VideoId { get; set; }
    }
    
}

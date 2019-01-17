using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VtuberBot.Database.Entities
{
    public class BiliBiliCommentInfo
    {
        [BsonElement("_id")]
        public ObjectId Id { get; set; }

        [BsonElement("liveId")]
        public string LiveId { get; set; }

        [BsonElement("type")]
        public DanmakuType Type { get; set; }

        [BsonElement("content")]
        public string Content { get; set; }

        [BsonElement("fromUsername")]
        public string Username { get; set; }

        [BsonElement("fromUserid")]
        public long Userid { get; set; }

        [BsonElement("giftName")]
        public string GiftName { get; set; }

        [BsonElement("giftCount")]
        public int GiftCount { get; set; }

        [BsonElement("costType")]
        public string CostType { get; set; }

        [BsonElement("cost")]
        public long Cost { get; set; }

        [BsonElement("isVip")]
        public bool IsVip { get; set; }

        [BsonElement("isAdmin")]
        public bool IsAdmin { get; set; }
    }

    public enum DanmakuType
    {
        Gift,
        Comment,
        JoinRoom
    }
}

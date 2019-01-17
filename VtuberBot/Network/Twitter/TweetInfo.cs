using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace VtuberBot.Network.Twitter
{
    public class TweetInfo
    {
        [JsonProperty("id")]
        [BsonElement("_id")]
        public long Id { get; set; }

        [JsonProperty("text")]
        [BsonElement("text")]
        public string Content { get; set; }

        [BsonIgnore]
        public DateTime CreateTime => DateTime.ParseExact(_createTime,
            "ddd MMM dd HH:mm:ss zzzz yyyy", CultureInfo.GetCultureInfo("en-us"));

        [JsonProperty("user")]
        [BsonElement("user")]
        public TwitterUser User { get; set; }

        [JsonProperty("is_quote_status")]
        [BsonElement("is_quote")]
        public bool IsQuote { get; set; }

        [JsonProperty("quoted_status")]
        [BsonElement("quoted_tweet")]
        public TweetInfo QuotedTweet { get; set; }

        [JsonProperty("retweeted_status")]
        [BsonElement("retweeted_tweet")]
        public TweetInfo RetweetedTweet { get; set; }

        [BsonIgnore]
        public bool IsReply => _replyUserId != null;

        [BsonIgnore]
        public string ReplyScreenname => _replyScreenName;


        [JsonProperty("in_reply_to_status_id")]
        [BsonElement("replyTweetId")]
        private long? _replyTweetId;  //所回复的TweetId 可NULL 
        [JsonProperty("in_reply_to_user_id")]
        [BsonElement("replyUserId")]
        private long? _replyUserId;   //所回复的UserId 可NULL
        [JsonProperty("quoted_status_id")]
        [BsonElement("quotedTweetId")]
        private long? _quotedTweetId; //所引用的TweetId 可NULL
        [JsonProperty("in_reply_to_screen_name")]
        [BsonElement("replyScreenName")]
        private string _replyScreenName;  //所回复的Screenname 可NULL

        [JsonProperty("created_at")]
        [BsonElement("createTime")]
        private string _createTime;



    }
}

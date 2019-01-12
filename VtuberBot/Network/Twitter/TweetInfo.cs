using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;

namespace VtuberBot.Network.Twitter
{
    public class TweetInfo
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("text")]
        public string Content { get; set; }

        public DateTime CreateTime => DateTime.ParseExact(_createTime,
            "ddd MMM dd HH:mm:ss zzzz yyyy", CultureInfo.GetCultureInfo("en-us"));

        [JsonProperty("user")]
        public TwitterUser User { get; set; }

        [JsonProperty("is_quote_status")]
        public bool IsQuote { get; set; }

        [JsonProperty("quoted_status")]
        public TweetInfo QuotedTweet { get; set; }

        [JsonProperty("retweeted_status")]
        public TweetInfo RetweetedTweet { get; set; }

        public bool IsReply => _replyUserId != null;

        public string ReplyScreenname => _replyScreenName;


        [JsonProperty("in_reply_to_status_id")]
        private long? _replyTweetId;  //所回复的TweetId 可NULL 
        [JsonProperty("in_reply_to_user_id")]
        private long? _replyUserId;   //所回复的UserId 可NULL
        [JsonProperty("quoted_status_id")]
        private long? _quotedTweetId; //所引用的TweetId 可NULL
        [JsonProperty("in_reply_to_screen_name")]
        private string _replyScreenName;  //所回复的Screenname 可NULL

        [JsonProperty("created_at")]
        private string _createTime;



    }
}

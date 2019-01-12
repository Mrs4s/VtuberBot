using System;
using System.Collections.Generic;
using System.Text;

namespace VtuberBot.Database.Configs
{
    public class SubscribeConfig
    {
        public string VtuberName { get; set; }

        public bool PublishTweet { get; set; }

        public bool ReplyTweet { get; set; }

        public bool Retweeted { get; set; }

        public bool BeginLive { get; set; }

        public bool BilibiliBeginLive { get; set; }

        public bool UploadVideo { get; set; }
    }

}

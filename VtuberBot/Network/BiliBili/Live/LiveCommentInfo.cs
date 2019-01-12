using System;
using System.Collections.Generic;
using System.Text;

namespace VtuberBot.Network.BiliBili.Live
{
    public class LiveCommentInfo
    {
        public string Username { get; set; }

        public long Userid { get; set; }

        public string Message { get; set; }

        public bool IsAdmin { get; set; }

        public bool IsVip { get; set; }
    }
}

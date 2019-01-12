using System;
using System.Collections.Generic;
using System.Text;

namespace VtuberBot.Database
{

    public class VtuberInfo
    {
        public string OriginalName { get; set; }

        public string ChineseName { get; set; }

        public string YoutubeChannelId { get; set; }

        public string TwitterProfileId { get; set; }

        public long BilibiliUserId { get; set; }

        public string UserlocalProfile { get; set; }

        public string HiyokoProfileId { get; set; }

        public string YoutubeUploadsPlaylistId { get; set; }

        public List<string> NickNames { get; set; } = new List<string>();


    }
}

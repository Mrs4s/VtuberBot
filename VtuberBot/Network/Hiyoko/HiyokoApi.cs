using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfflineServer.Lib.Network;

namespace VtuberBot.Network.Hiyoko
{
    public class HiyokoApi
    {
        public static List<HiyokoTimelineItem> GetLiveTimeline(string date)
        {
            var client = new MyHttpClient();
            var requestJson = new JObject()
            {
                ["date"] = date,
                ["user_token"] = null
            };
            return JsonConvert.DeserializeObject<List<HiyokoTimelineItem>>(
                JObject.Parse(client.Post("https://hiyoko.sonoj.net/f/avtapi/schedule/fetch_curr",
                    requestJson.ToString(Formatting.None), "application/json"))["schedules"].ToString());
        }

        public static List<HiyokoStreamerInfo> SearchStreamer(string keyword)
        {
            var client = new MyHttpClient();
            var requestJson = new JObject()
            {
                ["user_token"] = null,
                ["keyword"] = keyword,
                ["groups"] = string.Empty,
                ["inc_old_group"] = 0,
                ["retired"] = "all",
                ["following"] = "all",
                ["notifications"] = "all"
            };
            var json = JObject.Parse(client.Post("https://hiyoko.sonoj.net/f/avtapi/search/streamer/fetch",
                requestJson.ToString(Formatting.None), "application/json"));
            if (!json["result"].Any())
                return new List<HiyokoStreamerInfo>();

            return json["result"]
                .Select(token => JObject.Parse(client.Post("https://hiyoko.sonoj.net/f/avtapi/strm/fetch_summary",
                    "{\"streamer_id\":\"" + token["streamer_id"].ToObject<string>() + "\"}", "application/json")))
                .Select(infoJson => new HiyokoStreamerInfo
                {
                    Channels = JsonConvert.DeserializeObject<HiyokoChannelInfo[]>(infoJson["channels"].ToString()),
                    Name = infoJson["streamer"]["name"].ToObject<string>(),
                    TwitterId = infoJson["streamer"]["twitter_id"].ToObject<string>()
                }).ToList();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using OfflineServer.Lib.Network;
using VtuberBot.Tools;

namespace VtuberBot.Network.Twitter
{
    public class TwitterApi
    {
        public static string AccessToken { get; private set; }

        public static List<TweetInfo> GetTimelineByUser(string username,int count=5)
        {
            if(string.IsNullOrEmpty(AccessToken))
                InitAccessToken();
            var client = new MyHttpClient()
            {
                AccessToken = "Bearer " + AccessToken
            };
            return JsonConvert.DeserializeObject<List<TweetInfo>>(client.Get(
                $"https://api.twitter.com/1.1/statuses/user_timeline.json?count={count}&screen_name={username}"));

        }

        public static void InitAccessToken()
        {
            var client = new MyHttpClient()
            {
                AccessToken = "Basic " + Config.DefaultConfig.TwitterApiKey
            };
            var json = JObject.Parse(client.Post("https://api.twitter.com/oauth2/token",
                new Dictionary<string, string>()
                {
                    ["grant_type"] = "client_credentials"
                }));

            AccessToken = json["access_token"].ToObject<string>();

        }

    }
}

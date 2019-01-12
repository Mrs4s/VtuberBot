using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfflineServer.Lib.Network;
using OfflineServer.Lib.Tools;
using VtuberBot.Tools;

namespace VtuberBot.Network.Youtube
{
    public class YoutubeApi
    {


        private static string ApiKey = Config.DefaultConfig.YoutubeDataApiKey;

        //Max return 50 videos...
        public static List<YoutubeVideo> GetVideosByChannelId(string channelId, int count = 5)
        {
            var client = new MyHttpClient();
            var json = JObject.Parse(client.Get(
                $"https://www.googleapis.com/youtube/v3/search?key={ApiKey}&channelId={channelId}&part=snippet,id&order=date&maxResults={count}"));
            var a = json.ToString();

            if (json["items"] == null)
                return new List<YoutubeVideo>();
            var result = (from token in json["items"]
                          where token["id"]["kind"].ToObject<string>() == "youtube#video"
                          let snippet = token["snippet"]
                          select new YoutubeVideo()
                          {
                              Title = snippet["title"].ToObject<string>(),
                              PublishTime = DateTime.Parse(snippet["publishedAt"].ToObject<string>()),
                              ChannelId = snippet["channelId"].ToObject<string>(),
                              VideoId = token["id"]["videoId"].ToObject<string>(),
                              VideoLink = "https://www.youtube.com/watch?v=" + token["id"]["videoId"].ToObject<string>(),
                              ChannelTitle = snippet["channelTitle"].ToObject<string>(),
                              ThumbnailLink = snippet["thumbnails"]["default"]["url"].ToObject<string>(),
                              Description = snippet["description"].ToObject<string>(),
                              IsLive = snippet["liveBroadcastContent"].ToObject<string>() == "live"
                          }).ToList();

            return result;
        }

        public static string GetFirstVideoId(string channelId)
        {
            var html = new MyHttpClient().Get($"https://www.youtube.com/channel/{channelId}/videos?pbj=1", string.Empty);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var node = doc.DocumentNode.SelectSingleNode(
                "//*[@id=\"channels-browse-content-grid\"]/li[1]/div/div[1]/div[2]/h3/a");
            return node.Attributes.FirstOrDefault(v=>v.Name=="href")?.Value.Split('=').Last();
        }

        public static string GetUploadsPlaylistId(string channelId)
        {
            var client = new MyHttpClient();
            var json = JObject.Parse(client.Get(
                $"https://www.googleapis.com/youtube/v3/channels?part=contentDetails&id={channelId}&key={ApiKey}"));
            return json["pageInfo"]["totalResults"].ToObject<int>() == 0
                ? null
                : json["items"].First()["contentDetails"]["relatedPlaylists"]["uploads"].ToObject<string>();
        }

        public static List<YoutubeVideo> GetPlaylistItems(string playListId, int count = 5)
        {
            var client = new MyHttpClient();
            var json = JObject.Parse(client.Get(
                $"https://www.googleapis.com/youtube/v3/playlistItems?part=snippet%2CcontentDetails&maxResults={count}&playlistId={playListId}&key={ApiKey}"));
            var result = (from token in json["items"]
                          let snippet = token["snippet"]
                          select new YoutubeVideo()
                          {
                              Title = snippet["title"].ToObject<string>(),
                              PublishTime = DateTime.Parse(snippet["publishedAt"].ToObject<string>()),
                              ChannelId = snippet["channelId"].ToObject<string>(),
                              VideoId = snippet["resourceId"]["videoId"].ToObject<string>(),
                              VideoLink = "https://www.youtube.com/watch?v=" + snippet["resourceId"]["videoId"].ToObject<string>(),
                              ChannelTitle = snippet["channelTitle"].ToObject<string>(),
                              ThumbnailLink = snippet["thumbnails"]["default"]["url"].ToObject<string>(),
                              Description = snippet["description"].ToObject<string>(),
                              IsLive = false
                          }).ToList();
            return result;
        }

        public static bool NowLive(string channelId)
        {
            var html = new MyHttpClient().Get($"https://www.youtube.com/channel/{channelId}/videos?pbj=1", string.Empty);
            return html.Contains("Live now") || html.Contains("Live ngayon");
        }

        public static YoutubeVideo GetYoutubeVideo(string videoId)
        {
            var client = new MyHttpClient();
            var json = JObject.Parse(client.Get(
                $"https://www.googleapis.com/youtube/v3/videos?id={videoId}&key={ApiKey}&part=liveStreamingDetails,snippet"));
            if (json["pageInfo"]["totalResults"].ToObject<int>() != 1)
                return null;
            var item = json["items"].First();
            var snippet = item["snippet"];
            var result = new YoutubeVideo()
            {
                Title = snippet["title"].ToObject<string>(),
                PublishTime = DateTime.Parse(snippet["publishedAt"].ToObject<string>()),
                ChannelId = snippet["channelId"].ToObject<string>(),
                VideoId = item["id"].ToObject<string>(),
                VideoLink = "https://www.youtube.com/watch?v=" + item["id"].ToObject<string>(),
                ChannelTitle = snippet["channelTitle"].ToObject<string>(),
                ThumbnailLink = snippet["thumbnails"]["default"]["url"].ToObject<string>(),
                Description = snippet["description"].ToObject<string>(),
                IsLive = snippet["liveBroadcastContent"].ToObject<string>() == "live",
            };
            if (item["liveStreamingDetails"] != null)
                result.LiveDetails = item["liveStreamingDetails"].ToObject<LiveStreamingDetail>();
            return result;
        }

        public static YoutubeLiveChatInfo GetLiveChatInfo(string liveChatId)
        {
            var client = new MyHttpClient();
            return JsonConvert.DeserializeObject<YoutubeLiveChatInfo>(client.Get(
                $"https://www.googleapis.com/youtube/v3/liveChat/messages?liveChatId={liveChatId}&part=id%2Csnippet&key={ApiKey}&maxResults=2000"));

        }
    }
}

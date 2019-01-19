using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using OfflineServer.Lib;
using VtuberBot.Network.BiliBili;
using VtuberBot.Network.Twitter;
using VtuberBot.Network.UserLocal;
using VtuberBot.Network.Youtube;
using VtuberBot.Tools;

namespace VtuberBot.Database
{
    public class CacheManager
    {
        public static CacheManager Manager { get; } = new CacheManager();

        private CacheManager()
        {
            GlobalTimer.Interval = 1000 * 30;
        }




        public void Init()
        {
            GlobalTimer.Actions.Add(TwitterCheckTimer);
            GlobalTimer.Actions.Add(UserlocalUpdateTimer);
            new Thread(() =>
            {
                while (true)
                {
                    try
                    {

                        YoutubeVideoCheckTimer();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error("ERROR", true, ex);
                    }

                    Thread.Sleep(1000 * 120);
                }
            }).Start();
            new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        var vtubers = Config.DefaultConfig.Vtubers.Where(v => !string.IsNullOrEmpty(v.YoutubeChannelId));
                        var groupA = new List<VtuberInfo>();
                        var groupB = new List<VtuberInfo>();
                        var num = 1;
                        foreach (var info in vtubers)
                        {
                            switch (num++ % 3)
                            {
                                case 0:
                                    groupA.Add(info);
                                    break;
                                default:
                                    groupB.Add(info);
                                    break;
                            }
                        }
                        new Thread(() => YoutubeLiveCheckTimer(groupA)).Start();
                        YoutubeLiveCheckTimer(groupB);
                        BilibiliLiveCheckTimer();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error("ERROR", true, ex);
                    }
                    Thread.Sleep(1000 * 30);
                }

            }).Start();
        }

        #region Caches
        public Dictionary<VtuberInfo, List<YoutubeVideo>> LastCheckVideos { get; } =
            new Dictionary<VtuberInfo, List<YoutubeVideo>>();

        public Dictionary<VtuberInfo, YoutubeVideo> LastCheckLiveStatus { get; } = new Dictionary<VtuberInfo, YoutubeVideo>();

        public Dictionary<VtuberInfo, BiliBiliUser> LastCheckLiveBStatus { get; } = new Dictionary<VtuberInfo, BiliBiliUser>();


        public List<UserLocalLiveInfo> LastCheckTimeline { get; private set; } = new List<UserLocalLiveInfo>();

        public Dictionary<VtuberInfo, List<TweetInfo>> LastCheckTweets { get; } =
            new Dictionary<VtuberInfo, List<TweetInfo>>();


        public Dictionary<VtuberInfo, DateTime> LastTweetTime { get; } = new Dictionary<VtuberInfo, DateTime>();

        public Dictionary<VtuberInfo, DateTime> LastReplyTime { get; } = new Dictionary<VtuberInfo, DateTime>();

        public Dictionary<VtuberInfo, DateTime> LastRetweetedTime { get; } = new Dictionary<VtuberInfo, DateTime>();


        #endregion


        #region Events

        public event Action<VtuberInfo, YoutubeVideo> VtuberBeginLiveEvent;

        public event Action<VtuberInfo, BiliBiliUser> VtuberBeginLiveBilibiliEvent;

        public event Action<VtuberInfo, YoutubeVideo> VtuberUploadVideoEvent;

        public event Action<VtuberInfo, TweetInfo> VtuberPublishTweetEvent;

        public event Action<VtuberInfo, TweetInfo> VtuberReplyTweetEvent;

        public event Action<VtuberInfo, TweetInfo> VtuberRetweetedEvent;

        #endregion



        private void TwitterCheckTimer()
        {
            foreach (var vtuber in Config.DefaultConfig.Vtubers.ToArray().Where(v => v.TwitterProfileId != null))
            {
                var timeline = TwitterApi.GetTimelineByUser(vtuber.TwitterProfileId, 10);
                if (!LastCheckTweets.ContainsKey(vtuber))
                    LastCheckTweets.Add(vtuber, timeline);
                LastCheckTweets[vtuber] = timeline;
                var lastRetweeted = timeline.FirstOrDefault(v => v.RetweetedTweet != null);
                var lastPublish = timeline.FirstOrDefault(v => v.RetweetedTweet == null && !v.IsReply && !v.IsQuote);
                var lastReply = timeline.FirstOrDefault(v => v.IsReply);
                //Update cache
                if (lastPublish != null)
                {
                    if (!LastTweetTime.ContainsKey(vtuber))
                        LastTweetTime.Add(vtuber, lastPublish.CreateTime);
                    if (LastTweetTime[vtuber] != lastPublish.CreateTime)
                    {
                        LastTweetTime[vtuber] = lastPublish.CreateTime;
                        VtuberPublishTweetEvent?.Invoke(vtuber, lastPublish);
                    }
                }

                if (lastReply != null)
                {
                    if (!LastReplyTime.ContainsKey(vtuber))
                        LastReplyTime.Add(vtuber, lastReply.CreateTime);
                    if (LastReplyTime[vtuber] != lastReply.CreateTime)
                    {
                        LastReplyTime[vtuber] = lastReply.CreateTime;
                        VtuberReplyTweetEvent?.Invoke(vtuber, lastReply);
                    }
                }

                if (lastRetweeted != null)
                {
                    if (!LastRetweetedTime.ContainsKey(vtuber))
                        LastRetweetedTime.Add(vtuber, lastRetweeted.CreateTime);
                    if (LastRetweetedTime[vtuber] != lastRetweeted.CreateTime)
                    {
                        LastRetweetedTime[vtuber] = lastRetweeted.CreateTime;
                        VtuberRetweetedEvent?.Invoke(vtuber, lastRetweeted);
                    }
                }
            }
        }

        private void YoutubeVideoCheckTimer()
        {
            foreach (var vtuber in Config.DefaultConfig.Vtubers.ToArray().Where(v =>
                !string.IsNullOrEmpty(v.YoutubeChannelId) && string.IsNullOrEmpty(v.YoutubeUploadsPlaylistId)))
            {
                vtuber.YoutubeUploadsPlaylistId = YoutubeApi.GetUploadsPlaylistId(vtuber.YoutubeChannelId);
                Config.SaveToDefaultFile(Config.DefaultConfig);
                LogHelper.Info($"更新Vtuber {vtuber.OriginalName} Upload playlist id.");
            }
            foreach (var vtuber in Config.DefaultConfig.Vtubers.Where(v => !string.IsNullOrEmpty(v.YoutubeUploadsPlaylistId)))
            {
                var videos = YoutubeApi.GetPlaylistItems(vtuber.YoutubeUploadsPlaylistId, 10);
                if (!LastCheckVideos.ContainsKey(vtuber))
                    LastCheckVideos.Add(vtuber, videos);
                if (LastCheckVideos[vtuber].FirstOrDefault()?.VideoId != videos.FirstOrDefault()?.VideoId)
                    VtuberUploadVideoEvent?.Invoke(vtuber, videos.First());
                LastCheckVideos[vtuber] = videos;
            }
        }

        private void YoutubeLiveCheckTimer(List<VtuberInfo> vtubers)
        {
            foreach (var vtuber in vtubers)
            {
                if (!LastCheckLiveStatus.ContainsKey(vtuber))
                    LastCheckLiveStatus.Add(vtuber, new YoutubeVideo());
                var nowLive = YoutubeApi.NowLive(vtuber.YoutubeChannelId);
                if (nowLive && !LastCheckLiveStatus[vtuber].IsLive)
                {
                    var live = YoutubeApi.GetVideosByChannelId(vtuber.YoutubeChannelId).FirstOrDefault(v => v.IsLive);
                    if (live == null)
                    {
                        var liveId = YoutubeApi.GetLiveVideoId(vtuber.YoutubeChannelId);
                        if (liveId == null)
                        {
                            LogHelper.Error("Error: liveId is null", false);
                            continue;
                        }
                        live = YoutubeApi.GetYoutubeVideo(liveId);
                        if (!live.IsLive)
                            continue;
                    }
                    if (!LastCheckLiveStatus[vtuber].IsLive)
                        VtuberBeginLiveEvent?.Invoke(vtuber, live);
                    LastCheckLiveStatus[vtuber] = live;
                }
                if (!nowLive)
                    LastCheckLiveStatus[vtuber] = new YoutubeVideo();
            }
        }

        private void BilibiliLiveCheckTimer()
        {
            foreach (var vtuber in Config.DefaultConfig.Vtubers.Where(v => v.BilibiliUserId != default(long)))
            {
                var bUser = BiliBiliApi.GetBiliBiliUser(vtuber.BilibiliUserId);
                if (bUser != null)
                {
                    if (!LastCheckLiveBStatus.ContainsKey(vtuber))
                        LastCheckLiveBStatus.Add(vtuber, new BiliBiliUser());
                    if (!LastCheckLiveBStatus[vtuber].AreLive && bUser.AreLive)
                        VtuberBeginLiveBilibiliEvent?.Invoke(vtuber, bUser);
                    LastCheckLiveBStatus[vtuber] = bUser;
                }
            }
        }

        private void BilibiliVideoCheckTimer()
        {
            foreach (var vtuber in Config.DefaultConfig.Vtubers.Where(v => v.BilibiliUserId != default(long)))
            {

            }
        }

        public void UserlocalUpdateTimer()
        {
            //LastCheckTimeline = UserLocalApi.GetTimeLine();
        }
    }
}

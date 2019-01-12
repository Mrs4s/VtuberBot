using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MongoDB.Driver;
using VtuberBot.Database;
using VtuberBot.Network.Youtube;
using VtuberBot.Tools;

namespace VtuberBot.Robots
{
    public class LiveChatRecorder
    {
        public string LiveChatId { get; }

        public string VideoId { get; }

        public VtuberInfo Vtuber { get; set; }


        public List<YoutubeLiveChat> RecordedComments { get; } = new List<YoutubeLiveChat>();

        private readonly IMongoCollection<YoutubeLiveChat> _chatCollection;


        public event Action<string, LiveChatRecorder> LiveStoppedEvent; 


        public LiveChatRecorder(string liveChatId, VtuberInfo vtuber , string videoId)
        {
            LiveChatId = liveChatId;
            Vtuber = vtuber;
            VideoId = videoId;
            _chatCollection = Program.Database.GetCollection<YoutubeLiveChat>("youtube-live-chats");
        }

        public void StartRecord()
        {
            var num = 0;
            new Thread(() =>
            {
                while (true)
                {
                    var info = YoutubeApi.GetLiveChatInfo(LiveChatId);
                    if (info.PollingInterval == 0 && info.CommentsToken == null)
                    {
                        LiveStoppedEvent?.Invoke(LiveChatId, this);
                        break;
                    }
                    var comments = info.GetComments().Where(comment => RecordedComments.All(v => v.CommentId != comment.CommentId)).ToList();
                    if (comments.Any())
                    {
                        comments.ForEach(v => v.VideoId = VideoId);
                        try
                        {
                            _chatCollection.InsertMany(comments,new InsertManyOptions()
                            {
                                IsOrdered = false
                            });
                            RecordedComments.AddRange(comments);
                        }
                        catch (MongoBulkWriteException)
                        {
                            Thread.Sleep(10000);
                        }

                    }
                    if (++num % 20 == 0)
                    {
                        if (!CacheManager.Manager.LastCheckLiveStatus[Vtuber].IsLive)
                        {
                            LiveStoppedEvent?.Invoke(LiveChatId, this);
                            break;
                        }
                    }
                    Thread.Sleep(info.PollingInterval);
                }
            }).Start();
        }
    }
}

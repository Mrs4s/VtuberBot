using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using MongoDB.Bson;
using MongoDB.Driver;
using VtuberBot.Database;
using VtuberBot.Network.BiliBili.Live;
using VtuberBot.Tools;

namespace VtuberBot.Robots
{
    public class DanmakuRecorder
    {
        public long RoomId { get;  }

        public LiveClient Client { get; private set; }

        public bool StillLive { get; set; }

        public string LiveId { get; set; }

        public VtuberInfo Vtuber { get; }

        public event Action<DanmakuRecorder> LiveStoppedEvent;

        private readonly IMongoCollection<BiliBiliCommentInfo> _danmakuCollection;



        public DanmakuRecorder(long roomId,string liveId,VtuberInfo vtuber)
        {
            RoomId = roomId;
            LiveId = liveId;
            Vtuber = vtuber;
            _danmakuCollection = Program.Database.GetCollection<BiliBiliCommentInfo>("bili-live-comments");
        }

        public void StartRecord()
        {
            var num = 0;
            Client = new LiveClient(RoomId);
            if (!Client.ConnectAsync().GetAwaiter().GetResult())
            {
                LogHelper.Error("Cannot connect to danmaku server.");
                return;
            }

            StillLive = true;
            Client.GotGiftEvent += GotGiftEvent;
            Client.GotDanmuEvent += GotDanmakuEvent;
            Client.SocketDisconnectEvent += client =>
            {
                if (StillLive)
                    client.ConnectAsync().GetAwaiter().GetResult();
            };
            Client.LiveStoppedEvent += client =>
            {
                StillLive = false;
                client.CloseConnect();
                LiveStoppedEvent?.Invoke(this);
            };
            new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(10000);
                    if (!CacheManager.Manager.LastCheckLiveBStatus[Vtuber].AreLive)
                    {
                        StillLive = false;
                        Client.CloseConnect();
                        LiveStoppedEvent?.Invoke(this);
                        break;
                    }
                }
            }).Start();
        }

        private void GotDanmakuEvent(LiveCommentInfo info)
        {
            var danmaku = new BiliBiliCommentInfo()
            {
                Id = ObjectId.GenerateNewId(DateTime.Now),
                Type = DanmakuType.Comment,
                Username = info.Username,
                Userid = info.Userid,
                Content = info.Message,
                IsVip = info.IsVip,
                IsAdmin = info.IsAdmin,
                LiveId = LiveId
            };
            try
            {
                _danmakuCollection.InsertOne(danmaku);
            }
            catch(MongoBulkWriteException ex)
            {
                LogHelper.Error("Insert object error.", true, ex);
            }
        }

        private void GotGiftEvent(LiveGiftInfo info)
        {
            var danmaku = new BiliBiliCommentInfo()
            {
                Id = ObjectId.GenerateNewId(DateTime.Now),
                Type = DanmakuType.Gift,
                Username = info.Username,
                Userid = info.Userid,
                GiftName = info.GiftName,
                GiftCount = info.Count,
                CostType = info.CoinType,
                Cost = info.CostCoin,
                LiveId = LiveId
            };
            try
            {
                _danmakuCollection.InsertOne(danmaku);
            }
            catch (MongoBulkWriteException ex)
            {
                LogHelper.Error("Insert object error.", true, ex);
            }
        }
    }
}

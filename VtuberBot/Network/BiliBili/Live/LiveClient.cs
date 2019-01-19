using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfflineServer.Lib.Network;
using QQ.Framework.Utils;
using VtuberBot.Tools;
using Formatting = Newtonsoft.Json.Formatting;

namespace VtuberBot.Network.BiliBili.Live
{
    public class LiveClient
    {
        public long RoomId { get; }

        public string DanmuServer { get; private set; }

        public int Popularity { get; private set; }

        public int MaxPopularity { get; private set; }

        public ClientWebSocket WebSocket { get; private set; }



        #region Events

        public event Action<LiveGiftInfo> GotGiftEvent;

        public event Action<LiveCommentInfo> GotDanmuEvent;

        public event Action<LiveClient> SocketDisconnectEvent;

        public event Action<LiveClient> LiveStoppedEvent;
        #endregion



        public LiveClient(long roomId)
        {
            RoomId = roomId;

        }

        public void InitDanmuServer()
        {
            var client = new MyHttpClient();
            var xml = client.Get("https://live.bilibili.com/api/player?id=cid:" + RoomId);
            xml = $"<root>{xml}</root>";
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            DanmuServer = doc["root"]["dm_host_list"].InnerText.Split(',').First();
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                WebSocket = new ClientWebSocket();
                if (string.IsNullOrEmpty(DanmuServer))
                    InitDanmuServer();
                await WebSocket.ConnectAsync(new Uri($"wss://{DanmuServer}/sub"), CancellationToken.None);
                await SendJoinPacketAsync();
                new Thread(async () =>
                {
                    while (WebSocket.State == WebSocketState.Open)
                    {
                        try
                        {
                            await SendHeartbeatPacketAsync();
                            await Task.Delay(1000 * 30);
                        }
                        catch
                        {
                            //TODO: process this
                        }
                    }

                    SocketDisconnectEvent?.Invoke(this);
                }).Start();
                BeginProcessPacket();
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error($"Connect to danmu server {DanmuServer} error.", true, ex);
                return false;
            }
        }

        public void CloseConnect()
        {
            try
            {
                if (WebSocket.State == WebSocketState.Open)
                {
                    WebSocket.CloseAsync(WebSocketCloseStatus.Empty, null, CancellationToken.None).GetAwaiter()
                        .GetResult();
                }
            }
            catch
            {
                //
            }

        }

        private void BeginProcessPacket()
        {
            new Thread(async () =>
            {
                while (WebSocket.State == WebSocketState.Open)
                {
                    try
                    {
                        var buffer = new byte[40960];  //Fucking kimo风暴
                        var result = await WebSocket.ReceiveAsync(buffer, CancellationToken.None);
                        var bytes = new byte[result.Count];
                        Array.Copy(buffer, bytes, result.Count);
                        using (var memory = new MemoryStream(bytes))
                        using (var reader = new BinaryReader(memory))
                        {
                            while (memory.Position < memory.Length)
                            {
                                var length = reader.BeReadInt32();  //packet length
                                reader.BeReadUInt16();  //header length
                                reader.BeReadUInt16();   //protocol version
                                var operation = (LiveOperationEnum)reader.BeReadInt32();
                                reader.ReadInt32(); // Sequence Id
                                var playLoad = reader.ReadBytes(length - 16);
                                switch (operation)
                                {
                                    case LiveOperationEnum.COMMAND:
                                        ProcessCommand(playLoad);
                                        break;
                                    case LiveOperationEnum.POPULARITY:
                                        Popularity = (playLoad[0] << 24) | (playLoad[1] << 16) | (playLoad[2] << 8) | playLoad[3];
                                        if (Popularity > MaxPopularity)
                                            MaxPopularity = Popularity;
                                        break;
                                    case LiveOperationEnum.RECEIVE_HEART_BEAT:
                                        break;
                                    default:
                                        LogHelper.Error("Unknown operation id " + (int)operation);
                                        break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error("Process danmu packet error.", true, ex);
                    }
                }
            }).Start();
        }

        private void ProcessCommand(byte[] playLoad)
        {

            var json = JObject.Parse(Encoding.UTF8.GetString(playLoad));
            var cmd = json["cmd"].ToObject<string>();
            switch (cmd)
            {
                case "SEND_GIFT":
                    var giftInfo = json["data"].ToObject<LiveGiftInfo>();
                    GotGiftEvent?.Invoke(giftInfo);
                    break;
                case "DANMU_MSG":
                    var info = new LiveCommentInfo()
                    {
                        Username = json["info"][2][1].ToString(),
                        Userid = json["info"][2][0].ToObject<long>(),
                        Message = json["info"][1].ToObject<string>(),
                        IsAdmin = json["info"][2][2].ToString() == "1",
                        IsVip = json["info"][2][3].ToString() == "1"
                    };
                    GotDanmuEvent?.Invoke(info);
                    break;
                default:
                    //Console.WriteLine(json.ToString(Formatting.Indented));
                    break;
            }
        }

        private async Task SendHeartbeatPacketAsync()
        {
            await WebSocket.SendAsync(Pack(string.Empty, LiveOperationEnum.SEND_HEART_BEAT), WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        private async Task SendJoinPacketAsync()
        {
            var join = new
            {
                clientver = "1.5.14",
                platform = "web",
                protover = 1,
                roomid = RoomId,
                uid = 0
            };
            await WebSocket.SendAsync(Pack(join, LiveOperationEnum.AUTH_JOIN), WebSocketMessageType.Binary, true,
                CancellationToken.None);
        }



        private byte[] Pack(object obj, LiveOperationEnum operation)
        {
            var playLoad = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj, Formatting.None));
            using (var memory = new MemoryStream())
            using (var writer = new BinaryWriter(memory))
            {
                writer.BeWrite(playLoad.Length + 16);
                writer.BeUshortWrite(16);
                writer.BeUshortWrite(1);
                writer.BeWrite((int)operation);
                writer.BeWrite(1);
                writer.Write(playLoad);
                return memory.GetBuffer();
            }
        }
    }

    public enum LiveOperationEnum
    {
        SEND_HEART_BEAT = 2,
        POPULARITY = 3,
        COMMAND = 5,
        AUTH_JOIN = 7,
        RECEIVE_HEART_BEAT = 8
    }
}

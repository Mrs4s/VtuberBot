using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QQ.Framework;
using QQ.Framework.Domains;
using QQ.Framework.Sockets;
using VtuberBot.Database;
using VtuberBot.Network.BiliBili;
using VtuberBot.Network.BiliBili.Live;
using VtuberBot.Network.Hiyoko;
using VtuberBot.Network.Twitter;
using VtuberBot.Network.UserLocal;
using VtuberBot.Network.Youtube;
using VtuberBot.Robots;
using VtuberBot.Robots.Commands;
using VtuberBot.Tools;

namespace VtuberBot
{
    class Program
    {
        public static Robots.VtuberBot Bot { get; private set; }

        public static QQUser User { get; private set; }

        public static IMongoDatabase Database;


        static void Main(string[] args)
        {
            Database = new MongoClient(Config.DefaultConfig.DatabaseConnectionString).GetDatabase("vtuber-bot-data");
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            LogHelper.Info("登录中..");
            User = new QQUser(Config.DefaultConfig.Id, Config.DefaultConfig.Password);
            var socketServer = new SocketServiceImpl(User);
            var transponder = new Transponder();
            var sendService = new SendMessageServiceImpl(socketServer, User);
            var manage = new MessageManage(socketServer, User, transponder);
            manage.Init();
            QQGlobal.DebugLog = false;
            while (string.IsNullOrEmpty(User.NickName))
                Thread.Sleep(100);
            LogHelper.Info("登录完成");
            LogHelper.Info("载入缓存中");
            CacheManager.Manager.Init();
            Thread.Sleep(10000);
            LogHelper.Info("载入完成");
            Bot = new Robots.VtuberBot(sendService, transponder, User);
            Bot.Commands.Add(new MenuCommand());
            Bot.Commands.Add(new TimeLineCommand());
            Bot.Commands.Add(new OfficeInfoCommand());
            Bot.Commands.Add(new YoutubeSearchCommand());
            Bot.Commands.Add(new VtuberInfoCommand(sendService));
            Bot.Commands.Add(new SubscribeCommand());
            Bot.Commands.Add(new LiveCommand(sendService));
            Console.ReadLine();
        }
    }
}

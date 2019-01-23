using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
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
using QQ.Framework.Utils;
using Sisters.WudiLib;
using Sisters.WudiLib.Posts;
using VtuberBot.Database;
using VtuberBot.Network.BiliBili;
using VtuberBot.Network.BiliBili.Live;
using VtuberBot.Network.Hiyoko;
using VtuberBot.Network.Twitter;
using VtuberBot.Network.UserLocal;
using VtuberBot.Network.Youtube;
using VtuberBot.Plugin;
using VtuberBot.Robots;
using VtuberBot.Robots.Commands;
using VtuberBot.Tools;

namespace VtuberBot
{
    public class Program
    {
        public static LocalVtuberBot Bot { get; private set; }
        #region LocalQQClient
        public static QQUser User { get; private set; }
        #endregion

        #region CoolQClient

        public static HttpApiClient Client { get; private set; }
        public static ApiPostListener Listener { get; private set; }

        #endregion




        public static IMongoDatabase Database;

        public static ISendMessageService SendService;


        static void Main(string[] args)
        {
            Database = new MongoClient(Config.DefaultConfig.DatabaseConnectionString).GetDatabase("vtuber-bot-data");
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            if (!string.IsNullOrEmpty(Config.DefaultConfig.ProxyUrl))
                WebRequest.DefaultWebProxy = new WebProxy(Config.DefaultConfig.ProxyUrl);
            LogHelper.Info("载入中..");
            if (!Config.DefaultConfig.UseLocalClient)
            {
                LogHelper.Info("使用CoolQ HTTP接口启动机器人");
                Client = new HttpApiClient()
                {
                    ApiAddress = Config.DefaultConfig.CoolQApi,
                    AccessToken = Config.DefaultConfig.CoolQAccessToken
                };
                Listener = new ApiPostListener()
                {
                    ApiClient = Client,
                    PostAddress = Config.DefaultConfig.CoolQListenUrl,
                };
                Listener.StartListen();
                var service = new CoolService();
                SendService = service;
                Bot = new LocalVtuberBot(SendService, service, null);
            }
            else
            {
                LogHelper.Info("使用本地QQ框架启动机器人");
                User = new QQUser(Config.DefaultConfig.Id, Config.DefaultConfig.Password);
                var socketServer = new SocketServiceImpl(User);
                var transponder = new Transponder();
                SendService = new SendMessageServiceImpl(socketServer, User);
                var manage = new MessageManage(socketServer, User, transponder);
                manage.Init();
                QQGlobal.DebugLog = false;
                while (string.IsNullOrEmpty(User.NickName))
                    Thread.Sleep(100);
                Bot = new LocalVtuberBot(SendService, transponder, User);
            }
            LogHelper.Info("载入完成");
            LogHelper.Info("载入缓存中");
            CacheManager.Manager.Init();
            Thread.Sleep(10000);
            LogHelper.Info("载入完成");
            Bot.Commands.Add(new MenuCommand());
            Bot.Commands.Add(new TimeLineCommand());
            Bot.Commands.Add(new OfficeInfoCommand());
            Bot.Commands.Add(new YoutubeSearchCommand());
            Bot.Commands.Add(new VtuberInfoCommand(SendService));
            Bot.Commands.Add(new SubscribeCommand());
            Bot.Commands.Add(new LiveCommand(SendService));
            Bot.Commands.Add(new PluginManagerCommand(SendService));
            LogHelper.Info("载入插件中...");
            PluginManager.Manager.LoadPlugins();
            LogHelper.Info("载入完成.");
            Console.ReadLine();
        }
    }
}

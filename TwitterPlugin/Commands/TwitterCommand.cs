using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using QQ.Framework.Domains;
using VtuberBot;
using VtuberBot.Network.Twitter;
using VtuberBot.Robots;
using VtuberBot.Tools;

namespace TwitterPlugin.Commands
{
    public class TwitterCommand : RobotCommandBase
    {
        public override string[] Names { get; } = { "!推特","！推特","!twitter"};

        private IMongoCollection<TweetInfo> _tweetCollection;
        public TwitterCommand(ISendMessageService service) : base(service)
        {
            _tweetCollection = Program.Database.GetCollection<TweetInfo>("tweet-details");
        }
        
        [RobotCommand(3,1,"统计")]
        public void TwitterInfoCommand(MessageInfo message,string[] args)
        {
            var vtuber = Config.DefaultConfig.GetVtuber(args[2]);
            if (vtuber == null)
            {
                _service.SendToGroup(message.GroupNumber, "数据库中不存在" + args[2]);
                return;
            }

            var tweets = _tweetCollection.FindAsync(v => v.User.ScreenName == vtuber.TwitterProfileId).GetAwaiter().GetResult().ToList();
            _service.SendToGroup(message.GroupNumber,$" 关于 {vtuber.OriginalName} 的推特统计:" +
                                                     $"\r\n数据库记录的推特数: {tweets.Count}" +
                                                     $"\r\n回复数: {tweets.Count(v=>v.IsReply)}" +
                                                     $"\r\n待续.");
        }

        [RobotCommand(2, 1, "Detail")]
        public void TwitterDetailCommand(MessageInfo message, string[] args)
        {
            
        }
        


        public override void ShowHelpMessage(MessageInfo message, string[] args)
        {
            _service.SendToGroup(message.GroupNumber,"推特相关统计命令：" +
                                                     "\r\n!Twitter 统计 <Vtuber> 查询发推统计" +
                                                     "\r\n!Twitter 历史 <Vtuber> 查询发推历史" +
                                                     "\r\n!Twitter Detail   查询推特统计");
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using VtuberBot.Network.Youtube;

namespace VtuberBot.Database
{
    //TODO: 暂时使用 File System 作为 Database 方便分析， 一般 LiveChat 的评论数不会太多 暂且不会出现瓶颈， 抽空换成MongoDB
    public class CommentDatabase
    {
        //目录结构: {DataPath}/{Channel Id}/{Video Id}.json
        public string DataPath { get; }

        public CommentDatabase(string dataPath)
        {
            DataPath = dataPath;
        }

        public void SaveLiveChatComments(string channelId, string videoId, List<YoutubeLiveChat> comments)
        {
            if (!Directory.Exists(DataPath))
                Directory.CreateDirectory(DataPath);
            if (!Directory.Exists(Path.Combine(DataPath, channelId)))
                Directory.CreateDirectory(Path.Combine(DataPath, channelId));
            File.WriteAllText(Path.Combine(DataPath, channelId, videoId + ".json"),
                JsonConvert.SerializeObject(comments));
        }

        public YoutubeLiveChat[] GetCommentsByVideo(string channelId, string videoId)
        {
            if (!File.Exists(Path.Combine(DataPath, channelId, videoId + ".json")))
                return new YoutubeLiveChat[0];
            return JsonConvert.DeserializeObject<YoutubeLiveChat[]>(
                File.ReadAllText(Path.Combine(DataPath, channelId, videoId + ".json")));
        }
        /*
        public string[] GetLiveHistory(string channelId)
        {
            var path = Path.Combine(DataPath, channelId);
            if (!Directory.Exists(path))
                return null;
            return Directory.GetFiles(path).Select(Path.GetFileNameWithoutExtension).ToArray();
        }
        */
        public List<YoutubeLiveInfo> GetLiveHistory(string channelId)
        {
            var path = Path.Combine(DataPath, channelId, "history.json");
            if (!File.Exists(path))
                return new List<YoutubeLiveInfo>();
            return JsonConvert.DeserializeObject<List<YoutubeLiveInfo>>(File.ReadAllText(path))
                .OrderBy(v => v.BeginTime).ToList();
        }

        public void AddChannelLiveInfo(YoutubeLiveInfo info)
        {
            var history = GetLiveHistory(info.Channel);
            if (history.Any(v => v.VideoId == info.VideoId))
                return;
            history.Add(info);
            File.WriteAllText(Path.Combine(DataPath, info.Channel, "history.json"),
                JsonConvert.SerializeObject(history));
        }


    }
}

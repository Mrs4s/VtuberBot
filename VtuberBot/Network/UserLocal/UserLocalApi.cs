using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using OfflineServer.Lib.Network;
using VtuberBot.Tools;

namespace VtuberBot.Network.UserLocal
{
    public class UserLocalApi
    {
        public static List<UserLocalLiveInfo> GetTimeLine()
        {
            var client = new MyHttpClient();
            var text = client.Get("http://virtual-youtuber.userlocal.jp/");
            var html = new HtmlDocument();
            html.LoadHtml(text);
            var body = html.DocumentNode.SelectNodes("html").First().SelectNodes("body").First();
            var listNode = body.SelectSingleNode("/html/body/div[2]/div[2]/div[3]/table").ChildNodes.Where(v => v.Name == "tr");
            var time = string.Empty;
            var result = new List<UserLocalLiveInfo>();
            foreach (var node in listNode)
            {
                if (node.GetAttributeValue("class") == "text-white bg-dark")
                {
                    time = node.InnerText.Trim();
                    time = time.Substring(0, time.IndexOf('('));
                    continue;
                }

                var subTime = time;
                var subNodes = node.ChildNodes.Where(v => v.Name == "td");
                subTime += subNodes.First().InnerText.Trim();
                subNodes = subNodes.Last().ChildNodes.Where(v => v.Name == "div");
                foreach (var subNode in subNodes)
                {
                    var dateTime = Convert.ToDateTime(subTime+ subNode.ChildNodes.First(v => v.Name == "div").InnerText.Trim(), new DateTimeFormatInfo()
                    {
                        ShortDatePattern = "MM月dd日hh時mm分"
                    });
                    dateTime += new TimeSpan(1, 0, 0);
                    var spans = subNode.ChildNodes.Last(v => v.Name == "div").ChildNodes.Where(v => v.Name == "span");
                    result.AddRange(from span in spans
                        select span.InnerText.Trim()
                        into titleOrigin
                        let liveTitle = titleOrigin.Substring(0, titleOrigin.IndexOf("(")==-1 ? titleOrigin.Length : titleOrigin.IndexOf("(")).Trim()
                        let vtuberName =
                            titleOrigin.Substring(titleOrigin.IndexOf("(") + 1,
                                titleOrigin.Length - titleOrigin.IndexOf("(") - 2)
                        select new UserLocalLiveInfo() {LiveTime = dateTime, Title = liveTitle, VTuber = vtuberName});
                }

            }
            return result;
        }

        public static UserLocalOfficeInfo GetOfficeInfo(string officeName)
        {
            var client = new MyHttpClient();
            var text = client.Get("http://virtual-youtuber.userlocal.jp/office/" + officeName);
            var html = new HtmlDocument();
            html.LoadHtml(text);
            var body = html.DocumentNode.SelectNodes("html").First().SelectNodes("body").First();
            var channelCount = body.SelectSingleNode("/html/body/div[2]/div[3]/div/div[2]/div/div[3]/span[2]")
                .InnerText;
            var fanCount = body.SelectSingleNode("/html/body/div[2]/div[3]/div/div[2]/div/div[4]/span[2]").InnerText;
            var avgFanCount = body.SelectSingleNode("/html/body/div[2]/div[3]/div/div[2]/div/div[5]/span[2]").InnerText;
            var officeDisPlayName = body.SelectSingleNode("/html/body/div[2]/div[3]/div/div[2]/div/div[1]/div[2]/h2")
                .InnerText;
            return new UserLocalOfficeInfo()
            {
                OfficeName = officeDisPlayName,
                ChannelCount = channelCount,
                TotalFanCount = fanCount,
                AvgFanCount = avgFanCount
            };

        }
    }
}

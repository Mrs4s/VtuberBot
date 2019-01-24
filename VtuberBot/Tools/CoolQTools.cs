using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QQ.Framework;
using QQ.Framework.Domains;
using QQ.Framework.HttpEntity;
using QQ.Framework.Utils;
using Sisters.WudiLib;

namespace VtuberBot.Tools
{
    public static class CoolQTools
    {
        public static void SendImageToGroup(this ISendMessageService service, long groupId, Image image)
        {
            using (var memory = new MemoryStream())
            {
                image.Save(memory, ImageFormat.Jpeg);
                service.SendImageToGroup(groupId, memory.GetBuffer());
            }
        }

        public static void SendImageToGroup(this ISendMessageService service, long groupId, byte[] bytes)
        {
            var image = new TextSnippet()
            {
                Type = MessageType.Picture
            };
            image.Set("data", bytes);
            service.SendToGroup(groupId, image);
        }

        public static GroupItem[] GetGroupList(this HttpApiClient apiClient)
        {
            using (var client = new HttpClient())
            {
                var json = JObject.Parse(client.GetStringAsync(apiClient.ApiAddress + "get_group_list?access_token=" +
                                                 apiClient.AccessToken).GetAwaiter().GetResult());
                if(json.Value<int>("retcode")!=0)
                    return new GroupItem[0];
                return json["data"].Select(token => new GroupItem
                {
                    Gc = token["group_id"].ToObject<long>(),
                    Gn = token["group_name"].ToString()
                }).ToArray();
            }
        }

        public static async Task SendGroupMessageLimitedAsync(this HttpApiClient apiClient,long groupId,string message)
        {
            using (var client = new HttpClient())
            {
                await client.PostAsync($"{apiClient.ApiAddress}send_group_msg_rate_limited?access_token={apiClient.AccessToken}",
                    new StringContent(JsonConvert.SerializeObject(new
                    {
                        group_id = groupId,
                        message = message,
                        auto_escape = true
                    }),Encoding.UTF8, "application/json"));
            }
        }


    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HookappServer
{
    public class FirebaseHandler
    {
        public static void GenerateFirebaseRequest(string senderName,string deviceID)
        {
            WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
            tRequest.Method = "post";
            tRequest.ContentType = "application/json";
            var objNotification = new
            {
                data = new
                {
                    name = senderName
                },
                priority = "high",
                to = deviceID
            };
            string jsonNotificationFormat = Newtonsoft.Json.JsonConvert.SerializeObject(objNotification);

            Byte[] byteArray = Encoding.UTF8.GetBytes(jsonNotificationFormat);
            tRequest.Headers.Add(string.Format("Authorization: key={0}", "AAAA2h5tFRY:APA91bG3wtsc_5zfuJm57ij9mhTVIh1hsEIFA0UvuMyN2RhqVeuvljOomxwWch6fY2H391K_lTXgfEOYEd7MddpLzr9wsQwCKVWd0SAISHPmWIAlLEbfmHw_Rlp0EvuQtbmlQnokB6Is"));
            tRequest.Headers.Add(string.Format("Sender: id={0}", "936813335830"));
            tRequest.ContentLength = byteArray.Length;
            tRequest.ContentType = "application/json";
            using (Stream dataStream = tRequest.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);

                using (WebResponse tResponse = tRequest.GetResponse())
                {
                    using (Stream dataStreamResponse = tResponse.GetResponseStream())
                    {
                        using (StreamReader tReader = new StreamReader(dataStreamResponse))
                        {
                            String responseFromFirebaseServer = tReader.ReadToEnd();

                            FCMResponse response = Newtonsoft.Json.JsonConvert.DeserializeObject<FCMResponse>(responseFromFirebaseServer);
                            if (response.success == 1)
                            {
                                Print.Success("Sent the firebase notification");
                            }
                            else if (response.failure == 1)
                            {
                                Print.Error("Failed to send the firebase notification");
                            }
                        }
                    }
                }
            }
        }
    }

    public class FCMResponse
    {
        public long multicast_id { get; set; }
        public int success { get; set; }
        public int failure { get; set; }
        public int canonical_ids { get; set; }
        public List<FCMResult> results { get; set; }
    }
    public class FCMResult
    {
        public string message_id { get; set; }
    }
}

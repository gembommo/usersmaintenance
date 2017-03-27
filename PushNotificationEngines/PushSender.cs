using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyState.WebApplication.PushNotificationEngines
{
    public class PushSender
    {
        public static PushDetails SendReconnectRequest(IEnumerable<string> usersCodes)
        {
            var pushDetails = Push_Google(
                new PushSentData()
                {
                    Data = new PushSentMessage()
                    {
                        Date = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
                        Messagetype = PushMessageType.PleaseReconnect.ToString()
                    },
                    RegistrationIds = usersCodes,
                    TimeToLive = 2419200,
                    CollapseKey = PushMessageType.PleaseReconnect.ToString()
                });

            return pushDetails;
        }

        public static PushDetails SendSimpleMessageWithUrl(string title, string message, string url, IEnumerable<string> usersCodes)
        {
            var pushDetails = Push_Google(
                new PushSentData
                {
                    Data = new PushSentMessage
                    {
                        Date = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
                        Messagetype = PushMessageType.SimpleMessage.ToString(),
                        Message = message,
                        Title = title,
                        Url = url
                    },
                    RegistrationIds = usersCodes,
                    TimeToLive = 2419200,
                    CollapseKey = PushMessageType.SimpleMessage.ToString()
                });
            return pushDetails;
        }

        public static PushDetails SendResyncBusinessNumbersRequest(IEnumerable<string> usersCodes)
        {
            var pushDetails = Push_Google(
                new PushSentData()
                {
                    CollapseKey = PushMessageType.ResyncBusinessNumbers.ToString(),
                    Data = new PushSentMessage()
                    {
                        Date = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
                        Messagetype = PushMessageType.ResyncBusinessNumbers.ToString()
                    },
                    RegistrationIds = usersCodes,
                    TimeToLive = 2419200
                });

            return pushDetails;
        }

        public static PushDetails Push_Google(PushSentData message)
        {
            JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            string PushGoogleUri = "https://android.googleapis.com/gcm/send";
            string PushGoogleAppId = "AIzaSyAHwgci8LzCMMRdJRiiklA0_IGJ5Y_1QQU";
            string PushGoogleSenderId = "664789495415";

            WebRequest request;
            string postData = null;
            try
            {
                message.RegistrationIds = message.RegistrationIds.Where(x => !string.IsNullOrEmpty(x));
                postData = JsonConvert.SerializeObject(message, JsonSerializerSettings);

                if (!message.RegistrationIds.Any())
                {
                    return new PushDetails()
                    {
                        Status = ResponseStatus.Failure,

                        PushRequest = new PushRequest()
                        {
                            PostedData = postData,
                            Headers = null
                        },
                        PushResponse = string.Empty
                    };
                }

                request = WebRequest.Create(PushGoogleUri);
                request.Method = "POST";
                request.Headers.Add(String.Format("Authorization: key={0}", PushGoogleAppId));
                request.Headers.Add(String.Format("Sender: id={0}", PushGoogleSenderId));

                #region Request - As an object

                request.ContentType = "application/json;";

                using (var requestStream = request.GetRequestStream())
                {
                    using (var sw = new StreamWriter(requestStream))
                    {
                        sw.Write(postData);
                    }
                }
            }
            catch (Exception e)
            {

                return new PushDetails()
                {
                    Status = ResponseStatus.Failure,

                    PushRequest = new PushRequest()
                    {
                        PostedData = postData,
                        Headers = null
                    },
                    PushResponse = e.Message
                };
            }

                #endregion
            try
            {
                #region Response

                var response = request.GetResponse();
                string responseStr;
                using (var responseStream = response.GetResponseStream())
                {
                    using (var rs = new StreamReader(responseStream))
                    {
                        responseStr = rs.ReadToEnd();
                    }
                }
                var responseFromGoogle = JsonConvert.DeserializeObject<ResponseFromGoogle>(responseStr, JsonSerializerSettings);


                #endregion

                return new PushDetails()
                {
                    Status = ResponseStatus.Success,

                    PushRequest = new PushRequest()
                    {
                        PostedData = postData,
                        Headers = request.Headers
                    },
                    PushResponse = responseStr,
                    RawResponse = responseFromGoogle
                };
            }
            catch (Exception e)
            {

                return new PushDetails()
                {
                    Status = ResponseStatus.Failure,

                    PushRequest = new PushRequest()
                    {
                        PostedData = postData,
                        Headers = request.Headers
                    },
                    PushResponse = e.Message
                };
            }
        }
    }
}

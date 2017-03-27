using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MyState.WebApplication.PushNotificationEngines
{
    public class PushNotification
    {

        public PushNotification(string pushGoogleAppId = null, string pushGoogleSenderId = null)
        {

            PushGoogleAppId = pushGoogleAppId ?? PushGoogleAppId;
            PushGoogleSenderId = pushGoogleSenderId ?? PushGoogleSenderId;
        }

        #region Providers

        #region google

        public const string PushGoogleName = "GOOGLE";
        public const string PushGoogleUri = "https://android.googleapis.com/gcm/send";

        public string PushGoogleAppId = "AIzaSyAHwgci8LzCMMRdJRiiklA0_IGJ5Y_1QQU";
        public string PushGoogleSenderId = "664789495415";
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

        #endregion

        #region apple

        /// yeah...
        public const string PushAppleName = "APPLE";

        public const string PushAppleUri = "**";

        #endregion

        #endregion

        public async Task<PushDetails> PushAsync(PushDeviceType pushDeviceType,
            PushSentData message)
        {

            switch (pushDeviceType)//Todo: move to Factory
            {
                case PushDeviceType.Google:
                    return await Push_GoogleAsync(message);

                case PushDeviceType.Apple:
                    return Push_Apple(message);
            }
            return new PushDetails()
            {
                Status = ResponseStatus.Failure,
                PushRequest = new PushRequest() { PostedData = "NA" },
                PushResponse = "NA",
            };
        }

        public PushDetails Push(PushDeviceType pushDeviceType,
            PushSentData message)
        {

            switch (pushDeviceType)//Todo: move to Factory
            {
                case PushDeviceType.Google:
                    return Push_Google(message);

                case PushDeviceType.Apple:
                    return Push_Apple(message);
            }
            return new PushDetails()
            {
                Status = ResponseStatus.Failure,
                PushRequest = new PushRequest() { PostedData = "NA" },
                PushResponse = "NA",
            };
        }


        private PushDetails Push_Apple(PushSentData message)
        {
            return new PushDetails()
            {
                Status = ResponseStatus.Failure
            };
        }

        public async Task<PushDetails> Push_GoogleAsync(PushSentData message)
        {
            //TODO: the part of dividing to 1000s should be here. not in the previous function.
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

                using (var requestStream = await request.GetRequestStreamAsync())
                {
                    using (var sw = new StreamWriter(requestStream))
                    {
                        await sw.WriteAsync(postData);
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

                var response = await request.GetResponseAsync();
                string responseStr;
                using (var responseStream = response.GetResponseStream())
                {
                    using (var rs = new StreamReader(responseStream))
                    {
                        responseStr = await rs.ReadToEndAsync();
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
                    PushResponse = responseStr
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

        public PushDetails Push_Google(PushSentData message)
        {
            //TODO: the part of dividing to 1000s should be here. not in the previous function.
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

    public class PushSentMessage
    {
        public PushSentMessage()
        {

        }
        public PushSentMessage(string message, PushMessageType pushMessageType)
        {
            Message = message;
            Messagetype = pushMessageType.ToString();
        }
        
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
        [JsonProperty(PropertyName = "messagetype")]
        public string Messagetype { get; set; }
        [JsonProperty(PropertyName = "date")]
        public string Date { get; set; }
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "guid")]
        public string Guid { get; set; }

        [JsonProperty(PropertyName = "AdditionalInfo")]
        public string AdditionalInfo { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }


    public class PushSentData
    {
        [JsonProperty(PropertyName = "registration_ids")]
        public IEnumerable<string> RegistrationIds { get; set; }
        [JsonProperty(PropertyName = "data")]
        public PushSentMessage Data { get; set; }
        [JsonProperty(PropertyName = "collapse_key")]
        public string CollapseKey { get; set; }
        [JsonProperty(PropertyName = "time_to_live")]
        public int TimeToLive { get; set; }
        [JsonProperty(PropertyName = "delay_while_idle")]
        public bool DelayWhileIdle { get; set; }
    }

    public class GoogleResult
    {
        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; }
    }

    public class ResponseFromGoogle
    {
        [JsonProperty(PropertyName = "multicast_id")]
        public long MulticastId { get; set; }
        [JsonProperty(PropertyName = "success")]
        public int Success { get; set; }
        [JsonProperty(PropertyName = "failure")]
        public int Failure { get; set; }
        [JsonProperty(PropertyName = "canonical_ids")]
        public int CanonicalIds { get; set; }
        [JsonProperty(PropertyName = "results")]
        public List<GoogleResult> Results { get; set; }
    }
    
    public enum PushDeviceType
    {
        Google,
        Apple
    }
}

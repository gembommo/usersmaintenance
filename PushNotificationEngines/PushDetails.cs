using System.Net;

namespace MyState.WebApplication.PushNotificationEngines
{
    public class PushRequest
    {
        public string PostedData { get; set; }
        public WebHeaderCollection Headers { get; set; }
    }

    public class PushDetails
    {
        public ResponseStatus Status { get; set; }
        public PushRequest PushRequest { get; set; }
        public string PushResponse { get; set; }
        public ResponseFromGoogle RawResponse { get; set; }
    }
}
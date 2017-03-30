using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonInterfaces
{
    public class Log
    {
        public Log()
        {
            
        }
        public Log(Exception exception, string title, MessageType messageType, string action)
        {
            Title = title;
            Message = exception.Message;
            Details = action;
            Severity = (int)messageType;

        }

        public string Message { get; set; }
        public string Details { get; set; }
        public int Severity { get; set; }
        public string CallStack { get; set; }
        public string Title { get; set; }

        public object Param;

        public enum MessageType
        {
            Exception,
            Info
        }
    }
}

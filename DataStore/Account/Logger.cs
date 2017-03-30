
using System;
using CommonInterfaces;
using MyState.WebApplication.PushNotificationEngines;

namespace MyState.WebApplication.DataStore.Account
{
    public class MyStateLogger : IMyStateLogger
    {
        private readonly IDbCompleteDataStore _dal;

        public MyStateLogger(IDbCompleteDataStore dal)
        {
            _dal = dal;
        }

        public void Write(Log log)
        {
            _dal.InsertLog(log);
        }

        public void Write(Exception ex)
        {
            Write(new Log()
            {
                Message = ex.Message,
                CallStack = ex.StackTrace,
                Severity = (int)Log.MessageType.Exception,
            });
        }
    }


    public class MyStateEmptyLogger : IMyStateLogger
    {
        public MyStateEmptyLogger(IDbCompleteDataStore dal)
        {
            
        }
        public void Write(Log log)
        {
            
        }

        public void Write(Exception ex)
        {
            throw new NotImplementedException();
        }
    }
}

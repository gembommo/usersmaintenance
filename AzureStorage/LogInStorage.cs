using System;
using System.Collections.Generic;
using System.Linq;
using CommonInterfaces;

namespace AzureStorage
{
    public class LogInStorage : IMyStateLogger
    {
        private readonly IAzureStorage _azureStorage;

        public LogInStorage(IAzureStorage azureStorage)
        {
            _azureStorage = azureStorage;
        }

        public void WriteToLog(string exceptionName, params KeyValuePair<string, string>[] parameters)
        {
            if (parameters == null || parameters.Count() < 2)
                return;
            _azureStorage.SaveLogRecord(new LogRecord()
            {
                Messagetype = exceptionName,
                Title = exceptionName,
                Message = parameters[0].Value,
                StackTrace = parameters[1].Value,
            });
        }

        public void WriteToLog(Exception ex, params KeyValuePair<string, string>[] parameters)
        {
            if (parameters == null || parameters.Count() < 2)
                return;
            _azureStorage.SaveLogRecord(new LogRecord()
            {
                Messagetype = ex.Message,
                Title = ex.Message,
                Message = parameters[0].Value,
                StackTrace = parameters[1].Value,
            });
        }

        public void Write(Log log)
        {
            if (log == null)
                return;
            _azureStorage.SaveLogRecord(new LogRecord()
            {
                Messagetype = ((Log.MessageType)log.Severity).ToString() ,
                Title = log.Title,
                Message = log.Message,
                StackTrace = log.CallStack,
            });
        }

        public void Write(Exception ex)
        {
            if (ex == null)
                return;
            _azureStorage.SaveLogRecord(new LogRecord()
            {
                Messagetype = Log.MessageType.Exception.ToString(),
                Message = ex.Message,
                StackTrace = ex.StackTrace
            });
        }
    }
}

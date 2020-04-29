using System;
using System.Collections.Generic;
using System.Text;

namespace Sheev.Common.Logger
{
    public interface ILoggerManager : NLog.ILogger
    {
        void LogDetailMessage(String message, LT319.Common.Models.DetailRequest logDetailRequest, NLog.LogLevel logLevel);

        void LogHeaderMessage(String message, LT319.Common.Models.HeaderRequest logHeaderRequest, NLog.LogLevel logLevel);

        void IGLogMessage(String message, Sheev.Common.Models.BackupLogRequest iGLogRequest, NLog.LogLevel logLevel);

    }
}

using NLog;
using System;

namespace Sheev.Common.Logger
{
    public class LoggerManager : NLog.Logger, ILoggerManager
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public void LogDetailMessage(String message, LT319.Common.Models.DetailRequest detailLogRequest, NLog.LogLevel logLevel)
        {
            var eventInfo = new LogEventInfo(logLevel, logger.Name, message);
            eventInfo.Properties["TrackingGuid"] = detailLogRequest.TrackingGuid;
            eventInfo.Properties["ST_ApplicationId"] = detailLogRequest.ST_ApplicationId;
            eventInfo.Properties["TM_MappingCollectionTypeId"] = detailLogRequest.MappingCollectionTypeId;
            eventInfo.Properties["Activity"] = detailLogRequest.Activity;
            eventInfo.Properties["Status"] = detailLogRequest.DetailStatus;
            eventInfo.Properties["DetailDateTime"] = detailLogRequest.ActivityTimestamp;
            eventInfo.Properties["Details"] = detailLogRequest.Details;
            // You can also add them like this:
            //eventInfo.Properties.Add("CustomNumber", 42);
            // Send to Log
            logger.Log(eventInfo);
        }

        public void LogHeaderMessage(String message, LT319.Common.Models.HeaderRequest headerLogRequest, NLog.LogLevel logLevel)
        {
            var eventInfo = new LogEventInfo(logLevel, logger.Name, message);
            eventInfo.Properties["TrackingGuid"] = headerLogRequest.TrackingGuid;
            eventInfo.Properties["Name"] = headerLogRequest.Name;
            eventInfo.Properties["Direction"] = headerLogRequest.Direction;
            eventInfo.Properties["SystemId"] = headerLogRequest.SystemId;
            eventInfo.Properties["MappingCollectionType"] = headerLogRequest.MappingCollectionType;
            eventInfo.Properties["InternalId"] = headerLogRequest.InternalId;
            eventInfo.Properties["ExternalId"] = headerLogRequest.ExternalId;
            eventInfo.Properties["StartTimesTamp"] = headerLogRequest.StartTimestamp;
            eventInfo.Properties["Status"] = headerLogRequest.Status;
            // Send to Log
            logger.Log(eventInfo);
        }

        public void IGLogMessage(String message, Sheev.Common.Models.BackupLogRequest iGLogRequest, NLog.LogLevel logLevel)
        {
            var eventInfo = new LogEventInfo(logLevel, logger.Name, message);
            eventInfo.Properties["ST_ApplicationId"] = iGLogRequest.ApplicationId;
            eventInfo.Properties["DV_CompanyId"] = iGLogRequest.CompanyId;
            eventInfo.Properties["EventType"] = iGLogRequest.EventType;
            eventInfo.Properties["Location"] = iGLogRequest.Location;
            eventInfo.Properties["Message"] = iGLogRequest.Message;
            eventInfo.Properties["Scope"] = iGLogRequest.Scope;
            eventInfo.Properties["ReferenceId"] = iGLogRequest.ReferenceId;
            // Send to Log
            logger.Log(eventInfo);
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Diagnostics;

namespace IG2000.Data.Utilities
{
    /// <summary>
    /// Contains Methods for Logging Errors 
    /// </summary>
    public sealed class Logging
    {
        #region Public Members
        public static ThreadedLogger_TechDetail threadedTechLogger;
        public static ThreadedLogger_TrackingDetail threadedTrackingLogger;
        #endregion

        #region LogEvents
        /// <summary>
        /// Log all events here 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="location"></param>
        /// <param name="eventType"></param>
        /// <param name="trackingGuid"></param>
        public static void LogEvent(string message, string location, EventLogEntryType eventType, Sheev.Common.BaseModels.IBaseContextModel context, Guid? trackingGuid = null)
        {
            try
            {
                // Create the Backup Log Request
                Sheev.Common.Models.BackupLogRequest logRequest = new Sheev.Common.Models.BackupLogRequest()
                {
                    ApplicationId = (int)context.GenericSettings.Value.ApplicationId,
                    CompanyId = context.Security == null? Guid.Parse("BEA5A8E3-345A-4277-8669-2263E1939E3C") : context.Security.GetCompanyId(),
                    EventType = eventType.ToString(),
                    Location = location,
                    Message = message,
                    ReferenceId = trackingGuid.HasValue ? trackingGuid.Value.ToString() : null
                };

                // Create a new Threaded Logger Tech Detail is one does not already exist
                if (threadedTechLogger == null)
                    threadedTechLogger = new ThreadedLogger_TechDetail();

                //Nlog*******
                string logLevel = string.Empty;
                // Get the logLevel for the value passed in for eventType
                switch (eventType)
                {
                    case EventLogEntryType.Error:
                        logLevel = "Error";
                        break;
                    case EventLogEntryType.Warning:
                        logLevel = "Warning";
                        break;
                    case EventLogEntryType.Information:
                        logLevel = "Information";
                        break;
                }

                if (context.NLogger != null)
                {
                    if (logLevel == "Error")
                    {
                        context.NLogger.IGLogMessage($"IG_Log", logRequest, NLog.LogLevel.Error);
                    }
                    else if(logLevel == "Warning")
                    {
                        context.NLogger.IGLogMessage($"IG_Log", logRequest, NLog.LogLevel.Warn);
                    }
                    else
                    {
                        context.NLogger.IGLogMessage($"IG_Log", logRequest, NLog.LogLevel.Info);
                    }
                }
                //*********

                // Log the error message
                threadedTechLogger.LogMessage(logRequest, context);
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error: {ex.Message} while trying to log: {message}";

                BackupLogger(errorMessage, location, eventType, context, trackingGuid);
            }
        }

        /// <summary>
        /// Used to create a log tracking header and/or check/create tracking guid
        /// </summary>
        /// <param name="trackingGuid"></param>
        /// <param name="context"></param>
        /// <param name="requestTrackingGuid"></param>
        /// <param name="systemId"></param>
        /// <param name="internalId"></param>
        /// <returns></returns>
        public static Guid CreateLogTrackingHeader(Guid? trackingGuid, Sheev.Common.BaseModels.IBaseContextModel context, Guid? requestTrackingGuid = null, long? systemId = 0, long? internalId = 0)
        {
            // Create a new Guid
            var responseGuid = Guid.NewGuid();

            // Check if trackingGuid passed in has a value and is not an empty guid, if it has a value, return that value
            if (trackingGuid.HasValue && trackingGuid != Guid.Empty)
                return trackingGuid.Value;
            else if (requestTrackingGuid.HasValue && requestTrackingGuid != Guid.Empty)
                return requestTrackingGuid.Value;

            // Create a new Header Request 
            LT319.Common.Models.HeaderRequest logHeader = new LT319.Common.Models.HeaderRequest();

            try
            {
                // Build the Header Request
                logHeader.TrackingGuid = responseGuid;
                logHeader.StartTimestamp = DateTimeOffset.Now;
                logHeader.Direction = (int)Sheev.Common.Utilities.Constants.TM_MappingDirection.TO_IPAAS;
                logHeader.SystemId = systemId;
                logHeader.MappingCollectionType = (int)Sheev.Common.Utilities.Constants.TM_MappingCollectionType.NONE;
                logHeader.Status = LT319.Common.Utilities.Constants.TrackingStatus.Active.ToString();

                // Add Internal Id if a value for Internal Id was passed in
                if (internalId != null && internalId > 0)
                    logHeader.InternalId = internalId;

                // Create a new Tracking Detail Log
                Utilities.TrackingDetailLog trackingLogRequest = new TrackingDetailLog()
                {
                    Token = context.Security.GetAuthToken(),
                    TrackingHeaderLogRequest = logHeader
                };

                // Check if the threadedTrackingLogger has a value, if it does not, create a new one
                if (threadedTrackingLogger == null)
                    threadedTrackingLogger = new ThreadedLogger_TrackingDetail();

                //Log using NLog
                //check if the application is logging the message don't have NLog set.
                if(context.NLogger != null)
                {
                    context.NLogger.LogHeaderMessage($"Log Header ({trackingLogRequest.TrackingHeaderLogRequest.TrackingGuid})", trackingLogRequest.TrackingHeaderLogRequest, NLog.LogLevel.Info);
                }
                 
                // Log the tracking log request
                //To remove logging to old logger comment the below statement
                threadedTrackingLogger.LogMessage(trackingLogRequest, context);
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error: {ex.Message} while trying to create activity header: GUID {trackingGuid}";

                BackupLogger(errorMessage, "Logging.CreatLogTrackingHeader()", EventLogEntryType.Error, context, trackingGuid);
            }

            // Return the new Tracking Guid
            return responseGuid;
        }
        #endregion

        /// <summary>
        /// Log all events here 
        /// </summary>
        /// <param name="details"></param>
        /// <param name="activity"></param>
        /// <param name="trackingStatus"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <param name="logType"></param>
        public static void LogTrackingEvent(string details, string activity, LT319.Common.Utilities.Constants.TrackingStatus trackingStatus, Sheev.Common.BaseModels.IBaseContextModel context, Guid? trackingGuid)
        {
            try
            {
                    string status = string.Empty;

                    // Get the status for the activity based on the value passed in for trackingStatus
                    switch (trackingStatus)
                    {
                        case LT319.Common.Utilities.Constants.TrackingStatus.Active:
                            status = "Active";
                            break;
                        case LT319.Common.Utilities.Constants.TrackingStatus.Complete:
                            status = "Complete";
                            break;
                        case LT319.Common.Utilities.Constants.TrackingStatus.Error:
                            status = "Error";
                            break;
                    }

                    // Create a new Detail Request and build the request that will be added to the database
                    LT319.Common.Models.DetailRequest logRequest = new LT319.Common.Models.DetailRequest()
                    {
                        Details = details,
                        ST_ApplicationId = context.GenericSettings == null ? 0: (int)context.GenericSettings.Value.ApplicationId,
                        Activity = activity,
                        ActivityTimestamp = DateTimeOffset.Now,
                        MappingCollectionTypeId = (int)Sheev.Common.Utilities.Constants.TM_MappingCollectionType.NONE,
                        TrackingGuid = trackingGuid.Value,
                        DetailStatus = status.ToString()
                    };

                    // Create a new Tracking Detail Log 
                    Utilities.TrackingDetailLog trackingLogRequest = new TrackingDetailLog()
                    {
                        Token = context.Security.GetAuthToken(),
                        TrackingDetailLogRequest = logRequest
                    };

                    if (threadedTrackingLogger == null)
                        threadedTrackingLogger = new ThreadedLogger_TrackingDetail();

                //check if the application is logging the message don't have NLog set.
                if (context.NLogger != null)
                {
                    //Log using NLog
                    if (status == "Error")
                    {
                        context.NLogger.LogDetailMessage($"Log Detail ({status})", trackingLogRequest.TrackingDetailLogRequest, NLog.LogLevel.Error);
                    }
                    else
                    {
                        context.NLogger.LogDetailMessage($"Log Detail ({status})", trackingLogRequest.TrackingDetailLogRequest, NLog.LogLevel.Info);
                    }
                }

                //If still needed to update the header for each detail created, then make sure
                //the header is null and call the below method(I think the only thing that 
                //needs to be updated in the header is the 'Status' and 'EndDate. 
                //Maybe better to create an endpoint 'UpdateHeader'

                // Log the message details
                //To remove logging to old logger comment the below statement
                threadedTrackingLogger.LogMessage(trackingLogRequest, context);
                
            }
            catch (Exception ex)
            {
                //string errorMessage = $"Error: {ex.Message} while trying to log: {details}";
                string errorMessage = JsonConvert.SerializeObject(ex, Formatting.Indented);

                BackupLogger(errorMessage, "Logging.LogTrackingEvent()", EventLogEntryType.Error, context, trackingGuid);
            }
        }

        #region Helper Method(s)
        private static void BackupLogger(string message, string location, EventLogEntryType eventType, Sheev.Common.BaseModels.IBaseContextModel context, Guid? trackingGuid = null)
        {
            //try
            //{
            //    var request = new DockingBay94.Models.BackupLogRequest()
            //    {
            //        ApplicationId = applicationId,
            //        CompanyId = Security.GetCompanyId(),
            //        EventType = eventType.ToString(),
            //        Location = location,
            //        Message = message,
            //        ReferenceId = trackingGuid.ToString()
            //    };

            //    var restClient = new RestSharp.RestClient(ConfigurationManager.AppSettings["BackupLogger_Url"]);
            //    RestSharp.RestRequest req = new RestSharp.RestRequest("/Todd/Damnit", RestSharp.Method.POST);
            //    var bodyJSON = JsonConvert.SerializeObject(request);
            //    req.AddParameter("application/json", bodyJSON, ParameterType.RequestBody);
            //    var resp = (RestSharp.RestResponse)restClient.Execute(req);


            //}
            //catch (Exception ex2)
            //{
            //    string directory = AppDomain.CurrentDomain.BaseDirectory;
            //    string filePath = "Logs\\";
            //    string fileName = "Log-" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";

            //    try
            //    {
            //        if (!Directory.Exists(directory + filePath) || File.Exists(fileName))
            //        {
            //            Directory.CreateDirectory(directory + filePath);
            //        }

            //        using (StreamWriter writer = new StreamWriter(directory + filePath + fileName, true))
            //        {
            //            writer.WriteLine(string.Format("{0} [{1}] [{2}]: {3}", DateTime.Now, EventLogEntryType.Warning.ToString(), "LogEvent()", ex2.Message));

            //            // Log Format - 2003-02-26 12:04:46 [Priority] [Location]: Message
            //            if (!string.IsNullOrEmpty(location))
            //                writer.WriteLine(string.Format("{0} [{1}] [{2}]: {3}", DateTime.Now, eventType.ToString(), location, message));
            //            else
            //                writer.WriteLine(string.Format("{0} [{1}]: {2}", DateTime.Now, eventType.ToString(), message));
            //        }
            //    }
            //    catch (Exception exc)
            //    {
            //        throw exc;
            //    }
            //}
        }
        #endregion
    }

    /// <summary>
    /// Contains the methods the log errors
    /// </summary>
    public sealed class ErrorLogger
    {
        #region Properties
        private static object _ObjLock = new object();
        #endregion

        #region Public Method(s)
        /// <summary>
        /// Logs error messages 
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="methodName"></param>
        /// <param name="trackingGuid"></param>
        /// <param name="logType"></param>
        public static void Report(string errorMessage, string methodName, Sheev.Common.BaseModels.IBaseContextModel context, Guid? trackingGuid = null, EventLogEntryType logType = EventLogEntryType.Information)
        {
            var logLevel = "D"; //context.Configuration.AppSettings["LogLevel"];
            var logEntryType = EventLogEntryType.Error;

            // Select the log level based on what's passed in
            switch (logLevel)
            {
                case "D":
                    logEntryType = EventLogEntryType.Information;
                    break;
                case "W":
                    logEntryType = EventLogEntryType.Warning;
                    break;
                default:
                    logEntryType = EventLogEntryType.Error;
                    break;
            }

            if (logType <= logEntryType)
            {
                lock (_ObjLock)
                {
                    Logging.LogEvent(errorMessage, methodName, logType, context, trackingGuid);
                }
            }
        }

        /// <summary>
        /// Logs error messages 
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="methodName"></param>
        /// <param name="trackingGuid"></param>
        /// <param name="logType"></param>
        public static void Report(Exception ex, string methodName, Sheev.Common.BaseModels.BaseContextModel context, Guid? trackingGuid = null, EventLogEntryType logType = EventLogEntryType.Information)
        {
            var errorMessage = JsonConvert.SerializeObject(ex, Formatting.Indented);

            Report(errorMessage, methodName, context, trackingGuid, logType);
        }
        #endregion
    }
}

using Newtonsoft.Json;
using RestSharp;
using System;
using System.Net;

namespace IG2000.Data.Utilities
{
    public class ThreadedLogger_TrackingDetail : ThreadedLogger
    {
        protected override void AsyncLogMessage(object row, Sheev.Common.BaseModels.IBaseContextModel context)
        {
            TrackingDetailLog logRequest = ((TrackingDetailLog)row);

            if (logRequest.TrackingHeaderLogRequest == null)
            {
                try
                {
                    var restClient = new RestSharp.RestClient(context.ApiUrlSettings.Value.LT319_URL);
                    RestSharp.RestRequest req = new RestSharp.RestRequest("/v2/Detail", RestSharp.Method.POST);
                    var bodyJSON = JsonConvert.SerializeObject(logRequest.TrackingDetailLogRequest);
                    req.AddHeader("Authorization", $"Bearer {logRequest.Token}");
                    req.AddParameter("application/json", bodyJSON, ParameterType.RequestBody);
                    var resp = (RestSharp.RestResponse)restClient.Execute(req);

                    if (resp.StatusCode != HttpStatusCode.NoContent)
                    {
                        Utilities.ErrorLogger.Report(JsonConvert.SerializeObject(resp), "ThreadedLogger_TrackingDetail.AsyncLogMessage()", context, logRequest.TrackingDetailLogRequest.TrackingGuid, System.Diagnostics.EventLogEntryType.Error);

                        //  throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = resp.ErrorException.Message };
                    }
                }
                catch (Exception ex)
                {
                    Utilities.ErrorLogger.Report(JsonConvert.SerializeObject(ex), "ThreadedLogger_TrackingDetail.AsyncLogMessage()", context, logRequest.TrackingDetailLogRequest.TrackingGuid, System.Diagnostics.EventLogEntryType.Error);
                }
            }
            else
            {
                try
                {
                    var restClient = new RestSharp.RestClient(context.ApiUrlSettings.Value.LT319_URL);
                    RestSharp.RestRequest req = new RestSharp.RestRequest("/v2/Header", RestSharp.Method.POST);
                    var bodyJSON = JsonConvert.SerializeObject(logRequest.TrackingHeaderLogRequest);
                    req.AddHeader("Authorization", $"Bearer {logRequest.Token}");
                    req.AddParameter("application/json", bodyJSON, ParameterType.RequestBody);
                    var resp = (RestSharp.RestResponse)restClient.Execute(req);

                    if (resp.StatusCode != HttpStatusCode.NoContent)
                    {
                        Utilities.ErrorLogger.Report(JsonConvert.SerializeObject(resp), "ThreadedLogger_TrackingHeader.AsyncLogMessage()", context, logRequest.TrackingHeaderLogRequest.TrackingGuid, System.Diagnostics.EventLogEntryType.Error);

                        //  throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = resp.ErrorException.Message };
                    }
                }
                catch (Exception ex)
                {
                    Utilities.ErrorLogger.Report(JsonConvert.SerializeObject(ex), "ThreadedLogger_TrackingHeader.AsyncLogMessage()", context, logRequest.TrackingHeaderLogRequest.TrackingGuid, System.Diagnostics.EventLogEntryType.Error);
                }
            }
        }
    }
    public class TrackingDetailLog
    {
        public string Token;
        public LT319.Common.Models.DetailRequest TrackingDetailLogRequest;
        public LT319.Common.Models.HeaderRequest TrackingHeaderLogRequest;
    }
}

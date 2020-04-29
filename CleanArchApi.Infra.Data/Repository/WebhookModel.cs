using Newtonsoft.Json;
using RestSharp;
using Sheev.Common.BaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tagge.Models.Interfaces;

namespace Tagge.Models
{
    public class WebhookModel : IWebhookModel
    {
        /// <summary>
        /// Fire a webhook event
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <param name="attempt"></param>
        /// <returns></returns>
        public async Task FireWebhookEvent(Sheev.Common.Models.WebhookResponse request, IBaseContextModel context, Guid trackingGuid, int attempt = 0)
        {
            try
            {
                // Start Log Here
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"iring webhook event: {request.Scope}. Attempt: {attempt + 1}/3 Note if webhook is not found in the receiver then this is either a clapback or the system is not subscribed to this scope.", "Trigger Webhook Event", LT319.Common.Utilities.Constants.TrackingStatus.Active, context, trackingGuid);

                // Rest Client
                var restClient = new RestSharp.RestClient(context.ApiUrlSettings.Value.Skyhook_URL);

                // Building the request and setting endpoint target
                RestSharp.RestRequest req = new RestSharp.RestRequest("/v2/Event", RestSharp.Method.POST);

                // Serializing the body
                var bodyJSON = JsonConvert.SerializeObject(request);

                // Added the application/json header
                req.AddParameter("application/json", bodyJSON, ParameterType.RequestBody);

                // Execute the request
                var resp = (RestSharp.RestResponse)restClient.Execute(req);

                // Check the response
                if (!resp.IsSuccessful)
                {
                    // Log something here
                    IG2000.Data.Utilities.ErrorLogger.Report(JsonConvert.SerializeObject(resp), "WebhookModel.FireWebhookEvent()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);

                    if (resp.ErrorException != null)
                    {
                        IG2000.Data.Utilities.Logging.LogTrackingEvent($"Webhook was unable to be sent! Reason: {resp.ErrorException.Message}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                    }

                    // Retry
                    if (attempt < 3)
                        await FireWebhookEvent(request, context, trackingGuid, attempt + 1);
                }

                // End Log Here
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Webhook {request.Scope} successfully fired on attempt: {attempt + 1}.", "Trigger Webhook Event", LT319.Common.Utilities.Constants.TrackingStatus.Active, context, trackingGuid);

            }
            catch (Exception ex)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"See logs for additional details", "Failed", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                IG2000.Data.Utilities.ErrorLogger.Report(ex.Message, "WebhookModel.FireWebhookEvent()", context, trackingGuid, System.Diagnostics.EventLogEntryType.Error);

                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError, ReasonPhrase = ex.Message };
            }
        }
    }
}

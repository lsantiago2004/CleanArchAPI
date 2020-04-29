using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Sheev.Common.BaseModels;
using Sheev.Common.Logger;
using Tagge.Authentication;
using Tagge.Filters;
using Tagge.Models;
using Tagge.Utilities;
using Tagge.Models.Interfaces;

namespace Tagge.Controllers
{
    [Route("v2/Catalog")]
    [ApiController]
    public class KitController : ControllerBase
    {
        #region Private Properties
        private readonly IBaseContextModel _context;
        private readonly IKitModel _kitModel;
        #endregion

        #region Constructor(s)
        public KitController(IKitModel kitModel, ILoggerManager logger, IOptions<Sheev.Common.Models.MongoDbSetting> mongoDbSettings, IOptions<Sheev.Common.Models.ApiUrlSetting> apiSettings)
        {
            _context = new Models.ContextModel(mongoDbSettings, apiSettings, logger);
            _kitModel = kitModel;
        }
        #endregion

        #region Get Call(s)
        /// <summary>
        /// Gets a Kit By sku
        /// </summary>
        /// <param name="id">kit sku</param>
        /// <param name="trackingGuid">Tracking Guid</param>
        /// <returns></returns>
        [HttpGet("Kit/{id}")]
        [AuthorizeClaim("Product Kit", K2SO.Auth.Constants.PermissionAccessType.VIEW)]
        public async Task<Tagge.Common.Models.KitResponse> GetKit(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            if (string.IsNullOrEmpty(id))
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Id ({id}) invalid! Reason: Id is null or empty", $"Status Code: 400 Bad Request", LT319.Common.Utilities.Constants.TrackingStatus.Error, _context, (Guid)trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = "Id is null or empty" };
            }

            var response = new Tagge.Common.Models.KitResponse();
            response = await _kitModel.GetById(id, (Guid)trackingGuid);

            return response;
        }
        #endregion

        #region Post Call(s)
        /// <summary>
        /// Adds new Kit
        /// </summary>
        /// <param name="request"></param>
        [HttpPost("Kit")]
        [AuthorizeClaim("Product Kit", K2SO.Auth.Constants.PermissionAccessType.CREATE)]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<Tagge.Common.Models.KitResponse> PostKit([FromBody]Tagge.Common.Models.KitRequest request)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            Guid trackingGuid = Guid.NewGuid();

            if (request == null)
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, null);
            else
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, request.TrackingGuid);

            //Utilities.RestErrorHandler.CheckKitRequest(request, _context, trackingGuid);

            var response = new Tagge.Common.Models.KitResponse();
            response = await _kitModel.Save(request, trackingGuid);

            return response;
        }
        #endregion

        #region Put Call(s)
        /// <summary>
        /// Update existing Kit
        /// </summary>
        /// <param name="id">kit id</param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("Kit/{id}")]
        [AuthorizeClaim("Product Kit", K2SO.Auth.Constants.PermissionAccessType.EDIT)]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<Tagge.Common.Models.KitResponse> PutKit(string id, [FromBody]Tagge.Common.Models.KitRequest request)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            Guid trackingGuid = Guid.NewGuid();

            if (request == null)
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, null);
            else
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, request.TrackingGuid);

            //long longId = Utilities.RestErrorHandler.CheckId(id, _context, trackingGuid);

            var response = new Tagge.Common.Models.KitResponse();
            response = await _kitModel.Update(id, request, trackingGuid);

            return response;
        }

        /// <summary>
        /// Reactivates a Kit
        /// </summary>
        /// <remarks>
        /// Reactivates a Kit by its sku
        /// </remarks>
        /// <param name="id">Kit sku</param>
        /// <param name="trackingGuid">Tracking Guid</param>
        /// <returns></returns>
        [HttpPut("Kit/Reactivate/{id}")]
        [AuthorizeClaim("Product Kit", K2SO.Auth.Constants.PermissionAccessType.EDIT)]
        public async Task<NoContentResult> ReactivateKit(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            if (string.IsNullOrEmpty(id))
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Id ({id}) invalid! Reason: Id is null or empty", $"Status Code: 400 Bad Request", LT319.Common.Utilities.Constants.TrackingStatus.Error, _context, (Guid)trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = "Id is null or empty" };
            }

            await _kitModel.DeleteOrReactivate(id, true, (Guid)trackingGuid);

            return NoContent();
        }
        #endregion

        #region Delete Call(s)
        // DELETE api/values/5
        /// <summary>
        /// Removes a Kit
        /// </summary>
        /// <param name="id">kit sku</param>
        /// <param name="trackingGuid">Tracking Guid</param>
        /// <returns></returns>
        [HttpDelete("Kit/{id}")]
        [AuthorizeClaim("Product Kit", K2SO.Auth.Constants.PermissionAccessType.DELETE)]
        public async Task<NoContentResult> DeleteKit(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            if (string.IsNullOrEmpty(id))
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Id ({id}) invalid! Reason: Id is null or empty", $"Status Code: 400 Bad Request", LT319.Common.Utilities.Constants.TrackingStatus.Error, _context, (Guid)trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = "Id is null or empty" };
            }

            await _kitModel.DeleteOrReactivate(id, false, (Guid)trackingGuid);

            return NoContent();
        }
        #endregion
    }
}
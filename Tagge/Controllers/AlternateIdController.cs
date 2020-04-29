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
using Product.Authentication;
using Product.Filters;
using Product.Models;
using Product.Models.Interfaces;
using Product.Utilities;

namespace Product.Controllers
{
    [Route("v2/Catalog")]
    [ApiController]
    public class AlternateIdController : ControllerBase
    {
        #region Private Properties
        private readonly IBaseContextModel _context;
        private readonly IAlternateIdModel _alternateIdModel;
        #endregion

        #region Constructor(s)
        public AlternateIdController(IAlternateIdModel alternateIdModel, ILoggerManager logger, IOptions<Sheev.Common.Models.MongoDbSetting> mongoDbSettings, IOptions<Sheev.Common.Models.ApiUrlSetting> apiSettings)
        {
            _context = new Models.ContextModel(mongoDbSettings, apiSettings, logger);
            _alternateIdModel = alternateIdModel;
        }
        #endregion

        #region Get Call(s)
        /// <summary>
        /// Gets a Alternate Id By Id
        /// </summary>
        /// <param name="id">Alternate Id</param>
        /// <param name="trackingGuid">Tracking Guid</param>
        /// <returns></returns>
        [HttpGet("AlternateId/{id}")]
        [AuthorizeClaim("Product Alternate Id", K2SO.Auth.Constants.PermissionAccessType.VIEW)]
        public async Task<Product.Common.Models.ProductAlternateIdResponse> GetAlternateId(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            if(string.IsNullOrEmpty(id))
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Id ({id}) invalid! Reason: Id is null or empty", $"Status Code: 400 Bad Request", LT319.Common.Utilities.Constants.TrackingStatus.Error, _context, (Guid)trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = "Id is null or empty" };
            }

            var response = new Product.Common.Models.ProductAlternateIdResponse();
            response = await _alternateIdModel.GetById(_context, id, "PC_Product", (Guid)trackingGuid);

            return response;
        }
        #endregion

        #region Post Call(s)
        /// <summary>
        /// Adds new Alternate Id 
        /// </summary>
        /// <param name="request"></param>
        [HttpPost("AlternateId")]
        [AuthorizeClaim("Product Alternate Id", K2SO.Auth.Constants.PermissionAccessType.CREATE)]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<Product.Common.Models.ProductAlternateIdResponse> PostAlternateId([FromBody]Product.Common.Models.AlternateIdRequest request)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            Guid trackingGuid = Guid.NewGuid();

            if (request == null)
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, null);
            else
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, request.TrackingGuid);

            Utilities.RestErrorHandler.CheckAlternateIdRequest(request, _context, trackingGuid);

            var response = new Product.Common.Models.ProductAlternateIdResponse();
            response = await _alternateIdModel.Save(_context, request, trackingGuid);

            return response;
        }
        #endregion

        #region Put Call(s)
        /// <summary>
        /// Update existing Alternate Id 
        /// </summary>
        /// <param name="id">Alternate Id id</param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("AlternateId/{id}")]
        [AuthorizeClaim("Product Alternate Id", K2SO.Auth.Constants.PermissionAccessType.EDIT)]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<Product.Common.Models.ProductAlternateIdResponse> PutAlternateId(string id, [FromBody]Product.Common.Models.AlternateIdRequest request)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            Guid trackingGuid = Guid.NewGuid();

            if (request == null)
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, null);
            else
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, request.TrackingGuid);

            Utilities.RestErrorHandler.CheckAlternateIdRequest(request, _context, trackingGuid);

            var response = new Product.Common.Models.ProductAlternateIdResponse();
            response = await _alternateIdModel.Update(_context, id, request, trackingGuid);

            return response;
        }

        /// <summary>
        /// Reactivates a Alternate Id 
        /// </summary>
        /// <remarks>
        /// Reactivates a Alternate Id by its id
        /// <br />
        /// Id format: product id|AlternateId Type Id|Sku
        /// </remarks>
        /// <param name="id">Alternate Id id</param>
        /// <param name="trackingGuid">Tracking Guid</param>
        /// <returns></returns>
        [HttpPut("AlternateId/Reactivate/{id}")]
        [AuthorizeClaim("Product Alternate Id", K2SO.Auth.Constants.PermissionAccessType.EDIT)]
        public async Task<NoContentResult> ReactivateAlternateId(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            await _alternateIdModel.DeleteOrReactivate(_context, id, "PC_Product", true, (Guid)trackingGuid);

            return NoContent();
        }
        #endregion

        #region Delete Call(s)
        // DELETE api/values/5
        /// <summary>
        /// Removes a Alternate Id 
        /// </summary>
        /// <remarks>
        /// Removes a Alternate Id by its id
        /// <br />
        /// Id format: product id|AlternateId Type Id|Sku
        /// </remarks>
        /// <param name="id">Alternate Id  id</param>
        /// <param name="trackingGuid">Tracking Guid</param>
        /// <returns></returns>
        [HttpDelete("AlternateId/{id}")]
        [AuthorizeClaim("Product Alternate Id", K2SO.Auth.Constants.PermissionAccessType.DELETE)]
        public async Task<NoContentResult> DeleteAlternateId(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            await _alternateIdModel.DeleteOrReactivate(_context, id, "PC_Product", false, (Guid)trackingGuid);

            return NoContent();
        }
        #endregion
    }
}
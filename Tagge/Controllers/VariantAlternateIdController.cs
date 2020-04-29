using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Sheev.Common.Logger;
using Product.Authentication;
using Product.Filters;
using Product.Models;
using Product.Models.Interfaces;

namespace Product.Controllers
{
    [Route("v2/Catalog")]
    [ApiController]
    public class VariantAlternateIdController : ControllerBase
    {
        #region Private Properties
        private readonly Models.ContextModel _context;
        private readonly IAlternateIdModel _alternativeIdModel;
        #endregion

        #region Constructor(s)
        public VariantAlternateIdController(IAlternateIdModel alternativeIdModel, ILoggerManager logger, IOptions<Sheev.Common.Models.MongoDbSetting> mongoDbSettings, IOptions<Sheev.Common.Models.ApiUrlSetting> apiSettings)
        {
            _context = new Models.ContextModel(mongoDbSettings, apiSettings, logger);
            _alternativeIdModel = alternativeIdModel;

        }
        #endregion

        #region Get Call(s)
        /// <summary>
        /// Gets a Variant Alternate Id By Id
        /// </summary>
        /// <param name="id">category id</param>
        /// <param name="trackingGuid">Tracking Guid</param>
        /// <returns></returns>
        [HttpGet("Variant/AlternateId/{id}")]
        [AuthorizeClaim("Product Variant Alternate Id", K2SO.Auth.Constants.PermissionAccessType.VIEW)]
        public async Task<Product.Common.Models.ProductAlternateIdResponse> GetProductVariantAltId(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            var response = new Product.Common.Models.ProductAlternateIdResponse();
            response = await _alternativeIdModel.GetById(_context, id, "PC_ProductVariant", (Guid)trackingGuid);

            return response;
        }
        #endregion

        #region Post Call(s)
        /// <summary>
        /// Adds new Variant Alternate Id
        /// </summary>
        /// <param name="request"></param>
        [HttpPost("Variant/AlternateId")]
        [AuthorizeClaim("Product Variant Alternate Id", K2SO.Auth.Constants.PermissionAccessType.CREATE)]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<Product.Common.Models.ProductAlternateIdResponse> PostProductVariantAltId([FromBody]Product.Common.Models.AlternateIdRequest request)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            Guid trackingGuid = Guid.NewGuid();

            if (request == null)
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, null);
            else
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, request.TrackingGuid);

            Utilities.RestErrorHandler.CheckAlternateIdRequest(request, _context, trackingGuid);

            var response = new Product.Common.Models.ProductAlternateIdResponse();
            response = await _alternativeIdModel.SaveVariant(_context, request, trackingGuid);

            return response;
        }
        #endregion

        #region Put Call(s)
        /// <summary>
        /// Update existing Variant Alternate Id
        /// </summary>
        /// <param name="id">Variant Alternate Id id</param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("Variant/AlternateId/{id}")]
        [AuthorizeClaim("Product Variant Alternate Id", K2SO.Auth.Constants.PermissionAccessType.EDIT)]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<Product.Common.Models.ProductAlternateIdResponse> PutProductVariantAltId(string id, [FromBody]Product.Common.Models.AlternateIdRequest request)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            Guid trackingGuid = Guid.NewGuid();

            if (request == null)
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, null);
            else
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, request.TrackingGuid);

            Utilities.RestErrorHandler.CheckAlternateIdRequest(request, _context, trackingGuid);

            var response = new Product.Common.Models.ProductAlternateIdResponse();
            response = await _alternativeIdModel.UpdateVariant(_context, id, request, trackingGuid);

            return response;
        }

        /// <summary>
        /// Reactivates a Variant Alternate Id
        /// </summary>
        /// <remarks>
        /// Reactivates a Variant Alternate Id by its id
        /// <br />
        /// Id format variant id|AlternateId Type Id|Sku
        /// </remarks>
        /// <param name="id">Variant Alternate Id id</param>
        /// <param name="trackingGuid">Tracking Guid</param>
        /// <returns></returns>
        [HttpPut("Variant/AlternateId/Reactivate/{id}")]
        [AuthorizeClaim("Product Variant Alternate Id", K2SO.Auth.Constants.PermissionAccessType.EDIT)]
        public async Task<NoContentResult> ReactivateProductVariantAltId(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            //Utilities.RestErrorHandler.CheckId(id, 2, _context, (Guid)trackingGuid);

            await _alternativeIdModel.DeleteOrReactivate(_context, id, "PC_ProductVariant", true, (Guid)trackingGuid);

            return NoContent();
        }
        #endregion

        #region Delete Call(s)
        /// <summary>
        /// Removes a Variant Alternate Id
        /// </summary>
        /// <remarks>
        /// Removes a Variant Alternate Id by its id
        /// <br />
        /// Id format: variant id|AlternateId Type Id|Sku
        /// </remarks>
        /// <param name="id">Variant Alternate Id id</param>
        /// <param name="trackingGuid">Tracking Guid</param>
        /// <returns></returns>
        [HttpDelete("Variant/AlternateId/{id}")]
        [AuthorizeClaim("Product Variant Alternate Id", K2SO.Auth.Constants.PermissionAccessType.DELETE)]
        public async Task<NoContentResult> DeleteProductVariantAltId(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            //long longId = Utilities.RestErrorHandler.CheckId(id, _context, (Guid)trackingGuid);

            await _alternativeIdModel.DeleteOrReactivate(_context, id, "PC_ProductVariant", false, (Guid)trackingGuid);

            return NoContent();
        }
        #endregion
    }
}
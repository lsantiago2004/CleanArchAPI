using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sheev.Common.Logger;
using Product.Authentication;
using Product.Filters;
using Product.Models;
using Product.Models.Interfaces;

namespace Product.Controllers
{
    [Route("v2/Catalog/Product")]
    [ApiController]
    public class VariantController : ControllerBase
    {
        #region Private Properties
        private readonly Models.ContextModel _context;
        private readonly IVariantModel _variantModel;
        #endregion

        #region Constructor(s)
        public VariantController(IVariantModel variantModel, ILoggerManager logger, IOptions<Sheev.Common.Models.MongoDbSetting> mongoDbSettings, IOptions<Sheev.Common.Models.ApiUrlSetting> apiSettings)
        {
            _context = new Models.ContextModel(mongoDbSettings, apiSettings, logger);
            _variantModel = variantModel;
        }
        #endregion

        #region Get Call(s)
        /// <summary>
        /// Get a single product variant by its id
        /// </summary>
        /// <param name="id">product variant id</param>
        /// <param name="trackingGuid">Tracking Guid</param>
        /// <returns></returns>
        [HttpGet("Variant/{id}")]
        [AuthorizeClaim("Product Variant", K2SO.Auth.Constants.PermissionAccessType.VIEW)]
        public async Task<Product.Common.Models.ProductVariantResponse> Get(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            long longId = Utilities.RestErrorHandler.CheckId(id, _context, (Guid)trackingGuid);

            var response = new Product.Common.Models.ProductVariantResponse();

            response = await _variantModel.GetById(longId, (Guid)trackingGuid);

            return response;
        }
        #endregion

        #region Post Call(s)
        #endregion

        #region Put Call(s)
        /// <summary>
        /// Saves a product variant by product id
        /// </summary>
        /// <remarks>
        /// Adds/Updates an existing variant based on the product id provided
        /// ### Required Fields ###
        /// - Product Id (query string)
        /// - Sku
        /// </remarks>
        /// <param name="id">product id</param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("Variant/{id}")]
        [AuthorizeClaim("Product Variant", K2SO.Auth.Constants.PermissionAccessType.EDIT)]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<Product.Common.Models.ProductVariantResponse> Put(string id, [FromBody]Product.Common.Models.ProductVariantRequest request)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            Guid trackingGuid = Guid.NewGuid();

            if (request == null)
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, null);
            else
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, request.TrackingGuid);


            long longId = Utilities.RestErrorHandler.CheckId(id, _context, trackingGuid);
            Utilities.RestErrorHandler.CheckVariantRequest(request, _context, trackingGuid);

            var response = new Product.Common.Models.ProductVariantResponse();

            response = await _variantModel.Update(request, longId, trackingGuid, true);

            return response;
        }

        /// <summary>
        /// Reactivates a product variant
        /// </summary>
        /// <remarks>
        /// Reactivates a product variant by its id
        /// </remarks>
        /// <param name="id">Product Variant Id</param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        [HttpPut("Variant/Reactivate/{id}")]
        [AuthorizeClaim("Product Variant", K2SO.Auth.Constants.PermissionAccessType.EDIT)]
        public async Task<NoContentResult> ReactivateProduct(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            long longId = Utilities.RestErrorHandler.CheckId(id, _context, (Guid)trackingGuid);

            await _variantModel.DeleteOrReactivate(longId, true, (Guid)trackingGuid);

            return NoContent();
        }
        #endregion

        #region Delete Call(s)
        /// <summary>
        /// Deletes a product variant
        /// </summary>
        /// <remarks>
        /// Removes a product variant by product variant id
        /// </remarks>
        /// <param name="id">product variant id</param>
        /// <param name="trackingGuid">Tracking Guid</param>
        /// <returns></returns>
        [HttpDelete("Variant/{id}")]
        [AuthorizeClaim("Product Variant", K2SO.Auth.Constants.PermissionAccessType.DELETE)]
        public async Task<NoContentResult> Delete(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            long longId = Utilities.RestErrorHandler.CheckId(id, _context, (Guid)trackingGuid);

            await _variantModel.DeleteOrReactivate(longId, false, (Guid)trackingGuid);

            return NoContent();
        }
        #endregion
    }
}
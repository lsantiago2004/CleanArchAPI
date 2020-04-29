using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sheev.Common.Logger;
using Tagge.Authentication;
using Tagge.Filters;
using Tagge.Models;
using Tagge.Models.Interfaces;

namespace Tagge.Controllers
{
    [Route("v2/Catalog")]
    [ApiController]
    public class VariantCategoryController : ControllerBase
    {
        #region Private Properties
        private readonly Models.ContextModel _context;
        private readonly ICategoryAssignmentModel _categoryAssignmentModel;
        #endregion

        #region Constructor(s)
        public VariantCategoryController(ICategoryAssignmentModel categoryAssignmentModel, ILoggerManager logger, IOptions<Sheev.Common.Models.MongoDbSetting> mongoDbSettings, IOptions<Sheev.Common.Models.ApiUrlSetting> apiSettings)
        {
            _context = new Models.ContextModel(mongoDbSettings, apiSettings, logger);
            _categoryAssignmentModel = categoryAssignmentModel;
        }
        #endregion

        #region Get Call(s)
        /// <summary>
        /// Gets a Product Variant Category By Id
        /// </summary>
        /// <param name="id">Product Category id</param>
        /// <param name="trackingGuid">Tracking Guid</param>
        /// <returns></returns>
        [HttpGet("ProductVariantCategory/{id}")]
        [AuthorizeClaim("Product Category", K2SO.Auth.Constants.PermissionAccessType.VIEW)]
        public async Task<Tagge.Common.Models.ProductCategoryResponse> GetProductCategory(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            Utilities.RestErrorHandler.CheckId(id, 2, _context, (Guid)trackingGuid);

            var response = new Tagge.Common.Models.ProductCategoryResponse();
            response = await _categoryAssignmentModel.GetById(_context, id, "PC_ProductVariant", (Guid)trackingGuid);

            return response;
        }
        #endregion

        #region Post Call(s)
        /// <summary>
        /// Adds new Product Variant Category
        /// </summary>
        /// <param name="request"></param>
        [HttpPost("ProductVariantCategory")]
        [AuthorizeClaim("Product Category", K2SO.Auth.Constants.PermissionAccessType.CREATE)]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<Tagge.Common.Models.ProductCategoryResponse> PostProductCategory([FromBody]Tagge.Common.Models.ProductCategoryRequest request)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            Guid trackingGuid = Guid.NewGuid();

            if (request == null)
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, null);
            //else
            //    trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, request.TrackingGuid);

            //Utilities.RestErrorHandler.CheckKitRequest(request, _context, trackingGuid);

            var response = new Tagge.Common.Models.ProductCategoryResponse();
            response = await _categoryAssignmentModel.Save(_context, request, "PC_ProductVariant", trackingGuid);

            return response;
        }
        #endregion

        #region Delete Call(s)
        /// <summary>
        /// Removes a Product Variant Category
        /// </summary>
        /// <param name="id">Category id</param>
        /// <param name="trackingGuid">Tracking Guid</param>
        /// <returns></returns>
        [HttpDelete("ProductVariantCategory/{id}")]
        [AuthorizeClaim("Product Category", K2SO.Auth.Constants.PermissionAccessType.DELETE)]
        public async Task<NoContentResult> DeletesProductCategory(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            Utilities.RestErrorHandler.CheckId(id, 2, _context, (Guid)trackingGuid);

            await _categoryAssignmentModel.DeleteOrReactivate(_context, id, "PC_ProductVariant", false, (Guid)trackingGuid);

            return NoContent();
        }
        #endregion
    }
}
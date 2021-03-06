﻿using System;
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
    public class ProductCategoryController : ControllerBase
    {
        #region Private Properties
        private readonly Models.ContextModel _context;
        private readonly ICategoryAssignmentModel _categoryAssignment;
        #endregion

        #region Constructor(s)
        public ProductCategoryController(ICategoryAssignmentModel categoryAssignment, ILoggerManager logger, IOptions<Sheev.Common.Models.MongoDbSetting> mongoDbSettings, IOptions<Sheev.Common.Models.ApiUrlSetting> apiSettings)
        {
            _context = new Models.ContextModel(mongoDbSettings, apiSettings, logger);
            _categoryAssignment = categoryAssignment;
        }
        #endregion

        #region Get Call(s)
        /// <summary>
        /// Gets a Product Category By Id
        /// </summary>
        /// <param name="id">Product Category id</param>
        /// <param name="trackingGuid">Tracking Guid</param>
        /// <returns></returns>
        [HttpGet("ProductCategory/{id}")]
        [AuthorizeClaim("Product Category", K2SO.Auth.Constants.PermissionAccessType.VIEW)]
        public async Task<Product.Common.Models.ProductCategoryResponse> GetProductCategory(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            Utilities.RestErrorHandler.CheckId(id, 2, _context, (Guid)trackingGuid);

            var response = new Product.Common.Models.ProductCategoryResponse();
            response = await _categoryAssignment.GetById(_context, id, "PC_Product", (Guid)trackingGuid);

            return response;
        }
        #endregion

        #region Post Call(s)
        /// <summary>
        /// Adds new Product Category
        /// </summary>
        /// <param name="request"></param>
        [HttpPost("ProductCategory")]
        [AuthorizeClaim("Product Category", K2SO.Auth.Constants.PermissionAccessType.CREATE)]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<Product.Common.Models.ProductCategoryResponse> PostProductCategory([FromBody]Product.Common.Models.ProductCategoryRequest request)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            Guid trackingGuid = Guid.NewGuid();

            if (request == null)
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, null);
            //else
            //    trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, request.TrackingGuid);

            //Utilities.RestErrorHandler.CheckKitRequest(request, _context, trackingGuid);

            var response = new Product.Common.Models.ProductCategoryResponse();
            response = await _categoryAssignment.Save(_context, request, "PC_Product", trackingGuid);

            return response;
        }
        #endregion

        #region Delete Call(s)
        /// <summary>
        /// Removes a Product Category
        /// </summary>
        /// <param name="id">Category id</param>
        /// <param name="trackingGuid">Tracking Guid</param>
        /// <returns></returns>
        [HttpDelete("ProductCategory/{id}")]
        [AuthorizeClaim("Product Category", K2SO.Auth.Constants.PermissionAccessType.DELETE)]
        public async Task<NoContentResult> DeletesProductCategory(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            Utilities.RestErrorHandler.CheckId(id, 2, _context, (Guid)trackingGuid);

            await _categoryAssignment.DeleteOrReactivate(_context, id, "PC_Product", false, (Guid)trackingGuid);

            return NoContent();
        }
        #endregion
    }
}
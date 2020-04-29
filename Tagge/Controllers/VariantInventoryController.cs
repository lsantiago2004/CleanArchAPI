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
    public class VariantInventoryController : ControllerBase
    {
        #region Private Properties
        private readonly Models.ContextModel _context;
        private readonly IInventoryModel _inventoryModel;
        #endregion

        #region Constructor(s)
        public VariantInventoryController(IInventoryModel inventoryModel, ILoggerManager logger, IOptions<Sheev.Common.Models.MongoDbSetting> mongoDbSettings, IOptions<Sheev.Common.Models.ApiUrlSetting> apiSettings)
        {
            _context = new Models.ContextModel(mongoDbSettings, apiSettings, logger);
            _inventoryModel = inventoryModel;
        }
        #endregion

        #region Get Call(s)
        /// <summary>
        /// Get a list of inventories per variant
        /// </summary>
        /// <remarks>
        /// Gets a list of all inventories for a specific variant
        /// </remarks>
        /// <param name="id">product id</param>
        /// <param name="trackingGuid">Tracking Guid</param>
        /// <returns></returns>
        [HttpGet("Variant/Inventories")]
        [AuthorizeClaim("Product Variant Inventory", K2SO.Auth.Constants.PermissionAccessType.VIEW)]
        public async Task<List<Tagge.Common.Models.InventoryResponse>> GetAll(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            var response = new List<Tagge.Common.Models.InventoryResponse>();

            long longId = Utilities.RestErrorHandler.CheckId(id, _context, (Guid)trackingGuid);

            response = await _inventoryModel.GetAll(longId, "PC_ProductVariant", (Guid)trackingGuid);

            return response;
        }

        /// <summary>
        /// Gets an variant inventory record
        /// </summary>
        /// <remarks>
        /// Gets an variant inventory record based on the variant inventory id provided
        /// </remarks>
        /// <param name="id">inventory id</param>
        /// <param name="trackingGuid">Tracking Guid</param>
        /// <returns></returns>
        [HttpGet("Variant/Inventory/{id}")]
        [AuthorizeClaim("Product Variant Inventory", K2SO.Auth.Constants.PermissionAccessType.VIEW)]
        public async Task<Tagge.Common.Models.InventoryResponse> Get(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            long longId = Utilities.RestErrorHandler.CheckId(id, _context, (Guid)trackingGuid);

            var response = new Tagge.Common.Models.InventoryResponse();

            response = await _inventoryModel.GetById(longId, (Guid)trackingGuid);

            return response;
        }
        #endregion

        #region Post Call(s)
        /// <summary>
        /// Add new variant inventory
        /// </summary>
        /// <remarks>
        /// Adds a variant inventory 
        /// </remarks>
        /// <param name="id">product variant id</param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("Variant/Inventory")]
        [AuthorizeClaim("Product Variant Inventory", K2SO.Auth.Constants.PermissionAccessType.CREATE)]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<Tagge.Common.Models.InventoryResponse> Post(string id, [FromBody]Tagge.Common.Models.InventoryRequest request)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            Guid trackingGuid = Guid.NewGuid();

            if (request == null)
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, null);
            else
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, request.TrackingGuid);

            //Utilities.RestErrorHandler.CheckInventoryRequest(request, _context, trackingGuid);
            //long longId = Utilities.RestErrorHandler.CheckId(id, _context, trackingGuid);

            var response = new Tagge.Common.Models.InventoryResponse();

            response = await _inventoryModel.Save(request, "PC_ProductVariant", id, trackingGuid, true);

            return response;
        }
        #endregion

        #region Put Call(s)
        /// <summary>
        /// Update existing variant inventory
        /// </summary>
        /// <remarks>
        /// Updates an existing variant inventory based on the variant inventory id provided.
        /// </remarks>
        /// <param name="id">variant inventory id</param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("Variant/Inventory/{id}")]
        [AuthorizeClaim("Product Variant Inventory", K2SO.Auth.Constants.PermissionAccessType.EDIT)]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<Tagge.Common.Models.InventoryResponse> Put(string id, [FromBody]Tagge.Common.Models.InventoryRequest request)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            Guid trackingGuid = Guid.NewGuid();

            if (request == null)
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, null);
            else
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, request.TrackingGuid);


            long longId = Utilities.RestErrorHandler.CheckId(id, _context, trackingGuid);
            //Utilities.RestErrorHandler.CheckInventoryRequest(request, _context, trackingGuid);

            var response = new Tagge.Common.Models.InventoryResponse();

            response = await _inventoryModel.Update(request, longId,  trackingGuid, true);

            return response;
        }

        /// <summary>
        /// Reactivates a variant inventory
        /// </summary>
        /// <remarks>
        /// Reactivates a variant inventory by its variant inventory id
        /// </remarks>
        /// <param name="id">variant inventory Id</param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        [HttpPut("Variant/Inventory/Reactivate/{id}")]
        [AuthorizeClaim("Product Variant Inventory", K2SO.Auth.Constants.PermissionAccessType.EDIT)]
        public async Task<NoContentResult> ReactivateProduct(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            long longId = Utilities.RestErrorHandler.CheckId(id, _context, (Guid)trackingGuid);

            await _inventoryModel.DeleteOrReactivate(longId, true, (Guid)trackingGuid);

            return NoContent();
        }
        #endregion

        #region Delete Call(s)
        /// <summary>
        /// Remove a variant inventory
        /// </summary>
        /// <remarks>
        /// Removes a variant inventory by variant inventory id
        /// </remarks>
        /// <param name="id">variant inventory id</param>
        /// <param name="trackingGuid">activity tracking guid</param>
        /// <returns></returns>
        [HttpDelete("Variant/Inventory/{id}")]
        [AuthorizeClaim("Product Variant Inventory", K2SO.Auth.Constants.PermissionAccessType.DELETE)]
        public async Task<NoContentResult> Delete(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            long longId = Utilities.RestErrorHandler.CheckId(id, _context, (Guid)trackingGuid);

            await _inventoryModel.DeleteOrReactivate(longId, false, (Guid)trackingGuid);

            return NoContent();
        }
        #endregion
    }
}
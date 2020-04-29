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

namespace Product.Controllers
{
    [Route("v2/Catalog")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        #region Private Properties
        private readonly IBaseContextModel _context;
        private readonly IInventoryModel _inventoryModel;
        #endregion

        #region Constructor(s)
        public InventoryController(IInventoryModel inventoryModel, ILoggerManager logger, IOptions<Sheev.Common.Models.MongoDbSetting> mongoDbSettings, IOptions<Sheev.Common.Models.ApiUrlSetting> apiSettings)
        {
            _context = new Models.ContextModel(mongoDbSettings, apiSettings, logger);
            _inventoryModel = inventoryModel;
        }
        #endregion

        #region Get Call(s)
        /// <summary>
        /// Get a list of inventories per product
        /// </summary>
        /// <remarks>
        /// Gets a list of all inventories for a specific product
        /// </remarks>
        /// <param name="id">product id</param>
        /// <param name="trackingGuid">Tracking Guid</param>
        /// <returns></returns>
        [HttpGet("Inventories")]
        [AuthorizeClaim("Product Inventory", K2SO.Auth.Constants.PermissionAccessType.VIEW)]
        public async Task<List<Product.Common.Models.InventoryResponse>> GetAll(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            var response = new List<Product.Common.Models.InventoryResponse>();

            long longId = Utilities.RestErrorHandler.CheckId(id, _context, (Guid)trackingGuid);

            response = await _inventoryModel.GetAll(longId, "PC_Product", (Guid)trackingGuid);

            return response;
        }

        /// <summary>
        /// Gets an inventory record
        /// </summary>
        /// <remarks>
        /// Gets an inventory record based on the inventory id provided
        /// </remarks>
        /// <param name="id">inventory id</param>
        /// <param name="trackingGuid">Tracking Guid</param>
        /// <returns></returns>
        [HttpGet("Inventory/{id}")]
        [AuthorizeClaim("Product Inventory", K2SO.Auth.Constants.PermissionAccessType.VIEW)]
        public async Task<Product.Common.Models.InventoryResponse> Get(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            long longId = Utilities.RestErrorHandler.CheckId(id, _context, (Guid)trackingGuid);

            var response = new Product.Common.Models.InventoryResponse();

            response = await _inventoryModel.GetById(longId, (Guid)trackingGuid);

            return response;
        }
        #endregion

        #region Post Call(s)
        // POST api/values
        /// <summary>
        /// Add new inventory
        /// </summary>
        /// <remarks>
        /// Adds a inventory to a product based on the product id
        /// </remarks>
        /// <param name="id">product id</param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("Inventory")]
        [AuthorizeClaim("Product Inventory", K2SO.Auth.Constants.PermissionAccessType.CREATE)]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<Product.Common.Models.InventoryResponse> Post(string id, [FromBody]Product.Common.Models.InventoryRequest request)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            Guid trackingGuid = Guid.NewGuid();

            if (request == null)
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, null);
            else
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, request.TrackingGuid);

            //Utilities.RestErrorHandler.CheckInventoryRequest(request, _context, trackingGuid);
            long longId = Utilities.RestErrorHandler.CheckId(id, _context, trackingGuid);

            var response = new Product.Common.Models.InventoryResponse();

            response = await _inventoryModel.Save(request, "PC_Product", longId.ToString(), trackingGuid, true);

            return response;
        }
        #endregion

        #region Put Call(s)
        /// <summary>
        /// Update existing inventory
        /// </summary>
        /// <remarks>
        /// Updates an existing inventory based on the inventory id provided.
        /// </remarks>
        /// <param name="id">inventory id</param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("Inventory/{id}")]
        [AuthorizeClaim("Product Inventory", K2SO.Auth.Constants.PermissionAccessType.EDIT)]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<Product.Common.Models.InventoryResponse> Put(string id, [FromBody]Product.Common.Models.InventoryRequest request)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            Guid trackingGuid = Guid.NewGuid();

            if (request == null)
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, null);
            else
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, request.TrackingGuid);


            long longId = Utilities.RestErrorHandler.CheckId(id, _context, trackingGuid);
            //Utilities.RestErrorHandler.CheckInventoryRequest(request, _context, trackingGuid);

            var response = new Product.Common.Models.InventoryResponse();

            response = await _inventoryModel.Update(request, longId, trackingGuid, true);

            return response;
        }

        /// <summary>
        /// Reactivates a inventory
        /// </summary>
        /// <remarks>
        /// Reactivates a inventory by its inventory id
        /// </remarks>
        /// <param name="id">Inventory Id</param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        [HttpPut("Inventory/Reactivate/{id}")]
        [AuthorizeClaim("Product Inventory", K2SO.Auth.Constants.PermissionAccessType.EDIT)]
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
        /// Remove a inventory
        /// </summary>
        /// <remarks>
        /// Removes a inventory by inventory id
        /// </remarks>
        /// <param name="id">inventory id</param>
        /// <param name="trackingGuid">activity tracking guid</param>
        /// <returns></returns>
        [HttpDelete("Inventory/{id}")]
        [AuthorizeClaim("Product Inventory", K2SO.Auth.Constants.PermissionAccessType.DELETE)]
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
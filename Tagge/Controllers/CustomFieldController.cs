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
    [Route("v2")]
    [ApiController]
    public class CustomFieldController : ControllerBase
    {
        #region Private Properties
        private readonly Models.ContextModel _context;
        private readonly ICustomFieldModel _customFieldModel;
        #endregion

        #region Constructor(s)
        public CustomFieldController(ICustomFieldModel customFieldModel, ILoggerManager logger, IOptions<Sheev.Common.Models.MongoDbSetting> mongoDbSettings, IOptions<Sheev.Common.Models.ApiUrlSetting> apiSettings)
        {
            _context = new Models.ContextModel(mongoDbSettings, apiSettings, logger);
            _customFieldModel = customFieldModel;
        }
        #endregion

        #region Post Call(s)
        /// <summary>
        /// Adds new Product Custom Field
        /// </summary>
        /// <remarks>
        /// Adds a product custom field. <br/>
        /// ### Id ###
        /// - This is the id of the item that the custom field is being attached too
        /// ### Type ###
        /// - Product
        /// - Product Inventory
        /// - Product Variant
        /// - Product Variant Inventory
        /// ### Required Fields ###
        /// - Id 
        /// - Type
        /// - Custom Field Name
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost("CustomField/Catalog")]
        [AuthorizeClaim("Product Custom Field", K2SO.Auth.Constants.PermissionAccessType.CREATE)]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<Product.Common.Models.GenericCustomFieldResponse> PostCatalogCustomField([FromBody]Product.Common.Models.GenericCustomFieldRequest request)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            Guid trackingGuid = Guid.NewGuid();

            if (request == null)
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, null);

            Utilities.RestErrorHandler.CheckGenericCustomFieldRequest(request, _context, trackingGuid);

            var response = new Product.Common.Models.GenericCustomFieldResponse();
            response = await _customFieldModel.Save(request, trackingGuid);

            return response;
        }
        #endregion

        #region Put Call(s)
        /// <summary>
        /// Update existing Product Custom Field
        /// </summary>
        /// <param name="id">Product Custom Field id</param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("CustomField/Catalog/{id}")]
        [AuthorizeClaim("Product Custom Field", K2SO.Auth.Constants.PermissionAccessType.EDIT)]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<Product.Common.Models.GenericCustomFieldResponse> PutCatalogCustomField(string id, [FromBody]Product.Common.Models.GenericCustomFieldRequest request)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            Guid trackingGuid = Guid.NewGuid();

            if (request == null)
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, null);
            else
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, request.TrackingGuid);

            Utilities.RestErrorHandler.CheckGenericCustomFieldRequest(request, _context, trackingGuid);

            var response = new Product.Common.Models.GenericCustomFieldResponse();
            response = await _customFieldModel.Update(id, request, trackingGuid);

            return response;
        }
        #endregion

        #region Delete Call(s)
        /// <summary>
        /// Removes a Product Custom Field
        /// </summary>
        /// <remarks>
        ///  ### Type ###
        /// - Product
        /// - Product Inventory
        /// - Product Variant
        /// - Product Variant Inventory
        /// </remarks>
        /// <param name="id">Product Custom Field id</param>
        /// <param name="type"></param>
        /// <param name="trackingGuid">Tracking Guid</param>
        /// <returns></returns>
        [HttpDelete("CustomField/Catalog/{id}/{type}")]
        [AuthorizeClaim("Product Custom Field", K2SO.Auth.Constants.PermissionAccessType.DELETE)]
        public async Task<NoContentResult> DeleteCatalogCustomField(string id, string type, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            await _customFieldModel.DeleteOrReactivate(id, type, false, (Guid)trackingGuid);

            return NoContent();
        }
        #endregion
    }
}
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
    [Route("v2/External")]
    [ApiController]
    public class ExternalIdController : ControllerBase
    {
        #region Private Properties
        private readonly IBaseContextModel _context;
        private readonly IExternalIdModel _externalIdModel;
        #endregion

        #region Constructor(s)
        public ExternalIdController(IBaseContextModel context, IExternalIdModel externalIdModel, ILoggerManager logger, IOptions<Sheev.Common.Models.MongoDbSetting> mongoDbSettings, IOptions<Sheev.Common.Models.ApiUrlSetting> apiSettings)
        {
            _context = context; //new Models.ContextModel(mongoDbSettings, apiSettings, logger);
            _externalIdModel = externalIdModel;
        }
        #endregion

        #region Get Call(s)
        /// <summary>
        /// Get External Id for a System
        /// </summary>
        /// <param name="id">iPaaS id</param>
        /// <param name="systemId">System Id for your specific website</param>
        /// <param name="tableName">Table Name: Product or Variant</param>
        /// <param name="trackingGuid">Tracking Guid</param>
        /// <returns></returns>
        [HttpGet("LookupExternal/{id}/{systemId}/{tableName}")]
        [AuthorizeClaim("External Id", K2SO.Auth.Constants.PermissionAccessType.VIEW)]
        public async Task<string> GetExternalIds(string id, string systemId, string tableName, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            long longSystemId = Utilities.RestErrorHandler.CheckId(systemId, _context, (Guid)trackingGuid);

            var response = "I have graphs and the command.";
            response = await _externalIdModel.GetExternalId(id, longSystemId, tableName, (Guid)trackingGuid);

            return response;
        }

        /// <summary>
        /// Get Spaceport Id 
        /// </summary>
        /// <remarks>
        /// Gets the external id for a product/variant/order/customer by C5 System Id and Table name.  <br />
        /// System Id for your specific website <br />
        /// Table Names: Product or Variant
        /// </remarks>
        /// <param name="systemId">System Id</param>
        /// <param name="tableName">Table Name: Product, Variant, Customer, Transaction, etc...</param>
        /// <param name="externalid">External Id</param>
        /// <param name="trackingGuid">Tracking Guid</param>
        /// <returns></returns>
        [HttpGet("LookupSpaceport/{systemId}/{tableName}/{*externalid}")]
        [AuthorizeClaim("External Id", K2SO.Auth.Constants.PermissionAccessType.VIEW)]
        public async Task<string> GetSpaceportId(string systemId, string tableName, string externalid, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            long longSystemId = Utilities.RestErrorHandler.CheckId(systemId, _context, (Guid)trackingGuid);

            var response = "If the Rebels have obtained a complete technical readout of this battlestation it is possible—however unlikely—that they might find a weakness, and exploit it"; ;
            response = await _externalIdModel.GetInternalId(externalid, longSystemId, tableName, (Guid)trackingGuid);

            return response;
        }

        /// <summary>
        /// Get Missing Products for a System
        /// </summary>
        /// <param name="systemId">System Id for your specific website</param>
        /// <param name="tablename">Table Name: Product </param>
        /// <returns></returns>
        [HttpGet("Missing/{systemId}/{tableName}")]
        [AuthorizeClaim("External Id", K2SO.Auth.Constants.PermissionAccessType.VIEW)]
        public async Task<Product.Common.Models.ProductCategoryResponse> GetMissing(string systemId, string tableName)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            //trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            //long longId = Utilities.RestErrorHandler.CheckId(id, _context, (Guid)trackingGuid);

            var response = new Product.Common.Models.ProductCategoryResponse();
            //response = await KitModel.GetById(longId, _context, (Guid)trackingGuid);

            return response;
        }
        #endregion

        #region Post Call(s)
        /// <summary>
        /// Get Spaceport Id 
        /// </summary>
        /// <remarks>
        /// Gets the external id for a product/variant/order/customer by iPaaS System Id and Table name.  <br />
        /// System Id for your specific website <br />
        /// Table Name: Product, Variant, Customer, Transaction, etc...
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost("LookupSpecial")]
        [AuthorizeClaim("External Id", K2SO.Auth.Constants.PermissionAccessType.CREATE)]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<string> GetSpaceportId([FromBody]Sheev.Common.Models.ExternalIdSpecialRequest request)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            Guid trackingGuid = Guid.NewGuid();

            if (request == null)
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, null);
            else
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, request.TrackingGuid);

            Utilities.RestErrorHandler.CheckExternalIdSpecialRequest(request, _context, trackingGuid);

            var response = await _externalIdModel.GetInternalId(request.ExternalId, request.SystemId, request.TableName, trackingGuid);

            return response;
        }

        /// <summary>
        /// Add/Updates external id by system id
        /// </summary>
        /// <remarks>
        /// Add/Updates a new external id for a product/order/customer <br /><br />
        /// <b>Required Fields:</b>
        /// - System Id: iPaaS Id for your specific system 
        /// - Internal Id: the iPaaS id of a product, order, customer etc.. 
        /// - Table Name: Product, Variant, Customer, Transaction, etc...
        /// - External Id: the External Id for a system other than iPaaS
        /// </remarks>
        /// <param name="request"></param>
        [HttpPost("UpdateExternalId")]
        [AuthorizeClaim("External Id", K2SO.Auth.Constants.PermissionAccessType.CREATE)]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<Product.Common.Models.ExternalIdResponse> UpdateExternalId([FromBody]Product.Common.Models.ExternalIdRequest request)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            Guid trackingGuid = Guid.NewGuid();

            if (request == null)
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, null);
            else
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, request.TrackingGuid);

            Utilities.RestErrorHandler.CheckExternalIdRequest(request, _context, trackingGuid);

            var response = new Product.Common.Models.ExternalIdResponse();
            response = await _externalIdModel.Update(request, (Guid)trackingGuid);

            return response;
        }
        #endregion

        #region Delete Call(s)
        /// <summary>
        /// Deletes external id by system id
        /// </summary>
        /// <remarks>
        /// Deletes an external id for a product/order/customer <br />
        /// System Id for your specific website <br />
        /// Table Names: Product, Transaction, Customer, etc...
        /// </remarks>
        /// <returns></returns>
        [HttpDelete("")]
        [AuthorizeClaim("External Id", K2SO.Auth.Constants.PermissionAccessType.DELETE)]
        public async Task<NoContentResult> DeleteExternalId(Product.Common.Models.ExternalIdRequest request)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            Guid trackingGuid = Guid.NewGuid();

            if (request == null)
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, null);
            else
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, request.TrackingGuid);

            if (request == null)
            {
                IG2000.Data.Utilities.Logging.LogTrackingEvent($"Request is null or empty. This could be due to an invalid json structure", $"Status Code: 400 Bad Request", LT319.Common.Utilities.Constants.TrackingStatus.Error, _context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = "Request is null or empty. This could be due to an invalid json structure" };
            }

            await _externalIdModel.DeleteOrReactivate(request, false, trackingGuid);

            return NoContent();
        }
        #endregion
    }
}
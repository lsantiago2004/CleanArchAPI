using System;
using System.Collections.Generic;
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
    public class AlternateIdTypeController : ControllerBase
    {
        #region Private Properties
        ////private readonly Models.ContextModel _context;
        private readonly IBaseContextModel _context;
        private readonly IAlternateIdTypeModel _alternateTypeId;
        #endregion

        #region Constructor(s)
        public AlternateIdTypeController(IBaseContextModel contextModel, IAlternateIdTypeModel alternateTypeId, ILoggerManager logger, IOptions<Sheev.Common.Models.MongoDbSetting> mongoDbSettings, IOptions<Sheev.Common.Models.ApiUrlSetting> apiSettings)
        {
            _context = contextModel; //new Models.ContextModel(mongoDbSettings, apiSettings, logger);
            _context = new Models.ContextModel(mongoDbSettings, apiSettings, logger);
            _alternateTypeId = alternateTypeId;
        }
        #endregion

        #region Get Call(s)
        /// <summary>
        /// Gets All Alternate Id Types
        /// </summary>
        /// <returns></returns>
        /// <param name="pageNumber">Parameter is passed from Query string if it is null then it default Value will be pageNumber: 1</param>
        /// <param name="pageSize">Parameter is passed from Query string if it is null then it default Value will be pageSize: 100</param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet("AlternateIdTypes")]
        [AuthorizeClaim("Alternate Id Type", K2SO.Auth.Constants.PermissionAccessType.VIEW)]
        public List<Sheev.Common.Models.GetAllResponse> GetAllAlternateIdTypes([FromQuery]int? pageNumber = 1, int? pageSize = 100, string filter = "")
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            var response = new List<Sheev.Common.Models.GetAllResponse>();
            long count = 0;

            // Get's No of Rows Count   
            // Get the rows
            response = _alternateTypeId.GetAll(_context, filter, ref count, pageNumber, pageSize);

            // Display TotalCount to Records to User  
            long totalCount = count;

            // Calculating Totalpage by Dividing (No of Records / Pagesize)  
            int totalPages = (int)Math.Ceiling(count / (double)pageSize);

            // if CurrentPage is greater than 1 means it has previousPage  
            var previous_Page = (int)pageNumber > 1 ? "Yes" : "No";

            // if TotalPages is greater than CurrentPage means it has nextPage  
            var next_Page = (int)pageNumber < totalPages ? "Yes" : "No";

            // Object which we are going to send in header   
            var paginationMetadata = new
            {
                total_Count = totalCount,
                page_Size = pageSize,
                current_Page = pageNumber,
                total_Pages = totalPages,
                previous_Page,
                next_Page
            };

            // Setting Header  
            HttpContext.Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

            // Dummy Code that should be removed
            HttpContext.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, X-Pagination");
            HttpContext.Response.Headers.Add("Access-Control-Expose-Headers", "X-Pagination");

            return response;
        }

        /// <summary>
        /// Gets a Alternate Id Type By Id
        /// </summary>
        /// <param name="id">Alternate Id Type id</param>
        /// <param name="trackingGuid">Tracking Guid</param>
        /// <returns></returns>
        [HttpGet("AlternateIdType/{id}")]
        [AuthorizeClaim("Alternate Id Type", K2SO.Auth.Constants.PermissionAccessType.VIEW)]
        public async Task<Product.Common.Models.AlternateIdTypeResponse> GetAlternateIdType(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            long longId = Utilities.RestErrorHandler.CheckId(id, _context, (Guid)trackingGuid);

            var response = new Product.Common.Models.AlternateIdTypeResponse();
            response = await _alternateTypeId.GetById(_context, longId, (Guid)trackingGuid);

            return response;
        }
        #endregion

        #region Post Call(s)
        /// <summary>
        /// Adds new Alternate Id Type
        /// </summary>
        /// <param name="request"></param>
        [HttpPost("AlternateIdType")]
        [AuthorizeClaim("Alternate Id Type", K2SO.Auth.Constants.PermissionAccessType.CREATE)]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<Product.Common.Models.AlternateIdTypeResponse> PostAlternateIdType([FromBody]Product.Common.Models.AlternateIdTypeRequest request)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            Guid trackingGuid = Guid.NewGuid();

            if (request == null)
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, null);
            else
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, request.TrackingGuid);

            //Utilities.RestErrorHandler.CheckAlternateIdRequest(request, _context, trackingGuid);

            var response = new Product.Common.Models.AlternateIdTypeResponse();
            response = await _alternateTypeId.Save(_context, request, trackingGuid);

            return response;
        }
        #endregion

        #region Put Call(s)
        /// <summary>
        /// Update existing AlternateIdType
        /// </summary>
        /// <param name="id">Alternate Id Type id</param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("AlternateIdType/{id}")]
        [AuthorizeClaim("Alternate Id Type", K2SO.Auth.Constants.PermissionAccessType.EDIT)]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<Product.Common.Models.AlternateIdTypeResponse> PutAlternateIdType(string id, [FromBody]Product.Common.Models.AlternateIdTypeRequest request)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            Guid trackingGuid = Guid.NewGuid();

            if (request == null)
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, null);
            else
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, request.TrackingGuid);

            //Utilities.RestErrorHandler.CheckCategoryRequest(request, _context, trackingGuid);

            long longId = Utilities.RestErrorHandler.CheckId(id, _context, trackingGuid);

            var response = new Product.Common.Models.AlternateIdTypeResponse();
            response = await _alternateTypeId.Update(_context, longId, request, trackingGuid);

            return response;
        }

        /// <summary>
        /// Reactivates a Alternate Id Type
        /// </summary>
        /// <remarks>
        /// Reactivates a AlternateIdType by its id
        /// </remarks>
        /// <param name="id">Alternate Id Type id</param>
        /// <param name="trackingGuid">Tracking Guid</param>
        /// <returns></returns>
        [HttpPut("AlternateIdType/Reactivate/{id}")]
        [AuthorizeClaim("Alternate Id Type", K2SO.Auth.Constants.PermissionAccessType.EDIT)]
        public async Task<NoContentResult> ReactivateAlternateIdType(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            long longId = Utilities.RestErrorHandler.CheckId(id, _context, (Guid)trackingGuid);

            await _alternateTypeId.DeleteOrReactivate(_context, longId, true, (Guid)trackingGuid);

            return NoContent();
        }
        #endregion

        #region Delete Call(s)
        /// <summary>
        /// Removes a Alternate Id Type
        /// </summary>
        /// <param name="id">Alternate Id Type id</param>
        /// <param name="trackingGuid">Tracking Guid</param>
        /// <returns></returns>
        [HttpDelete("AlternateIdType/{id}")]
        [AuthorizeClaim("Alternate Id Type", K2SO.Auth.Constants.PermissionAccessType.DELETE)]
        public async Task<NoContentResult> DeleteAlternateIdType(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            long longId = Utilities.RestErrorHandler.CheckId(id, _context, (Guid)trackingGuid);

            await _alternateTypeId.DeleteOrReactivate(_context, longId, false, (Guid)trackingGuid);

            return NoContent();
        }
        #endregion
    }
}
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Sheev.Common.Logger;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tagge.Authentication;
using Tagge.Filters;
using Tagge.Models;
using Tagge.Models.Interfaces;

namespace Tagge.Controllers
{
    [Route("v2/Catalog")]
    [ApiController]
    public class LocationGroupController : ControllerBase
    {
        #region Private Properties
        private readonly Models.ContextModel _context;
        private readonly ILocationGroupModel _locationGroupModel;
        #endregion

        #region Constructor(s)
        public LocationGroupController(ILocationGroupModel locationGroupModel, ILoggerManager logger, IOptions<Sheev.Common.Models.MongoDbSetting> mongoDbSettings, IOptions<Sheev.Common.Models.ApiUrlSetting> apiSettings)
        {
            _context = new Models.ContextModel(mongoDbSettings, apiSettings, logger);
            _locationGroupModel = locationGroupModel;
        }
        #endregion

        #region Get Call(s)
        /// <summary>
        /// Gets All Location Groups 
        /// </summary>
        /// <remarks>
        /// Adding an id to this call will remove that Location Group from the returned list. If the id is not added or is less than or equal to 0 then all location groups will be returned.
        /// </remarks>
        /// <returns></returns>
        /// <param name="id">Parameter is passed from Query string if it is null then it default Value will be id: 0</param>
        /// <param name="pageNumber">Parameter is passed from Query string if it is null then it default Value will be pageNumber: 1</param>
        /// <param name="pageSize">Parameter is passed from Query string if it is null then it default Value will be pageSize: 100</param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet("LocationGroups")]
        [AuthorizeClaim("Location", K2SO.Auth.Constants.PermissionAccessType.VIEW)]
        public List<Sheev.Common.Models.GetAllResponse> GetAllLocationGroups([FromQuery]int? pageNumber = 1, int? pageSize = 100, string filter = "")
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            var response = new List<Sheev.Common.Models.GetAllResponse>();
            long count = 0;

            // Get's No of Rows Count   
            // Get the rows
            response = _locationGroupModel.GetAll(filter, ref count, pageNumber, pageSize);

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
        /// Gets a Location By Id
        /// </summary>
        /// <param name="id">location group id</param>
        /// <param name="trackingGuid">Tracking Guid</param>
        /// <returns></returns>
        [HttpGet("LocationGroup/{id}")]
        [AuthorizeClaim("Location", K2SO.Auth.Constants.PermissionAccessType.VIEW)]
        public async Task<Tagge.Common.Models.LocationGroupResponse> GetLocationGroup(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            long longId = Utilities.RestErrorHandler.CheckId(id, _context, (Guid)trackingGuid);

            var response = new Tagge.Common.Models.LocationGroupResponse();

            response = await _locationGroupModel.GetById(longId, (Guid)trackingGuid);

            return response;
        }
        #endregion

        #region Post Call(s)
        /// <summary>
        /// Adds a new location group
        /// </summary>
        /// <remarks>
        /// Adds a new location group record  
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("LocationGroup")]
        [AuthorizeClaim("Location", K2SO.Auth.Constants.PermissionAccessType.CREATE)]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<Tagge.Common.Models.LocationGroupResponse> PostLocationGroup(Tagge.Common.Models.LocationGroupRequest request)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);

            Guid trackingGuid = Guid.NewGuid();

            if (request == null)
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, null);
            else
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, request.TrackingGuid);

            var response = new Tagge.Common.Models.LocationGroupResponse();

            response = await _locationGroupModel.Save(request, trackingGuid);

            return response;
        }
        #endregion

        #region Put Call(s)
        /// <summary>
        /// Adds/Update a location group record
        /// </summary>
        /// <remarks>
        /// Adds/Update a location group record based on the location group id and/or request provided
        /// </remarks>
        /// <param name="id">location group id</param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("LocationGroup/{id}")]
        [AuthorizeClaim("Location", K2SO.Auth.Constants.PermissionAccessType.CREATE)]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<Tagge.Common.Models.LocationGroupResponse> PutLocationGroup(string id, Tagge.Common.Models.LocationGroupRequest request)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            Guid trackingGuid = Guid.NewGuid();

            if (request == null)
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, null);
            else
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, request.TrackingGuid);

            long longId = Utilities.RestErrorHandler.CheckId(id, _context, trackingGuid);

            var response = new Tagge.Common.Models.LocationGroupResponse();

            response = await _locationGroupModel.Update(longId, request, trackingGuid);

            return response;
        }

        /// <summary>
        /// Reactivates Location Group
        /// </summary>
        /// <remarks>
        /// Reactivates a Location Group by its Id
        /// </remarks>
        /// <param name="id">Location Group Id</param>
        /// <param name="trackingGuid">Tracking Guid</param>
        /// <returns></returns>
        [HttpPut("LocationGroup/Reactivate/{id}")]
        [AuthorizeClaim("Location", K2SO.Auth.Constants.PermissionAccessType.EDIT)]
        public async Task<NoContentResult> ReactivateLocation(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);
            long longId = Utilities.RestErrorHandler.CheckId(id, _context, (Guid)trackingGuid);

            await _locationGroupModel.DeleteOrReactivate(longId, true, (Guid)trackingGuid);

            return NoContent();
        }
        #endregion

        #region Delete Call(s)
        /// <summary>
        /// Removes a Location Group
        /// </summary>
        /// <param name="id">location group id</param>
        /// <param name="trackingGuid">Tracking Guid</param>
        /// <returns></returns>
        [HttpDelete("LocationGroup/{id}")]
        [AuthorizeClaim("Location", K2SO.Auth.Constants.PermissionAccessType.DELETE)]
        public async Task<NoContentResult> DeleteLocationGroup(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);
            long longId = Utilities.RestErrorHandler.CheckId(id, _context, (Guid)trackingGuid);

            await _locationGroupModel.DeleteOrReactivate(longId, false, (Guid)trackingGuid);

            return NoContent();
        }
        #endregion
    }
}
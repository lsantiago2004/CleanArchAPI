using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Sheev.Common.Logger;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Product.Authentication;
using Product.Filters;
using Product.Models;
using Product.Models.Interfaces;

namespace Product.Controllers
{
    [Route("v2/Catalog")]
    [ApiController]
    public class CategorySetController : ControllerBase
    {
        #region Private Properties
        private readonly ContextModel _context;
        private readonly ICategorySetModel _categorySet;
        #endregion

        #region Constructor(s)
        public CategorySetController(ICategorySetModel categorySet, ILoggerManager logger, IOptions<Sheev.Common.Models.MongoDbSetting> mongoDbSettings, IOptions<Sheev.Common.Models.ApiUrlSetting> apiSettings)
        {
            _context = new Models.ContextModel(mongoDbSettings, apiSettings, logger);
            _categorySet = categorySet;
        }
        #endregion

        #region Get Call(s)
        /// <summary>
        /// Gets All Category Set
        /// </summary>
        /// <remarks>
        /// Adding an id to this call will remove that Location from the returned list. If the id is not added or is less than or equal to 0 then all locations will be returned.
        /// </remarks>
        /// <returns></returns>
        /// <param name="id">Parameter is passed from Query string if it is null then it default Value will be id: 0</param>
        /// <param name="pageNumber">Parameter is passed from Query string if it is null then it default Value will be pageNumber: 1</param>
        /// <param name="pageSize">Parameter is passed from Query string if it is null then it default Value will be pageSize: 100</param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet("CategorySets")]
        [AuthorizeClaim("Product Category", K2SO.Auth.Constants.PermissionAccessType.VIEW)]
        public List<Sheev.Common.Models.GetAllResponse> GetAllCategorySets([FromQuery]int? pageNumber = 1, int? pageSize = 100, string filter = "")
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            var response = new List<Sheev.Common.Models.GetAllResponse>();
            long count = 0;

            // Get's No of Rows Count   
            // Get the rows
            response = _categorySet.GetAll(_context, filter, ref count, pageNumber, pageSize);

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
        /// Gets a Category Set By Id
        /// </summary>
        /// <param name="id">Category Set id</param>
        /// <param name="trackingGuid">Tracking Guid</param>
        /// <returns></returns>
        [HttpGet("CategorySet/{id}")]
        [AuthorizeClaim("Product Category", K2SO.Auth.Constants.PermissionAccessType.VIEW)]
        public async Task<Product.Common.Models.CategorySetResponse> GetCategorySet(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            long longId = Utilities.RestErrorHandler.CheckId(id, _context, (Guid)trackingGuid);

            var response = new Product.Common.Models.CategorySetResponse();

            response = await _categorySet.GetById(_context, longId, (Guid)trackingGuid);

            return response;
        }
        #endregion

        #region Post Call(s)
        /// <summary>
        /// Adds a new category set
        /// </summary>
        /// <remarks>
        /// Adds a new category set record  
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("CategorySet")]
        [AuthorizeClaim("Product Category", K2SO.Auth.Constants.PermissionAccessType.CREATE)]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<Product.Common.Models.CategorySetResponse> PostCategorySet(Product.Common.Models.CategorySetRequest request)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);

            Guid trackingGuid = Guid.NewGuid();

            if (request == null)
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, null);
            else
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, request.TrackingGuid);

            var response = new Product.Common.Models.CategorySetResponse();

            response = await _categorySet.Save(_context, request, trackingGuid);

            return response;
        }
        #endregion

        #region Put Call(s)
        /// <summary>
        /// Adds/Update a category set record
        /// </summary>
        /// <remarks>
        /// Adds/Update a category set record based on the category set id and/or request provided
        /// </remarks>
        /// <param name="id">category set id</param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("CategorySet/{id}")]
        [AuthorizeClaim("Product Category", K2SO.Auth.Constants.PermissionAccessType.CREATE)]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<Product.Common.Models.CategorySetResponse> PutCategorySet(string id, Product.Common.Models.CategorySetRequest request)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            Guid trackingGuid = Guid.NewGuid();

            if (request == null)
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, null);
            else
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, request.TrackingGuid);

            long longId = Utilities.RestErrorHandler.CheckId(id, _context, trackingGuid);

            var response = new Product.Common.Models.CategorySetResponse();

            response = await _categorySet.Update(_context, longId, request, trackingGuid);

            return response;
        }

        /// <summary>
        /// Reactivates Location
        /// </summary>
        /// <remarks>
        /// Reactivates a category set by its Id
        /// </remarks>
        /// <param name="id">category set Id</param>
        /// <param name="trackingGuid">Tracking Guid</param>
        /// <returns></returns>
        [HttpPut("CategorySet/Reactivate/{id}")]
        [AuthorizeClaim("Product Category", K2SO.Auth.Constants.PermissionAccessType.EDIT)]
        public async Task<NoContentResult> ReactivateLocation(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);
            long longId = Utilities.RestErrorHandler.CheckId(id, _context, (Guid)trackingGuid);

            await _categorySet.DeleteOrReactivate(_context, longId, true, (Guid)trackingGuid);

            return NoContent();
        }
        #endregion

        #region Delete Call(s)
        /// <summary>
        /// Removes a Category Set
        /// </summary>
        /// <param name="id">category set id</param>
        /// <param name="trackingGuid">Tracking Guid</param>
        /// <returns></returns>
        [HttpDelete("CategorySet/{id}")]
        [AuthorizeClaim("Product Category", K2SO.Auth.Constants.PermissionAccessType.DELETE)]
        public async Task<NoContentResult> DeleteCategorySet(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);
            long longId = Utilities.RestErrorHandler.CheckId(id, _context, (Guid)trackingGuid);

            await _categorySet.DeleteOrReactivate(_context, longId, false, (Guid)trackingGuid);

            return NoContent();
        }
        #endregion
    }
}
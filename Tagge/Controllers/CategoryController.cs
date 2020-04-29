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
    [Route("v2/Catalog")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        #region Private Properties
        private readonly Models.ContextModel _context;
        private readonly ICategoryModel _categoryModel;

        #endregion

        #region Constructor(s)
        public CategoryController(ICategoryModel categoryModel, ILoggerManager logger, IOptions<Sheev.Common.Models.MongoDbSetting> mongoDbSettings, IOptions<Sheev.Common.Models.ApiUrlSetting> apiSettings)
        {
            _context = new Models.ContextModel(mongoDbSettings, apiSettings, logger);
            _context.NLogger = logger;
            _categoryModel = categoryModel;
        }
        #endregion

        #region Get Call(s)
        /// <summary>
        /// Gets All Categories
        /// </summary>
        /// <returns></returns>
        /// <param name="pageNumber">Parameter is passed from Query string if it is null then it default Value will be pageNumber: 1</param>
        /// <param name="pageSize">Parameter is passed from Query string if it is null then it default Value will be pageSize: 100</param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet("Categories")]
        [AuthorizeClaim("Product Category", K2SO.Auth.Constants.PermissionAccessType.VIEW)]
        public List<Sheev.Common.Models.GetAllResponse> GetAllCategories([FromQuery]int? pageNumber = 1, int? pageSize = 100, string filter = "")
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            var response = new List<Sheev.Common.Models.GetAllResponse>();
            long count = 0;

            // Get's No of Rows Count   
            // Get the rows
            response = _categoryModel.GetAll(_context, filter, ref count, pageNumber, pageSize);

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
        /// Gets a Category By Id
        /// </summary>
        /// <param name="id">category id</param>
        /// <param name="trackingGuid">Tracking Guid</param>
        /// <returns></returns>
        [HttpGet("Category/{id}")]
        [AuthorizeClaim("Product Category", K2SO.Auth.Constants.PermissionAccessType.VIEW)]
        public async Task<Product.Common.Models.CategoryResponse> GetCategory(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            long longId = Utilities.RestErrorHandler.CheckId(id, _context, (Guid)trackingGuid);

            var response = new Product.Common.Models.CategoryResponse();
            response = await _categoryModel.GetById(_context, longId, (Guid)trackingGuid);

            return response;
        }
        #endregion

        #region Post Call(s)
        /// <summary>
        /// Adds new Category
        /// </summary>
        /// <param name="request"></param>
        [HttpPost("Category")]
        [AuthorizeClaim("Product Category", K2SO.Auth.Constants.PermissionAccessType.CREATE)]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<Product.Common.Models.CategoryResponse> PostCategory([FromBody]Product.Common.Models.CategoryRequest request)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            Guid trackingGuid = Guid.NewGuid();

            if (request == null)
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, null);
            else
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, request.TrackingGuid);

            Utilities.RestErrorHandler.CheckCategoryRequest(request, _context, trackingGuid);

            var response = new Product.Common.Models.CategoryResponse();
            response = await _categoryModel.Save(_context, request, trackingGuid);

            return response;
        }
        #endregion

        #region Put Call(s)
        /// <summary>
        /// Update existing Category
        /// </summary>
        /// <param name="id">category id</param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("Category/{id}")]
        [AuthorizeClaim("Product Category", K2SO.Auth.Constants.PermissionAccessType.EDIT)]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<Product.Common.Models.CategoryResponse> PutCategory(string id, [FromBody]Product.Common.Models.CategoryRequest request)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            Guid trackingGuid = Guid.NewGuid();

            if (request == null)
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, null);
            else
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, request.TrackingGuid);

            Utilities.RestErrorHandler.CheckCategoryRequest(request, _context, trackingGuid);

            long longId = Utilities.RestErrorHandler.CheckId(id, _context, trackingGuid);

            var response = new Product.Common.Models.CategoryResponse();
            response = await _categoryModel.Update(_context, longId, request, trackingGuid);

            return response;
        }

        /// <summary>
        /// Reactivates a Category
        /// </summary>
        /// <remarks>
        /// Reactivates a category by its id
        /// </remarks>
        /// <param name="id">category id</param>
        /// <param name="trackingGuid">Tracking Guid</param>
        /// <returns></returns>
        [HttpPut("Category/Reactivate/{id}")]
        [AuthorizeClaim("Product Category", K2SO.Auth.Constants.PermissionAccessType.EDIT)]
        public async Task<NoContentResult> ReactivateCategory(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            long longId = Utilities.RestErrorHandler.CheckId(id, _context, (Guid)trackingGuid);

            await _categoryModel.DeleteOrReactivate(_context, longId, true,(Guid)trackingGuid);

            return NoContent();
        }
        #endregion

        #region Delete Call(s)
        // DELETE api/values/5
        /// <summary>
        /// Removes a Category
        /// </summary>
        /// <param name="id">category id</param>
        /// <param name="trackingGuid">Tracking Guid</param>
        /// <returns></returns>
        [HttpDelete("Category/{id}")]
        [AuthorizeClaim("Product Category", K2SO.Auth.Constants.PermissionAccessType.DELETE)]
        public async Task<NoContentResult> DeleteCategory(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            long longId = Utilities.RestErrorHandler.CheckId(id, _context, (Guid)trackingGuid);

            await _categoryModel.DeleteOrReactivate(_context, longId, false, (Guid)trackingGuid);

            return NoContent();
        }
        #endregion
    }
}
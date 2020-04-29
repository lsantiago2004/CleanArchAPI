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
    /// <summary>
    /// Hello
    /// </summary>
    [Route("v2/Catalog")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        #region Private Properties
        private readonly Models.ContextModel _context;
        private readonly IProductModel _productModel;
        #endregion

        #region Constructor(s)
        public ProductController(IProductModel productModel, ILoggerManager logger, IOptions<Sheev.Common.Models.MongoDbSetting> mongoDbSettings, IOptions<Sheev.Common.Models.ApiUrlSetting> apiSettings)
        {
            _context = new Models.ContextModel(mongoDbSettings, apiSettings, logger);
            _productModel = productModel;
        }
        #endregion

        #region Get Call(s)
        /// <summary>
        /// Get a list of products
        /// </summary>
        /// <remarks>
        /// Gets a list of all products in iPaaS with optional parameter of active flag to return inactive products
        /// ### Active Flag ###
        /// - ?activeFlag=false: returns all inactive products
        /// ### Filter ###
        /// - The optional filter parameter is a search feature that will only bring back records containing that search term
        /// </remarks>
        /// <param name="pageNumber">Parameter is passed from Query string if it is null then it default Value will be pageNumber: 1</param>
        /// <param name="pageSize">Parameter is passed from Query string if it is null then it default Value will be pageSize: 100</param>
        /// <param name="filter">Parameter is passed from Query string if it is null then the filter will not apply. The filter will search the following columns: Id, Name, Value.</param>
        /// <returns></returns>
        [HttpGet("Products")]
        [AuthorizeClaim("Product", K2SO.Auth.Constants.PermissionAccessType.VIEW)]
        public async Task<List<Sheev.Common.Models.GetAllResponse>> GetAll([FromQuery]int? pageNumber = 1, int? pageSize = 100, string filter = "")
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            var response = new List<Sheev.Common.Models.GetAllResponse>();
            long count = 0;

            // Get's No of Rows Count   
            // Get the rows
            response = _productModel.GetAllProducts(filter, ref count, pageNumber, pageSize);

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
        /// Get product by id
        /// </summary>
        /// <param name="id">product id</param>
        /// <param name="trackingGuid">Tracking Guid</param>
        /// <returns></returns>
        [HttpGet("Product/{id}")]
        [AuthorizeClaim("Product", K2SO.Auth.Constants.PermissionAccessType.VIEW)]
        public async Task<Product.Common.Models.ProductResponse> Get(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            long longId = Utilities.RestErrorHandler.CheckId(id, _context, (Guid)trackingGuid);

            var response = new Product.Common.Models.ProductResponse();

            response = await _productModel.GetProductById(longId, (Guid)trackingGuid);

            return response;
        }
        #endregion

        #region Post Call(s)
        // POST api/values
        /// <summary>
        /// Add new product
        /// </summary>
        /// <remarks>
        /// Adds a product 
        /// </remarks>
        /// <param name="request"></param>
        /// <param name="useMongo"></param>
        /// <returns></returns>
        [HttpPost("Product")]
        [AuthorizeClaim("Product", K2SO.Auth.Constants.PermissionAccessType.CREATE)]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<Product.Common.Models.ProductResponse> Post([FromBody]Product.Common.Models.ProductRequest request)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            Guid trackingGuid = Guid.NewGuid();

            if (request == null)
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, null);
            else
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, request.TrackingGuid);

            Utilities.RestErrorHandler.CheckProductRequest(request, _context, trackingGuid);

            var response = new Product.Common.Models.ProductResponse();

            response = await _productModel.Save(request, trackingGuid);

            return response;
        }

        // POST v1/Bulk/Products
        /// <summary>
        /// Add/Update a list of Products
        /// </summary>
        /// <remarks>
        /// Add/Update a list of Products
        /// </remarks>
        /// <param name="request">List of ProductRequest</param>
        /// <returns>DockingBay94.Models.BulkResponse</returns>
        //[HttpPost("Products")]
        //[AuthorizeClaim("Role", K2SO.Auth.Constants.PermissionAccessType.CREATE)]
        //public async Task<Sheev.Common.Models.BulkResponse> PostBulkProducts(List<Product.Common.Models.ProductRequest> request)
        //{
        //    //////***************************************************************
        //    //////create a list of products to be use in the testing. Need to change to Cavalier company to get products
        //    ////var index = 0;
        //    ////var responseAllProducts = CatalogModel.GetAllProducts();
        //    ////var jsonListProduct = "[";
        //    ////foreach (var product in responseAllProducts)
        //    ////{
        //    ////    if (index == 10)
        //    ////    {
        //    ////        break;
        //    ////    }
        //    ////    var prod = CatalogModel.GetProductById(Convert.ToInt32(product.Id));
        //    ////    string productJSON = JsonConvert.SerializeObject(prod);
        //    ////    jsonListProduct = jsonListProduct + productJSON + ",";
        //    ////    index++;
        //    ////}
        //    ////int indexLastComma = jsonListProduct.LastIndexOf(',');
        //    ////jsonListProduct = jsonListProduct.Remove(indexLastComma, 1);
        //    ////jsonListProduct = jsonListProduct + "]";
        //    //////**********************************************************

        //    var bulkResponse = new Sheev.Common.Models.BulkResponse();
        //    var productResponse = new Product.Common.Models.ProductResponse();
        //    var mainMessage = string.Empty;
        //    var statusOperation = string.Empty;

        //    int updateCount = 0;
        //    int createCount = 0;
        //    int successCount = 0;
        //    int failCount = 0;

        //    if (request == null || request.Count == 0)
        //    {
        //        bulkResponse.Message = "Request or items list is null or empty. This could be due to an invalid json structure.";
        //        return bulkResponse;
        //    }

        //    var indexItem = 0;
        //    //loop through the products on the list 
        //    foreach (var product in request)
        //    {
        //        var trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, product.TrackingGuid);

        //        if (indexItem > 199)
        //        {
        //            mainMessage = "The list of items is " + request.Count + " (can't be greater than 200). Will create/update up to 200, needs to resubmit the rest " + (request.Count - 200) + " items. ";
        //            break;
        //        }
        //        var item = new Sheev.Common.Models.Item();
        //        try
        //        {
        //            Utilities.RestErrorHandler.CheckProductRequest(product, _context, trackingGuid);
        //            //checking for default value from Swagger
        //            if (product.Id != null && product.Id != 0)
        //            {
        //                updateCount++;
        //                statusOperation = "Updated";
        //                //have to convert product.id in case is null, convert to 0.
        //                productResponse = CatalogModel.UpdateProduct(Convert.ToInt32(product.Id), product, _context, trackingGuid);
        //            }
        //            else
        //            {
        //                createCount++;
        //                statusOperation = "Created";
        //                productResponse = CatalogModel.SaveProduct(product, _context, trackingGuid);
        //            }

        //            successCount++;
        //            item.Status = statusOperation + " " + "Success";
        //        }
        //        catch (HttpResponseException ex)
        //        {
        //            failCount++;

        //            if (product.Id != null && product.Id != 0)
        //                statusOperation = "Updated";
        //            else
        //            {
        //                statusOperation = "Created";
        //            }
        //            item.Status = statusOperation + " " + "Failed";
        //            //item.FailedReason = ex.Response.ReasonPhrase;// ex.Response.ToString();

        //            //IG2000.Data.Utilities.Logging.LogTrackingEvent($"Bulk Gift Card Request failed! Reason: {ex.Response.ReasonPhrase}", $"Status:{statusOperation}", LT319.Common.Utilities.Constants.TrackingStatus.Error, trackingGuid);

        //        }

        //        item.Name = product.Name;
        //        //If fails creating the product, maybe no 'Id' has been created.
        //        if (item.Status.Contains("Fail"))
        //        {
        //            if (statusOperation == "Created")
        //            {
        //                if (product.Id == 0)
        //                {
        //                    item.Id = "0";
        //                }
        //                else
        //                {
        //                    item.Id = "";
        //                }
        //            }
        //            else
        //            {
        //                item.Id = Convert.ToString(product.Id);
        //            }
        //        }
        //        else
        //        {
        //            if (statusOperation == "Created")
        //            {
        //                item.Id = Convert.ToString(productResponse.Id);
        //            }
        //            else
        //            {
        //                item.Id = Convert.ToString(product.Id);
        //            }
        //        }
        //        bulkResponse.Items.Add(item);
        //        indexItem++;
        //    }

        //    //assign the stringbuilder to the message and any other message wants to include.
        //    bulkResponse.Message = mainMessage + "Check the list of " + indexItem + " items to see the success and failed transactions.";
        //    return bulkResponse;
        //}
        #endregion

        #region Put Call(s)
        // PUT api/values/
        /// <summary>
        /// Update existing product
        /// </summary>
        /// <remarks>
        /// Updates an existing product based on the product id provided.
        /// ### Product Categories ###
        /// New Product Categories can be added to a product via this product update. However, if removing a category, you need to use Delete /Catalog/ProductCategory/{id}
        /// </remarks>
        /// <param name="id">product id</param>
        /// <param name="request"></param>
        /// <param name="useMongo"></param>
        /// <returns></returns>
        [HttpPut("Product/{id}")]
        [AuthorizeClaim("Product", K2SO.Auth.Constants.PermissionAccessType.EDIT)]
        [ServiceFilter(typeof(ValidateModelAttribute))]
        public async Task<Product.Common.Models.ProductResponse> Put(string id, [FromBody]Product.Common.Models.ProductRequest request)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            Guid trackingGuid = Guid.NewGuid();

            if (request == null)
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, null);
            else
                trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(null, _context, request.TrackingGuid);


            long longId = Utilities.RestErrorHandler.CheckId(id, _context, trackingGuid);
            Utilities.RestErrorHandler.CheckProductRequest(request, _context, trackingGuid);

            var response = new Product.Common.Models.ProductResponse();

            response = await _productModel.Update(longId, request, trackingGuid);

            return response;
        }

        /// <summary>
        /// Reactivates a product
        /// </summary>
        /// <remarks>
        /// Reactivates a product by its product id
        /// </remarks>
        /// <param name="id">Product Id</param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        [HttpPut("Product/Reactivate/{id}")]
        [AuthorizeClaim("Product", K2SO.Auth.Constants.PermissionAccessType.EDIT)]
        public async Task<NoContentResult> ReactivateProduct(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            long longId = Utilities.RestErrorHandler.CheckId(id, _context, (Guid)trackingGuid);

            await _productModel.DeleteOrReactivate(longId, true, (Guid)trackingGuid);

            return NoContent();
        }
        #endregion

        #region Delete Call(s)
        // DELETE api/values/5
        /// <summary>
        /// Remove a product
        /// </summary>
        /// <remarks>
        /// Removes a product by product id
        /// </remarks>
        /// <param name="id">product id</param>
        /// <param name="trackingGuid">activity tracking guid</param>
        /// <returns></returns>
        [HttpDelete("Product/{id}")]
        [AuthorizeClaim("Product", K2SO.Auth.Constants.PermissionAccessType.DELETE)]
        public async Task<NoContentResult> Delete(string id, [FromQuery] Guid? trackingGuid = null)
        {
            _context.Security = new K2SO.Auth.Security(HttpContext.Request.Headers["Authorization"]);
            trackingGuid = IG2000.Data.Utilities.Logging.CreateLogTrackingHeader(trackingGuid, _context);

            long longId = Utilities.RestErrorHandler.CheckId(id, _context, (Guid)trackingGuid);

            await _productModel.DeleteOrReactivate(longId, false, (Guid)trackingGuid);

            return NoContent();
        }
        #endregion
    }
}
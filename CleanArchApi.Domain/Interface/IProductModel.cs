using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Deathstar.Data.Models;
using MongoDB.Driver;
using Sheev.Common.Models;
using Tagge.Common.Models;

namespace Tagge.Models.Interfaces
{
    public interface IProductModel
    {

        Task<int> DeleteOrReactivate(long id, bool reactivate, Guid trackingGuid);
        List<GetAllResponse> GetAllProducts(string queryString, ref long count, int? pageNumber = 1, int? pageSize = 100);
        Task<ProductResponse> GetProductById(long id, Guid trackingGuid);
        Task RollbackProduct(long id);
        Task<ProductResponse> Save(ProductRequest request, Guid trackingGuid);
        Task<ProductResponse> Update(long id, ProductRequest request, Guid trackingGuid);
        
        //cr - keep it static
        //Task CheckForDuplicateSkus(string sku, string companyId, IMongoCollection<PC_Product> productCollection, Guid trackingGuid);
        //Task<PC_Product> ValidateById(string id, Guid trackingGuid);
        //Task<PC_Product> ValidateBySku(string sku, Guid trackingGuid);
    }
}
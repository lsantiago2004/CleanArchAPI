using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Deathstar.Data.Models;
using MongoDB.Driver;
using Tagge.Common.Models;

namespace Tagge.Models.Interfaces
{
    public interface IVariantModel
    {
        Task CheckForDuplicateSkus(string sku, string companyId, IMongoCollection<PC_ProductVariant> productvariantCollection, Guid trackingGuid);
        Task<int> DeleteOrReactivate(long id, bool reactivate, Guid trackingGuid);
        Task<int> DeleteOrReactivateByProductId(long id, bool reactivate, string timestamp, string orginialTimestamp, Guid trackingGuid);
        Task<List<ProductVariantResponse>> GetAll(long productId, Guid trackingGuid);
        Task<ProductVariantResponse> GetById(long id, Guid trackingGuid);
        Task RollbackVariant(long id);
        Task<ProductVariantResponse> Save(ProductVariantRequest request, PC_Product dbProduct, Guid trackingGuid);
        Task<ProductVariantResponse> SaveOrUpdate(ProductVariantRequest request, PC_Product dbProduct, Guid trackingGuid);
        Task<ProductVariantResponse> Update(ProductVariantRequest request, long id, Guid trackingGuid, bool fromController = false);
        //Task ValidateById(string id,  Guid trackingGuid);
        //Task<PC_ProductVariant> ValidateBySku(string sku, Guid trackingGuid);
    }
}
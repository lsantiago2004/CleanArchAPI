using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Deathstar.Data.Models;
using Sheev.Common.BaseModels;
using Tagge.Common.Models;

namespace Tagge.Models.Interfaces
{
    public interface IAlternateIdModel
    {
        Task<int> DeleteOrReactivate(IBaseContextModel context, string combinedId, string tableName, bool reactivate, Guid trackingGuid);
        Task<ProductAlternateIdResponse> GetById(IBaseContextModel context, string id, string tableName, Guid trackingGuid);
        Task<ProductAlternateIdResponse> Save(IBaseContextModel context, AlternateIdRequest request, Guid trackingGuid);
        Task<List<PC_AlternateId>> SaveOrUpdate(IBaseContextModel context, long internalId, string sku, string tableName, List<ProductAlternateIdRequest> request, List<PC_AlternateId> dbExistingAlternateIds, PC_Product dbProduct, Guid trackingGuid);
        Task<ProductAlternateIdResponse> SaveVariant(IBaseContextModel context, AlternateIdRequest request, Guid trackingGuid);
        Task<ProductAlternateIdResponse> Update(IBaseContextModel context, string id, AlternateIdRequest request, Guid trackingGuid);
        Task<ProductAlternateIdResponse> UpdateVariant(IBaseContextModel context, string id, AlternateIdRequest request, Guid trackingGuid);
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Deathstar.Data.Models;
using Sheev.Common.BaseModels;
using Sheev.Common.Models;
using Tagge.Common.Models;

namespace Tagge.Models.Interfaces
{
    public interface ICategoryAssignmentModel
    {
        Task DeleteOrReactivate(IBaseContextModel context, string combinedId, string tableName, bool reactivate, Guid trackingGuid);
        Task<ProductCategoryResponse> GetById(IBaseContextModel context, string id, string tableName, Guid trackingGuid);
        Task<List<PC_ProductCategory>> Save(IBaseContextModel context, long internalId, List<GenericRequest> request, List<PC_ProductCategory> existingCategories, Guid trackingGuid);
        Task<ProductCategoryResponse> Save(IBaseContextModel context, ProductCategoryRequest request, string tableName, Guid trackingGuid);
    }
}
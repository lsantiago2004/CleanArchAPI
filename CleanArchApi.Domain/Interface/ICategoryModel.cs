using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sheev.Common.BaseModels;
using Sheev.Common.Models;
using Tagge.Common.Models;

namespace Tagge.Models.Interfaces
{
    public interface ICategoryModel
    {
        Task<int> DeleteOrReactivate(IBaseContextModel context, long id, bool reactivate, Guid trackingGuid);
        List<GetAllResponse> GetAll(IBaseContextModel context, string queryString, ref long count, int? pageNumber = 1, int? pageSize = 100);
        Task<CategoryResponse> GetById(IBaseContextModel context, long id, Guid trackingGuid);
        Task<CategoryResponse> Save(IBaseContextModel context, CategoryRequest request, Guid trackingGuid);
        Task<CategoryResponse> Update(IBaseContextModel context, long id, CategoryRequest request, Guid trackingGuid);

        ////Ciscular Reference
        ////Task<string> Validate(long id, Guid trackingGuid);
        ////Task Validate(List<Sheev.Common.Models.GenericRequest> categories, Guid trackingGuid);
    }
}
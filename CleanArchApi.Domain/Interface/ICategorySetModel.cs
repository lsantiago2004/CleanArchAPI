using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sheev.Common.BaseModels;
using Sheev.Common.Models;
using Tagge.Common.Models;

namespace Tagge.Models.Interfaces
{
    public interface ICategorySetModel
    {
        Task<int> DeleteOrReactivate(IBaseContextModel context, long id, bool reactivate, Guid trackingGuid);
        List<GetAllResponse> GetAll(IBaseContextModel context, string queryString, ref long count, int? pageNumber = 1, int? pageSize = 100);
        Task<CategorySetResponse> GetById(IBaseContextModel context, long id, Guid trackingGuid);
        Task<CategorySetResponse> Save(IBaseContextModel context, CategorySetRequest request, Guid trackingGuid);
        Task<CategorySetResponse> Update(IBaseContextModel context, long id, CategorySetRequest request, Guid trackingGuid);
        Task Validate(IBaseContextModel context, List<GenericRequest> categorySets, Guid trackingGuid);
    }
}
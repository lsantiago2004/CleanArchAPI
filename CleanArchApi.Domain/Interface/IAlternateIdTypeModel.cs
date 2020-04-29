using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sheev.Common.BaseModels;
using Sheev.Common.Models;
using Tagge.Common.Models;

namespace Tagge.Models.Interfaces
{
    public interface IAlternateIdTypeModel
    {
        Task<int> DeleteOrReactivate(IBaseContextModel context, long id, bool reactivate, Guid trackingGuid);
        List<GetAllResponse> GetAll(IBaseContextModel context, string queryString, ref long count, int? pageNumber = 1, int? pageSize = 100);
        Task<AlternateIdTypeResponse> GetById(IBaseContextModel context, long id, Guid trackingGuid);
        Task<AlternateIdTypeResponse> Save(IBaseContextModel context, AlternateIdTypeRequest request, Guid trackingGuid);
        Task<AlternateIdTypeResponse> Update(IBaseContextModel context, long id, AlternateIdTypeRequest request, Guid trackingGuid);
        Task Validate(IBaseContextModel context, long alternateIdTypeId, Guid trackingGuid);
    }
}
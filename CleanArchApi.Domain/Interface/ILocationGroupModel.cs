using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Deathstar.Data.Models;
using Sheev.Common.Models;
using Tagge.Common.Models;

namespace Tagge.Models.Interfaces
{
    public interface ILocationGroupModel
    {
        Task<int> DeleteOrReactivate(long id, bool reactivate, Guid trackingGuid);
        List<GetAllResponse> GetAll(string queryString, ref long count, int? pageNumber = 1, int? pageSize = 100);
        Task<LocationGroupResponse> GetById(long id, Guid trackingGuid);
        Task<LocationGroupResponse> Save(LocationGroupRequest request, Guid trackingGuid);
        ////Task<List<TM_GenericEntry>> SaveOrUpdate(List<GenericRequest> request, List<TM_GenericEntry> existingLocations, Guid trackingGuid);
        Task<LocationGroupResponse> Update(long id, LocationGroupRequest request, Guid trackingGuid);
        //Task Validate(List<GenericRequest> locationGroups, Guid trackingGuid);
        //Task<string> Validate(long locationGroupId, Guid trackingGuid);
    }
}
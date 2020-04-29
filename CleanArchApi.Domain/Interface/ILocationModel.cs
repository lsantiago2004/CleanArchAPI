using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Deathstar.Data.Models;
using Sheev.Common.Models;
using Tagge.Common.Models;

namespace Tagge.Models.Interfaces
{
    public interface ILocationModel
    {
        Task<int> DeleteOrReactivate(long id, bool reactivate, Guid trackingGuid);
        List<GetAllResponse> GetAll(string queryString, ref long count, int? pageNumber = 1, int? pageSize = 100);
        Task<LocationResponse> GetById(long id, Guid trackingGuid);
        Task<LocationResponse> Save(LocationRequest request, Guid trackingGuid);
        ////Task<List<TM_GenericEntry>> SaveOrUpdate(List<GenericRequest> request, List<TM_GenericEntry> existingLocations, Guid trackingGuid);
        Task SaveUpdateGeneric(GenericRequest request, Guid trackingGuid);
        Task<LocationResponse> Update(long id, LocationRequest request, Guid trackingGuid);
        ////Task Validate(List<GenericRequest> locations, Guid trackingGuid);
        ////Task<string> Validate(long locationId, Guid trackingGuid);
    }
}
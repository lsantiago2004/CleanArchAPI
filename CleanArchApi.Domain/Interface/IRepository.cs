using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tagge.Models.Interfaces
{
    public interface IRepository<T> where T:class
    {
        Task<int> DeleteOrReactivate(long id, bool reactivate, Guid trackingGuid);
        List<T> GetAll(string queryString, ref long count, int? pageNumber = 1, int? pageSize = 100);
        Task<T> GetById(long id, Guid trackingGuid);
        Task<T> Save(T request, Guid trackingGuid);
        Task<T> Update(long id, T request, Guid trackingGuid);
        Task Validate(List<T> categorySets, Guid trackingGuid);
    }
}

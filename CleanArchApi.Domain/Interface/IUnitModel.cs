using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Deathstar.Data.Models;
using Tagge.Common.Models;

namespace Tagge.Models.Interfaces
{
    public interface IUnitModel
    {
        Task<int> DeleteOrReactivate(long id, bool reactivate, Guid trackingGuid);
        Task<List<PC_ProductUnit>> Save(long productId, List<ProductUnitRequest> units, Guid trackingGuid);
    }
}
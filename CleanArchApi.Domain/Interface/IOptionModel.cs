using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Deathstar.Data.Models;
using Tagge.Common.Models;

namespace Tagge.Models.Interfaces
{
    public interface IOptionModel
    {
        Task<int> DeleteOrReactivate(string id, bool reactivate, Guid trackingGuid);
        Task<List<PC_Option>> Save(long productId, List<OptionRequest> options, Guid trackingGuid);
        Task Validate(long productId, List<OptionRequest> options, ContextModel context, Guid trackingGuid);
    }
}
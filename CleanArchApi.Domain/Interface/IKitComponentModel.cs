using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Deathstar.Data.Models;
using Tagge.Common.Models;

namespace Tagge.Models.Interfaces
{
    public interface IKitComponentModel
    {
        Task<List<PC_KitComponent>> Save(string parentId, string tableName, List<KitComponentRequest> request, Guid trackingGuid);
        Task<List<PC_KitComponent>> SaveOrUpdate(string parentId, string tableName, List<KitComponentRequest> request, List<PC_KitComponent> dbExistingComponents, Guid trackingGuid);
    }
}
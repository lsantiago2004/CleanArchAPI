using System;
using System.Threading.Tasks;
using Deathstar.Data.Models;
using Tagge.Common.Models;

namespace Tagge.Models.Interfaces
{
    public interface IKitModel
    {
        Task<int> DeleteOrReactivate(string id, bool reactivate, Guid trackingGuid);
        Task<KitResponse> GetById(string id, Guid trackingGuid);
        Task<KitResponse> Save(KitRequest request, Guid trackingGuid);
        Task<PC_Kit> Save(long internalId, string tableName, KitRequest request, Guid trackingGuid);
        Task<PC_Kit> SaveOrUpdate(long internalId, string tableName, PC_Kit dbExistingKit, KitRequest request, Guid trackingGuid);
        Task<KitResponse> Update(string combinedId, KitRequest request, Guid trackingGuid);
        Task Validate(string sku, Guid trackingGuid);
    }
}
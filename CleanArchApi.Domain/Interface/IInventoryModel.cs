using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tagge.Common.Models;

namespace Tagge.Models.Interfaces
{
    public interface IInventoryModel
    {
        Task<int> DeleteOrReactivate(long id, bool reactivate, Guid trackingGuid);
        Task<int> DeleteOrReactivate(long id, bool reactivate, string tableName, string timestamp, string orginialTimestamp, Guid trackingGuid);
        Task<List<InventoryResponse>> GetAll(long productId, string tableName, Guid trackingGuid);
        Task<InventoryResponse> GetById(long id, Guid trackingGuid);
        Task<List<InventoryResponse>> GetById(string id, string tableName, Guid trackingGuid);
        Task RollbackInventory(long id, string tableName);
        Task<InventoryResponse> Save(InventoryRequest request, string tableName, string internalId, Guid trackingGuid, bool fromController = false);
        Task<InventoryResponse> SaveOrUpdate(InventoryRequest request, string tableName, string internalId, Guid trackingGuid);
        Task<InventoryResponse> Update(InventoryRequest request, long id, Guid trackingGuid, bool fromController = false);
    }
}
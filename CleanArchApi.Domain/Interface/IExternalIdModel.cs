using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sheev.Common.BaseModels;
using Tagge.Common.Models;

namespace Tagge.Models.Interfaces
{
    public interface IExternalIdModel
    {
        Task<int> DeleteOrReactivate(ExternalIdRequest request, bool reactivate, Guid trackingGuid);
        Task<int> DeleteOrReactivateByParentId(string internalId, bool reactivate, string tableName, string timestamp, string orginialTimestamp, Guid trackingGuid, string scope, bool sendWebhook = false);
        Task<List<ExternalIdResponse>> GetByParentId(string id, string tableName, Guid trackingGuid);
        Task<string> GetExternalId(string internalId, long systemId, string tableName, Guid trackingGuid);
        Task<string> GetInternalId(string externalId, long systemId, string tableName, Guid trackingGuid);
        Task RollbackExternalId(long id, string tableName);
        Task<List<ExternalIdResponse>> SaveOrUpdateGenericExternalId(List<GenericExternalIdRequest> request, string tableName, string internalId, Guid trackingGuid);
        Task<ExternalIdResponse> Update(ExternalIdRequest request, Guid trackingGuid);
        Task ValidateInternalId(string id, string tableName, Guid trackingGuid);
    }
}
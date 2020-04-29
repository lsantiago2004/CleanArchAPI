using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Deathstar.Data.Models;
using Sheev.Common.BaseModels;
using Tagge.Common.Models;

namespace Tagge.Models.Interfaces
{
    public interface ICustomFieldModel
    {
        List<PC_CustomField> DeleteGenericCustomField(string internalId, List<PC_CustomField> existingCustomFields, string tableName, Guid trackingGuid);
        Task DeleteOrReactivate(string id, string userTableName, bool reactivate, Guid trackingGuid);
        Task<GenericCustomFieldResponse> Save(GenericCustomFieldRequest request, Guid trackingGuid);
        Task<List<PC_CustomField>> SaveGenericCustomField(List<GenericCustomFieldRequest> request, string tableName, string internalId, Guid trackingGuid);
        List<PC_CustomField> SaveOrUpdateGenericCustomField(GenericCustomFieldRequest request, List<PC_CustomField> existingCustomFields, string tableName, string internalId, ref PC_CustomField newCustomField, Guid trackingGuid);
        Task<List<PC_CustomField>> SaveOrUpdateGenericCustomFields(List<GenericCustomFieldRequest> request, List<PC_CustomField> existingCustomFields, string tableName, string internalId, Guid trackingGuid);
        Task<GenericCustomFieldResponse> Update(string id, GenericCustomFieldRequest request, Guid trackingGuid);
        void ValidateCustomField(string customFieldName, string tableName);
    }
}
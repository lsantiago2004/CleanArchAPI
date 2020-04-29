using System;
using System.Threading.Tasks;
using Sheev.Common.BaseModels;

namespace Tagge.Models.Interfaces
{
    public interface ITypeModel
    {
        Task ValidateKitComponentType(string requestField, Guid trackingGuid);
        Task ValidateKitType(string requestField, Guid trackingGuid);
        Task ValidateProductType(string requestField, Guid trackingGuid);
        void ValidateRelatedType(string requestField, Guid trackingGuid);
        Task ValidateStatusType(string requestField, Guid trackingGuid);
        Task ValidateTrackingMethodType(string requestField, Guid trackingGuid);
    }
}
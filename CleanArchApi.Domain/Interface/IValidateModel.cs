using System;
using System.Threading.Tasks;

namespace Tagge.Models.Interfaces
{
    public interface IValidateModel
    {
        Task ValidateSkuAndUnit(string sku, string unitName, Guid trackingGuid);
    }
}
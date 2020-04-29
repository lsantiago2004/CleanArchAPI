using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Deathstar.Data.Models;
using Tagge.Common.Models;

namespace Tagge.Models.Interfaces
{
    public interface IOptionValueModel
    {
        Task<int> DeleteOrReactivateVariantOptions(long id, string optionName, bool reactivate, Guid trackingGuid);
        Task<List<PC_OptionValue>> Save(long productId, PC_Option dbOption, List<OptionValueRequest> optionvalues, Guid trackingGuid);
        Task<List<PC_OptionValue>> SaveOrUpdateVariantOptions(long variantId, PC_Product dbProduct, List<OptionValueRequest> request, List<PC_OptionValue> dbExistingVariantOptions, Guid trackingGuid);
    }
}
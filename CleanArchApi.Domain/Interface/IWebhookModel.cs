using System;
using System.Threading.Tasks;
using Sheev.Common.BaseModels;
using Sheev.Common.Models;

namespace Tagge.Models.Interfaces
{
    public interface IWebhookModel
    {
        Task FireWebhookEvent(WebhookResponse request, IBaseContextModel context, Guid trackingGuid, int attempt = 0);
    }
}
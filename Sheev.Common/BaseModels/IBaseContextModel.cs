using K2SO.Auth;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Sheev.Common.Logger;
using Sheev.Common.Models;
using System.Collections.Generic;

namespace Sheev.Common.BaseModels
{
    public interface IBaseContextModel
    {
        IMongoDatabase Database { get; set; }
        IOptions<ApiUrlSetting> ApiUrlSettings { get; set; }
        IOptions<GenericSetting> GenericSettings { get; set; }
        IOptions<MongoDbSetting> MongoDbSettings { get; set; }
        ILoggerManager NLogger { get; set; }
        Security Security { get; set; }
        List<Sheev.Common.Models.LookupResponse> Systems { get; set; }

    }
}
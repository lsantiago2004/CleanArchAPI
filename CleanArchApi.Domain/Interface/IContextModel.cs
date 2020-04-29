using System.Collections.Generic;
using MongoDB.Driver;
using Sheev.Common.Models;

namespace Tagge.Models.Interfaces
{
    public interface IContextModel
    {
        IMongoDatabase Database { get; set; }
        List<LookupResponse> Systems { get; set; }
    }
}
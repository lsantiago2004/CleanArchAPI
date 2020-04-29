using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Sheev.Common.Logger;
using Sheev.Common.Models;
//using Deathstar.Data.Models;

namespace Sheev.Common.BaseModels
{
    /// <summary>
    /// The Base Context Model - Contains the properties used by all the Apis
    /// </summary>
    public class BaseContextModel : IBaseContextModel
    {
        // Naming Voliation -> Remember all properties should be PascalCased not camelCased
        public ILoggerManager NLogger { get; set; }
        public K2SO.Auth.Security Security { get; set; }
        public IOptions<Sheev.Common.Models.ApiUrlSetting> ApiUrlSettings { get; set; }
        public IOptions<Sheev.Common.Models.MongoDbSetting> MongoDbSettings { get; set; }
        public IOptions<Sheev.Common.Models.GenericSetting> GenericSettings { get; set; }
        public IMongoDatabase Database { get; set; }
        public List<LookupResponse> Systems { get; set; }

        //Collections
        //public IMongoCollection<Deathstar.Data.Models.PC_AlternateIdType> AlternateIdTypeCollection = context.Database.GetCollection<Deathstar.Data.Models.PC_AlternateIdType>("PC_AlternateIdType");
        /// <summary>
        /// This is needed to be able to log the correct application
        /// </summary>
        //public Sheev.Common.Utilities.Constants.ST_Application ApplicationId { get; set; }

        public BaseContextModel()
        {
            // Default constructor
        }
    }
}

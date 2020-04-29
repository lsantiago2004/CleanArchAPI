
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Sheev.Common.BaseModels;
using Sheev.Common.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tagge.Models
{
    /// <summary>
    /// In Soviet Russia Context Model has you!
    /// </summary>
    public class ContextModel : BaseContextModel
    {
        #region Properties
        /// <summary>
        /// Security Property
        /// </summary>
        //public K2SO.Auth.Security Security { get; set; }

        /// <summary>
        /// Mongo Database setting property
        /// </summary>
        //public IOptions<Sheev.Common.Models.MongoDbSetting> MongoDbSettings { get; set; }

        ///// <summary>
        ///// Api Url Settings 
        ///// </summary>
        //public IOptions<Sheev.Common.Models.ApiUrlSetting> ApiUrlSettings { get; set; }

        /// <summary>
        /// Mongo Database
        /// </summary>
        //public IMongoDatabase Database { get; set; }

        /// <summary>
        /// Systems for the current user
        /// </summary>
        public List<Sheev.Common.Models.LookupResponse> Systems { get; set; }
        #endregion

        // Mongo Database
        private IMongoDatabase GetDB()
        {
            var database = MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            string connectionString = MongoDbSettings.Value.ConnectionString;
            var client = new MongoClient(connectionString);
            Database = client.GetDatabase(database.Name);
            return Database;
        }

        //Collections
        public IMongoCollection<Deathstar.Data.Models.PC_AlternateIdType> AlternateIdTypeCollection;
        public IMongoCollection<Deathstar.Data.Models.PC_AlternateId> AlternateIdCollection;
        public IMongoCollection<Deathstar.Data.Models.PC_Product> ProductCollection;
        public IMongoCollection<Deathstar.Data.Models.PC_ProductVariant> ProductVariantCollection;


        #region Construcutor(s)
        /// <summary>
        /// Default Context Model Constructor 
        /// </summary>
        public ContextModel() { }

        /// <summary>
        /// Context Model Contructor (MongoDbSettings)
        /// </summary>
        /// <param name="mongoDbSettings"></param>
        public ContextModel(IOptions<Sheev.Common.Models.MongoDbSetting> mongoDbSettings)
        {
            MongoDbSettings = mongoDbSettings;
        }

        /// <summary>
        /// Context Model Constructor (MongoDbSettings, ApiUrlSetttings) 
        /// </summary>
        /// <param name="mongoDbSettings"></param>
        /// <param name="apiUrlSettings"></param>
        public ContextModel(IOptions<Sheev.Common.Models.MongoDbSetting> mongoDbSettings, IOptions<Sheev.Common.Models.ApiUrlSetting> apiUrlSettings)
        {
            // Mongo db settings found in app config
            MongoDbSettings = mongoDbSettings;

            // Api url settings found in app config
            ApiUrlSettings = apiUrlSettings;

            // Logging now using nLog
            //NLogger = logger;

            //Collections
            var db = GetDB();
            AlternateIdTypeCollection = db.GetCollection<Deathstar.Data.Models.PC_AlternateIdType>("PC_AlternateIdType");
            AlternateIdCollection = db.GetCollection<Deathstar.Data.Models.PC_AlternateId>("PC_AlternateId");
            ProductCollection = db.GetCollection<Deathstar.Data.Models.PC_Product>("PC_Product");
            ProductVariantCollection = db.GetCollection<Deathstar.Data.Models.PC_ProductVariant>("PC_AlternateId");





        }

        /// <summary>
        /// Context Model Constructor (MongoDbSettings, ApiUrlSetttings) 
        /// </summary>
        /// <param name="mongoDbSettings"></param>
        /// <param name="apiUrlSettings"></param>
        public ContextModel(IOptions<Sheev.Common.Models.MongoDbSetting> mongoDbSettings, IOptions<Sheev.Common.Models.ApiUrlSetting> apiUrlSettings, ILoggerManager logger)
        {
            // Mongo db settings found in app config
            MongoDbSettings = mongoDbSettings;

            // Api url settings found in app config
            ApiUrlSettings = apiUrlSettings;

            // Logging now using nLog
            NLogger = logger;

            //// Mongo Database
            //var database = MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "DeathStar");
            //string connectionString = MongoDbSettings.Value.ConnectionString;
            //var client = new MongoClient(connectionString);
            //Database = client.GetDatabase(database.Name);

            //Collections
            var db = GetDB();
            AlternateIdTypeCollection = db.GetCollection<Deathstar.Data.Models.PC_AlternateIdType>("PC_AlternateIdType");
            AlternateIdCollection = db.GetCollection<Deathstar.Data.Models.PC_AlternateId>("PC_AlternateId");
            ProductCollection = db.GetCollection<Deathstar.Data.Models.PC_Product>("PC_Product");
            ProductVariantCollection = db.GetCollection<Deathstar.Data.Models.PC_ProductVariant>("PC_AlternateId");


        }
        #endregion
    }
}

using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deathstar.Data.Models
{
    public class PC_ExternalId
    {
        #region Properties
        public long Id { get; set; }
        public string ST_TableName { get; set; }
        public long SystemId { get; set; }
        //public string SystemName { get; set; }
        public string ExternalId { get; set; }
        public string InternalId { get; set; }
        public string DV_CompanyId { get; set; }
        public string CreatedDateTime { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedDateTime { get; set; }
        public string UpdatedBy { get; set; }
        public bool IsActive { get; set; }
        #endregion

        #region Constructor(s)
        #endregion

        #region Method(s)
        public static List<Tagge.Common.Models.ExternalIdResponse> ConvertToResponse(string id, string tableName, string companyId, IMongoDatabase db)
        {
            // MongoDB Settings
            string collectionName = "PC_ExternalId";

            // Get MongoDB
            var externalIdCollection = db.GetCollection<Deathstar.Data.Models.PC_ExternalId>(collectionName);

            // Filter
            var filters = Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.InternalId, id);
            filters = filters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.ST_TableName, tableName);
            filters = filters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.IsActive, true);
            filters = filters & Builders<Deathstar.Data.Models.PC_ExternalId>.Filter.Eq(x => x.DV_CompanyId, companyId.ToString());

            var dbExternalIds = externalIdCollection.Find(filters).ToList();

            // Build the Response
            var response = new List<Tagge.Common.Models.ExternalIdResponse>();

            foreach (var dbExternalId in dbExternalIds)
            {
                var singleResponse = dbExternalId.ConvertToFullResponse();

                response.Add(singleResponse);
            }

            return response;
        }

        public Tagge.Common.Models.ExternalIdResponse ConvertToFullResponse()
        {
            var response = new Tagge.Common.Models.ExternalIdResponse()
            {
                Id = Id,
                SystemId = SystemId,
                //SystemName = SystemName,
                ExternalId = ExternalId,
                InternalId = InternalId
            };

            return response;
        }

        public void ConvertToDatabaseObject(string companyId, string tableName, Tagge.Common.Models.GenericExternalIdRequest request)
        {
            // Properties
            DV_CompanyId = companyId;
            ST_TableName = tableName;
            SystemId = request.SystemId;
            ExternalId = request.ExternalId;
        }

        public void ConvertToFullDatabaseObject(string companyId, string tableName, Tagge.Common.Models.ExternalIdRequest request)
        {
            // Properties
            DV_CompanyId = companyId;
            ST_TableName = tableName;
            SystemId = request.SystemId;
            ExternalId = request.ExternalId;
            InternalId = request.InternalId;
        }
        #endregion
    }
}

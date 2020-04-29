using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deathstar.Data.Models
{
    public class PC_Kit
    {
        #region Properties
        public string PC_Kit_Id { get; set; }
        public string Sku { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string CreatedDateTime { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedDateTime { get; set; }
        public string UpdatedBy { get; set; }
        public bool IsActive { get; set; }
        public List<PC_KitComponent> Components { get; set; }
        public List<PC_CustomField> CustomFields { get; set; }
        //public List<PC_ExternalId> ExternalIds { get; set; }
        #endregion

        #region Constructor(s)
        public PC_Kit()
        {
            Components = new List<PC_KitComponent>();
            CustomFields = new List<PC_CustomField>();
            //ExternalIds = new List<PC_ExternalId>();
        }
        #endregion

        #region Method(s)
        /// <summary>
        /// Convert to Response used for kit only
        /// </summary>
        /// <returns></returns>
        public Tagge.Common.Models.KitResponse ConvertToKitResponse()
        {
            var response = new Tagge.Common.Models.KitResponse();

            // Properties
            response.Id = PC_Kit_Id;
            response.Sku = Sku;
            response.Description = Description;
            response.Type = Type;

            // Components
            if (Components != null)
            {
                response.Components = new List<Tagge.Common.Models.KitComponentResponse>();
                foreach (var component in Components)
                {
                    response.Components.Add(component.ConvertToResponse());
                }
            }

            // Custom Fields
            if (CustomFields != null)
            {
                response.CustomFields = new List<Tagge.Common.Models.GenericCustomFieldResponse>();
                foreach (var customField in CustomFields)
                {
                    response.CustomFields.Add(customField.ConvertToResponse());
                }
            }

            // ExternalIds - Managed in PC_ExternalId

            return response;
        }

        /// <summary>
        /// Convert To Response when part of a bigger item
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public Tagge.Common.Models.KitResponse ConvertToResponse(string companyId, string tableName, IMongoDatabase db)
        {
            var response = new Tagge.Common.Models.KitResponse();

            // Properties
            response.Id = PC_Kit_Id;
            response.Sku = Sku;
            response.Description = Description;
            response.Type = Type;

            // Components
            if(Components != null)
            {
                response.Components = new List<Tagge.Common.Models.KitComponentResponse>();
                foreach(var component in Components)
                {
                    response.Components.Add(component.ConvertToResponse(companyId, tableName, db));
                }
            }

            // Custom Fields
            if (CustomFields != null)
            {
                response.CustomFields = new List<Tagge.Common.Models.GenericCustomFieldResponse>();
                foreach (var customField in CustomFields)
                {
                    response.CustomFields.Add(customField.ConvertToResponse());
                }
            }

            // ExternalIds 
            response.ExternalIds = PC_ExternalId.ConvertToResponse(PC_Kit_Id, tableName, companyId, db);

            return response;
        }

        public void ConvertToDatabaseObject(Tagge.Common.Models.KitRequest request)
        {
            // Properties
            Sku = request.Sku;
            Description = request.Description;
            Type = request.Type;

            // Components

            // Custom Fields - Managed in PC_CustomField

            // ExternalIds - Managed in PC_ExternalId
        }

        /// <summary>
        /// Sets the Primary Key
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="sku"></param>
        public void SetPrimaryKey(string parentId, string sku)
        {
            PC_Kit_Id = $"{parentId}|{sku}";
        }
        #endregion
    }
}
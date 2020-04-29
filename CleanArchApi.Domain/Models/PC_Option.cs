using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deathstar.Data.Models
{
    public class PC_Option
    {
        #region Properties
        public string PC_Option_Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int Order { get; set; }
        public string CreatedDateTime { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedDateTime { get; set; }
        public string UpdatedBy { get; set; }
        public bool IsActive { get; set; }
        public List<PC_OptionValue> Values { get; set; }
        public List<PC_CustomField> CustomFields { get; set; }
        #endregion

        #region Constructor(s)
        public PC_Option()
        {
            Values = new List<PC_OptionValue>();
            CustomFields = new List<PC_CustomField>();
        }
        #endregion

        #region Method(s)
        public Tagge.Common.Models.OptionResponse ConvertToOptionResponse()
        {
            var response = new Tagge.Common.Models.OptionResponse();

            // Properties
            response.Id = PC_Option_Id;
            response.OptionName = Name;
            response.Type = Type;
            response.Order = Order;

            // Option Values
            if(Values != null)
            {
                response.Values = new List<Tagge.Common.Models.OptionValueResponse>();
                foreach(var value in Values)
                {
                    response.Values.Add(value.ConvertToOptionValueResponse());
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

        public Tagge.Common.Models.OptionResponse ConvertToResponse(string companyId, string tableName, IMongoDatabase db)
        {
            var response = new Tagge.Common.Models.OptionResponse();

            // Properties
            response.Id = PC_Option_Id;
            response.OptionName = Name;
            response.Type = Type;
            response.Order = Order;

            // Option Values
            if (Values != null)
            {
                response.Values = new List<Tagge.Common.Models.OptionValueResponse>();
                foreach (var value in Values)
                {
                    response.Values.Add(value.ConvertToResponse(companyId, "PC_ProductOptionValue", db));
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
            response.ExternalIds = PC_ExternalId.ConvertToResponse(PC_Option_Id, tableName, companyId, db);

            return response;
        }

        public void ConvertToDatabaseObject(Tagge.Common.Models.OptionRequest request)
        {
            // Properties
            Name = request.OptionName;
            Type = request.Type;
            Order = request.Order;

            // Option Value - Managed in PC_ProductOptionValue

            // Custom Fields - Managed in PC_CustomField

            // ExternalIds - Managed in PC_ExternalId
        }

        /// <summary>
        /// Sets the primary key for this object
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="optionName"></param>
        public void SetPrimaryKey(string parentId, string optionName)
        {
            PC_Option_Id = $"{parentId}|{optionName}";
        }
        #endregion
    }
}
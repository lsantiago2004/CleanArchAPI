using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deathstar.Data.Models
{
    public class PC_OptionValue
    {
        #region Properties
        public string PC_OptionValue_Id { get; set; }
        public string OptionName { get; set; }
        public string Value { get; set; }
        public string Detail { get; set; }
        public int Order { get; set; }
        public string CreatedDateTime { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedDateTime { get; set; }
        public string UpdatedBy { get; set; }
        public bool IsActive { get; set; }
        public List<PC_CustomField> CustomFields { get; set; }
        //public List<PC_ExternalId> ExternalIds { get; set; }
        #endregion

        #region Constructor(s)
        public PC_OptionValue()
        {
            CustomFields = new List<PC_CustomField>();
            //ExternalIds = new List<PC_ExternalId>();
        }
        #endregion

        #region Method(s)
        /// <summary>
        /// Not Currently Used
        /// </summary>
        /// <returns></returns>
        public Tagge.Common.Models.OptionValueResponse ConvertToOptionValueResponse()
        {
            var response = new Tagge.Common.Models.OptionValueResponse();

            // Properties
            response.Id = PC_OptionValue_Id;
            response.OptionName = OptionName;
            response.Value = Value;
            response.Detail = Detail;
            response.Order = Order;

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
        /// Option Values on a Response
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="tableName"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public Tagge.Common.Models.OptionValueResponse ConvertToResponse(string companyId, string tableName, IMongoDatabase db)
        {
            var response = new Tagge.Common.Models.OptionValueResponse();

            // Set the basic id
            string optionId = PC_OptionValue_Id;

            // Get the option id
            if (tableName == "PC_ProductOptionValue")
            {
                string index = $"|{Value}";
                int indexOfSteam = optionId.IndexOf(index);
                if (indexOfSteam >= 0)
                    optionId = optionId.Remove(indexOfSteam);
            }
            else
            {
                string[] ids = optionId.Split('|');

                optionId = ids[0] + "|" + ids[2];
            }

            // Properties
            response.Id = PC_OptionValue_Id;
            response.OptionId = optionId;
            response.OptionName = OptionName;
            response.Value = Value;
            response.Detail = Detail;
            response.Order = Order;

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
            response.ExternalIds = PC_ExternalId.ConvertToResponse(PC_OptionValue_Id, tableName, companyId, db);

            return response;
        }

        public void ConvertToDatabaseObject(Tagge.Common.Models.OptionValueRequest request)
        {
            // Properties
            OptionName = request.OptionName;
            Value = request.Value;
            Detail = request.Detail;
            Order = request.Order;

            // Custom Fields - Managed in PC_CustomField

            // ExternalIds - Managed in PC_ExternalId
        }

        /// <summary>
        /// Sets the primary key for this object
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="variantId">is nullible</param>
        /// <param name="optionName"></param>
        /// <param name="optionValue"></param>
        public void SetPrimaryKey(string productId, string variantId, string optionName, string optionValue)
        {
            if(string.IsNullOrEmpty(variantId))
                PC_OptionValue_Id = $"{productId}|{optionName}|{optionValue}";
            else
                PC_OptionValue_Id = $"{productId}|{variantId}|{optionName}|{optionValue}";
        }
        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace Deathstar.Data.Models
{
    public class PC_CustomField
    {
        #region Properties
        public string PC_CustomField_Id { get; set; }
        public string CustomFieldName { get; set; }
        public string Value { get; set; }
        #endregion

        #region Constructor(s)
        #endregion

        #region Method(s)
        public Tagge.Common.Models.GenericCustomFieldResponse ConvertToResponse()
        {
            var response = new Tagge.Common.Models.GenericCustomFieldResponse()
            {
                Id = PC_CustomField_Id,
                CustomFieldName = CustomFieldName,
                Value = Value
            };

            return response;
        }

        public void ConvertToDatabaseObject(Tagge.Common.Models.GenericCustomFieldRequest request)
        {
            CustomFieldName = request.CustomFieldName;
            Value = request.Value;
        }

        public void SetPrimaryKey(string parentId, string name)
        {
            PC_CustomField_Id = $"{parentId}|{name}";
        }
        #endregion
    }
}

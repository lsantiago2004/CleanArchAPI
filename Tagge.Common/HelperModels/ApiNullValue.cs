using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tagge.Common.HelperModels
{
    //This BigCommerce API does not generally deal in nulls. It treats null values as if they weren't supplied, and will not update an existing item with those values.
    //  To wipe a value out in BC, you need to pass an empty string or a 0. Since this convention is not guaranteed and universal, we need to provide a method for
    //  identifying which fields this will apply to. This attribte allows us to decorate a given property with it's ApiNullValue. The C3PO function 
    //  TranslationUtilities.SetValue will honor this setting and replace Nulls with the decorator value.
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class ApiNullValue : System.Attribute
    {
        private object _nullValue;
        public object NullValue { get { return _nullValue; } }
        public ApiNullValue(object NullValue)
        {
            this._nullValue = NullValue;
        }
    }
}

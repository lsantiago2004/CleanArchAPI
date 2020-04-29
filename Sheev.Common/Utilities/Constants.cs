using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Sheev.Common.Utilities
{
    public class Constants
    {
        public enum ST_Application
        {
            NONE,
            SPACEPORT,
            DOCKINGBAY,
            C3P0,
            SHYHOOK,
            R2D2,
            BB8,
            JARJARBINKS,
            LANDOCALRISSIAN,
            MAZ,
            K2SO,
            TARKIN,
            TENANT,
            MOTTI,
            TAGGE,
            KRENNIC
        }

        public enum SP_ChangelogType
        {
            NONE,
            ADD,
            CHANGED,
            DEPRECATED,
            REMOVED,
            FIXED,
            SECURITY
        }

        public enum EH_ErrorStatus
        {
            NONE,
            ACTIVE,
            DISMISSED,
            RETRY_PENDING,
            READY_FOR_DELETE, //Success
            MANUAL_FIX_REQUIRED
        }

        public enum EX_EmailStatus
        {
            NONE,
            READY_TO_SEND,
            RETRY_PENDING,
            READY_FOR_DELETE, // Success
            FAILED
        }

        public enum EX_EmailType
        {
            NONE,
            SYSTEM,
            ERROR,
            NOTIFICATION,
            USER
        }

        public enum GM_ActivityType
        {
            NONE,
            ISSUE,
            REDEEM,
            VOID,
            REINSTATE,
            ADJUST
        }

        public enum GM_Status
        {
            NONE,
            ACTIVE,
            EXPIRED,
            VOID,
            REDEEMED
        }

        public enum GM_Type
        {
            NONE,
            GIFTCARD,
            STORE_CREDIT
        }

        public enum TM_LookuptranslationType
        {
            NONE,
            USER_ENTERED,
            STATIC,
            RETRIEVED_LIST,
            RETRIEVED_LIST_BUT_EDITABLE
        }

        public enum TM_MappingCollectionType
        {
            NONE = 0,
            PRODUCT = 1,
            PRODUCT_UNIT = 2,
            PRODUCT_INVENTORY = 3,
            PRODUCT_VARIANT = 4,
            PRODUCT_VARIANT_INVENTORY = 5,
            PRODUCT_CATEGORY = 6,
            LOCATION = 7,
            CUSTOMER = 8,
            TRANSACTION = 9,
            TRANSACTION_LINE = 10,
            TRANSACTION_ADDRESS = 11,
            TRANSACTION_TAX = 12,
            TRANSACTION_PAYMENT = 13,
            TRANSACTION_TRACKING_NUMBER = 14,
            TRANSACTION_NOTE = 15,
            CUSTOMER_CATEGORY = 16,
            CUSTOMER_ADDRESS = 17,
            CUSTOMER_CONTACT = 18,
            SHIPMENT = 19,
            SHIPMENT_LINE = 20,
            SHIPPING_METHOD = 21,
            PAYMENT_METHOD = 22,
            PRODUCT_OPTION = 23,
            PRODUCT_OPTION_VALUE = 24,
            PRODUCT_VARIANT_OPTION = 25,
            PRODUCT_VARIANT_OPTION_VALUE = 26,
            GIFT_CARD = 27,
            GIFT_CARD_ACTIVITY = 28,
            TRANSACTION_DISCOUNT = 29,
            CATALOG_CATEGORY_SET = 30,
            KIT = 31,
            KIT_COMPONENT = 32,
            ALTERNATE_ID_TYPE = 33,
            PRODUCT_ALTERNATE_ID = 34,
            PRODUCT_VARIANT_ALTERNATE_ID = 35,
            PRODUCT_CATEGORY_ASSIGNMENT = 36,
            PRODUCT_VARIANT_CATEGORY_ASSIGNMENT = 37,
            LOCATION_GROUP
        }

        public enum TM_MappingType
        {
            NONE,
            FIELD,
            FIELD_WITH_DEFAULT,
            STATIC,
            LOOKUP_TRANSLATION,
            DYNAMIC_FORMULA,
            REGEX_TRANSLATION
        }

        public enum OM_PaymentType
        {
            A_R = 9,
            CASH = 10,
            CHECK = 11,
            CREDIT_CARD = 12,
            DEBIT_CARD = 13,
            GIFT_CARD = 14,
            LOYALTY = 15,
            STORE_CREDIT = 16,
            EBT = 17,
            ONLINE = 18,
        }

        public enum OM_NoteType
        {
            NONE,
            GIFT,
            CUSTOMER_NOTE,
            STATUS,
            ORDER,
            PRIVATE
        }

        public enum OM_Status
        {
            NONE,
            PENDING,
            COMPLETE,
            CANCELLED,
            SHIPPED,
            BACKORDER
        }

        public enum OM_TransactionLineType
        {
            NONE,
            PRODUCT,
            GIFTCARD
        }

        public enum OM_TransactionType
        {
            NONE,
            ORDER,
            SHIPMENT,
            INVOICE,
            HOLD,
            QUOTE,
            TICKET
        }

        public enum ST_SystemType
        {
            IPAAS = 1,
            COUNTERPOINT = 2,
            BIGCOMMERCE = 3,
            MAGENTO2 = 4,
            SHOPIFY = 5,
            ADVANCED_STORE = 6,
            DYNAMIC_INTEGRATION = 7
        }

        public enum TM_MappingDirection
        {
            TO_IPAAS = 1,
            FROM_IPAAS = 2,
            BIDIRECTIONAL = 3
        }

        public enum TM_SyncType
        {
            UNKNOWN = 0,
            ADD = 1,
            UPDATE = 2,
            ADD_AND_UPDATE = 3,
            DELETE = 4,
            CASCADING_DELETE = 5 //The DELETE scope type is not used in the DB, but is used in incoming requests from BC, CP, etc.
        }

        public enum SP_CompanyStatus
        {
            NONE,
            TESTING,
            ACTIVE,
            LOCKED,
            PAUSED
        }

        public enum SP_UserStatus
        {
            NONE,
            ACTIVE,
            LOCKED,
            REGISTERED
        }

        [Flags]
        public enum TS_RunType
        {
            NONE = 0,
            PRODUCT = 1,
            CUSTOMER = 2,
            TRANSACTION = 4,
            GIFT_CARD = 8,
            ALL = PRODUCT | CUSTOMER | TRANSACTION | GIFT_CARD
        }

        public enum WH_Scope
        {
            //This should only be used to indicate that the scope was not found. This might happen if a scope was added to the DB, but not added here.
            UNKNOWN,

            [Description("customer/created")]
            CUSTOMER_CREATED,
            [Description("customer/updated")]
            CUSTOMER_UPDATED,
            [Description("customer/deleted")]
            CUSTOMER_DELETED,

            [Description("product/created")]
            PRODUCT_CREATED,
            [Description("product/updated")]
            PRODUCT_UPDATED,
            [Description("product/deleted")]
            PRODUCT_DELETED,

            [Description("product/inventory/created")]
            PRODUCT_INVENTORY_CREATED,
            [Description("product/inventory/updated")]
            PRODUCT_INVENTORY_UPDATED,
            [Description("product/inventory/deleted")]
            PRODUCT_INVENTORY_DELETED,

            [Description("product/variant/created")]
            PRODUCT_VARIANT_CREATED,
            [Description("product/variant/updated")]
            PRODUCT_VARIANT_UPDATED,
            [Description("product/variant/deleted")]
            PRODUCT_VARIANT_DELETED,

            [Description("product/variant/inventory/created")]
            PRODUCT_VARIANT_INVENTORY_CREATED,
            [Description("product/variant/inventory/updated")]
            PRODUCT_VARIANT_INVENTORY_UPDATED,
            [Description("product/variant/inventory/deleted")]
            PRODUCT_VARIANT_INVENTORY_DELETED,

            [Description("transaction/created")]
            TRANSACTION_CREATED,
            [Description("transaction/updated")]
            TRANSACTION_UPDATED,
            [Description("transaction/deleted")]
            TRANSACTION_DELETED,

            [Description("system/created")]
            SYSTEM_CREATED,
            [Description("system/updated")]
            SYSTEM_UPDATED,
            [Description("system/deleted")]
            SYSTEM_DELETED,

            [Description("giftcard/created")]
            GIFTCARD_CREATED,
            [Description("giftcard/updated")]
            GIFTCARD_UPDATED,
            [Description("giftcard/deleted")]
            GIFTCARD_DELETED,

            [Description("customer/address/created")]
            CUSTOMER_ADDRESS_CREATED,
            [Description("customer/address/updated")]
            CUSTOMER_ADDRESS_UPDATED,
            [Description("customer/address/deleted")]
            CUSTOMER_ADDRESS_DELETED,

            [Description("product/location/created")]
            PRODUCT_LOCATION_CREATED,
            [Description("product/location/updated")]
            PRODUCT_LOCATION_UPDATED,
            [Description("product/location/deleted")]
            PRODUCT_LOCATION_DELETED,

            [Description("customer/category/created")]
            CUSTOMER_CATEGORY_CREATED,
            [Description("customer/category/updated")]
            CUSTOMER_CATEGORY_UPDATED,
            [Description("customer/category/deleted")]
            CUSTOMER_CATEGORY_DELETED,

            [Description("transaction/payment_method/created")]
            TRANSACTION_PAYMENT_METHOD_CREATED,
            [Description("transaction/payment_method/updated")]
            TRANSACTION_PAYMENT_METHOD_UPDATED,
            [Description("transaction/payment_method/deleted")]
            TRANSACTION_PAYMENT_METHOD_DELETED,

            [Description("product/category/created")]
            PRODUCT_CATEGORY_CREATED,
            [Description("product/category/updated")]
            PRODUCT_CATEGORY_UPDATED,
            [Description("product/category/deleted")]
            PRODUCT_CATEGORY_DELETED,

            [Description("transaction/shipping_method/created")]
            TRANSACTION_SHIPPING_METHOD_CREATED,
            [Description("transaction/shipping_method/updated")]
            TRANSACTION_SHIPPING_METHOD_UPDATED,
            [Description("transaction/shipping_method/deleted")]
            TRANSACTION_SHIPPING_METHOD_DELETED,

            [Description("product/variant/category/created")]
            PRODUCT_VARIANT_CATEGORY_CREATED,
            [Description("product/variant/category/updated")]
            PRODUCT_VARIANT_CATEGORY_UPDATED,
            [Description("product/variant/category/deleted")]
            PRODUCT_VARIANT_CATEGORY_DELETED,

            [Description("product/categoryset/created")]
            PRODUCT_CATEGORYSET_CREATED,
            [Description("product/categorsety/updated")]
            PRODUCT_CATEGORYSET_UPDATED,
            [Description("product/categoryset/deleted")]
            PRODUCT_CATEGORYSET_DELETED,

            [Description("product/variant/categoryset/created")]
            PRODUCT_VARIANT_CATEGORYSET_CREATED,
            [Description("product/variant/categoryset/updated")]
            PRODUCT_VARIANT_CATEGORYSET_UPDATED,
            [Description("product/variant/categoryset/deleted")]
            PRODUCT_VARIANT_CATEGORYSET_DELETED,

            [Description("system/product/category/initialize")]
            SYSTEM_PRODUCT_CATEGORY_INITIALIZE,
            [Description("system/payment/method/initialize")]
            SYSTEM_PAYMENT_METHOD_INITIALIZE,
            [Description("system/location/initialize")]
            SYSTEM_LOCATION_INITIALIZE,
            [Description("system/shipping/method/initialize")]
            SYSTEM_SHIPPING_METHOD_INITIALIZE,
            [Description("system/customer/category/initialize")]
            SYSTEM_CUSTOMER_CATEGORY_INITIALIZE,
        }

        public enum DV_DesignationType
        {
            Customer = 1,
            MISP = 2,
            TechPartner = 4
        }

        public enum MM_RequestType
        {
            NewRequest = 1,
            ChangeRequest = 2
        }
        public enum MM_RequestStatusType
        {
            RequestPending = 1,
            IpaasAdminApproved = 2,
            IpaasAdminRejected = 3,
            MispApproved = 4,
            MispRejected = 5
        }

        public enum MM_RequestEmailTemplateType
        {
            MISPRequestSender,
            MISPRequestAdmin,
            MISPRequestAdminApproval,
            MISPRequestAdminReject
        }
    }
}

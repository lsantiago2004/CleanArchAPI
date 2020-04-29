using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;

namespace Sheev.Common.Utilities
{
    public class TypeHelper
    {
        /// <summary>
        /// Converts pretty name to db table type
        /// </summary>
        /// <param name="userType"></param>
        /// <returns></returns>
        public static string ConvertToDbType(string userType)
        {
            string type = string.Empty;

            switch (userType.ToLower())
            {
                case "pc_product":
                case "pc product":
                case "product":
                    type = "PC_Product";
                    break;
                case "variant":
                case "product variant":
                case "product_variant":
                case "pc product variant":
                case "pc_product_variant":
                case "pc_productvariant":
                    type = "PC_ProductVariant";
                    break;
                case "product unit":
                case "product_unit":
                case "pc product unit":
                case "pc_product_unit":
                case "pc_productunit":
                    type = "PC_ProductUnit";
                    break;
                case "inventory":
                case "product inventory":
                case "product_inventory":
                case "pc product inventory":
                case "pc_product_inventory":
                case "pc_productinventory":
                    type = "PC_ProductInventory";
                    break;
                case "variant inventory":
                case "product variant inventory":
                case "product_variant inventory":
                case "pc product variant inventory":
                case "pc_product_variant_inventory":
                case "pc_productvariant_inventory":
                case "pc_productvariantinventory":
                    type = "PC_ProductVariantInventory";
                    break;
                case "catalog category":
                case "catalog_category":
                case "pc catalog category":
                case "pc_catalog_category":
                case "pc_catalogcategory":
                case "product category":
                case "product_category":
                case "category":
                    type = "PC_Category";
                    break;
                case "catalog category set":
                case "category set":
                case "category_set":
                case "pc category set":
                case "pc categoryset":
                case "pc__category_set":
                case "pc_category_set":
                case "pc_categoryset":
                case "pc_catalogcategoryset":
                case "product category set":
                case "product_category_set":
                    type = "PC_CategorySet";
                    break;
                case "location":
                case "pc location":
                case "pc_location":
                    type = "PC_Location";
                    break;
                case "location group":
                case "pc location group":
                case "pc_location_group":
                case "pc_locationgroup":
                    type = "PC_LocationGroup";
                    break;
                case "transaction":
                case "om transaction":
                case "om_transaction":
                    type = "OM_Transaction";
                    break;
                case "transaction line":
                case "transaction_line":
                case "om transaction line":
                case "om_transaction_line":
                case "om_transactionline":
                    type = "OM_TransactionLine";
                    break;
                case "transaction address":
                case "transaction_address":
                case "om transaction address":
                case "om_transaction_address":
                case "om_transactionaddress":
                    type = "OM_TransactionAddress";
                    break;
                case "transaction tax":
                case "transaction_tax":
                case "om transaction tax":
                case "om_transaction_tax":
                case "om_transactiontax":
                    type = "OM_TransactionTax";
                    break;
                case "transaction payment":
                case "transaction_payment":
                case "om transaction payment":
                case "om_transaction_payment":
                case "om_transactionpayment":
                    type = "OM_TransactionPayment";
                    break;
                case "transaction tracking":
                case "transaction_tracking":
                case "om transaction tracking":
                case "om_transaction_tracking":
                case "transaction tracking number":
                case "transaction_tracking_number":
                case "om transaction tracking number":
                case "om_transaction_tracking_number":
                case "om_transactiontrackingnumber":
                    type = "OM_TransactionTracking";
                    break;
                case "transaction note":
                case "transaction_note":
                case "om transaction note":
                case "om_transaction_note":
                case "om_transactionnote":
                case "transaction comment":
                case "transaction_comment":
                case "om transaction comment":
                case "om_transaction_comment":
                    type = "OM_TransactionComment";
                    break;
                case "customer":
                case "cm customer":
                case "cm_customer":
                    type = "CM_Customer";
                    break;
                case "customer category":
                case "customer_category":
                case "cm customer category":
                case "cm_customer_category":
                case "cm_customercategory":
                    type = "CM_CustomerCategory";
                    break;
                case "customer address":
                case "customer_address":
                case "cm customer address":
                case "cm_customer_address":
                case "cm_address":
                case "address":
                    type = "CM_Address";
                    break;
                case "customer contact":
                case "customer_contact":
                case "cm customer contact":
                case "cm_customer_contact":
                case "cm_customercontact":
                    type = "CM_CustomerContact";
                    break;
                //case "shipment":
                //case "om shipment":
                //case "om_shipment":
                //    type = "Shipment";
                //    break;
                //case "shipment line":
                //case "shipment_line":
                //case "om shipment line":
                //case "om_shipment_line":
                //    type = "Shipment Line";
                //    break;
                case "shipping method":
                case "shipping_method":
                case "om shipping method":
                case "om_shipping_method":
                case "om_shippingmethod":
                    type = "OM_ShippingMethod";
                    break;
                case "payment method":
                case "payment_method":
                case "om payment method":
                case "om_payment_method":
                case "om_paymentmethod":
                    type = "OM_PaymentMethod";
                    break;
                case "product option":
                case "product_option":
                case "pc product option":
                case "pc_product_option":
                case "pc_productoption":
                    type = "PC_ProductOption";
                    break;
                case "product option value":
                case "product_option_value":
                case "pc product option value":
                case "pc_product_option_value":
                case "pc_productoptionvalue":
                    type = "PC_ProductOptionValue";
                    break;
                case "product variant option":
                case "product_variant_option":
                case "pc product variant option":
                case "pc_product_variant_option":
                case "pc_productvariantoption":
                    type = "PC_ProductVariantOption";
                    break;
                case "product variant option value":
                case "product_variant_option_value":
                case "pc product variant option value":
                case "pc_product_variant_option_value":
                case "pc_productvariantoptionvalue":
                    type = "PC_ProductVariantOptionValue";
                    break;
                case "alternate id type":
                case "alternate_id_type":
                case "pc alternate id type":
                case "pc_alternateidtype":
                case "pc_alternate_id_type":
                    type = "PC_AlternateIdType";
                    break;
                case "alternate id":
                case "product alternate id":
                case "product_alternate_id":
                case "pc alternate id":
                case "pc_alternate_id":
                    type = "PC_ProductAlternateId";
                    break;
                case "variant alternate id":
                case "product variant alternate id":
                case "product_variant_alternate_id":
                case "pc variant alternate id":
                case "pc_variant_alternate_id":
                    type = "PC_ProductVariantAlternateId";
                    break;
                case "kit":
                case "product kit":
                case "catalog kit":
                case "pc kit":
                case "pc_kit":
                    type = "PC_Kit";
                    break;
                case "variant kit":
                case "product variant kit":
                case "variant_kit":
                case "pc variant kit":
                case "pc_variant_kit":
                case "pc_variantkit":
                    type = "PC_VariantKit";
                    break;
                case "variant kit component":
                case "product variant kit component":
                case "variant_kit_component":
                case "pc variant kit component":
                case "pc_variant_kit_component":
                case "pc_variantkitcomponent":
                    type = "PC_VariantKitComponent";
                    break;
                case "kit component":
                case "product kit component":
                case "catalog kit component":
                case "pc kit component":
                case "pc_kitcomponent":
                    type = "PC_KitComponent";
                    break;
                case "gift card":
                case "gift_card":
                case "gm gift card":
                case "gm_gift_card":
                case "gm_giftcard":
                    type = "GM_GiftCard";
                    break;
                case "gift card activity":
                case "gift_card_activity":
                case "gm gift card activity":
                case "gm_gift_card_activity":
                case "gm_activity":
                case "gm activity":
                case "gm_giftcardactivity":
                    type = "GM_GiftCardActivity";
                    break;
                case "transaction discount":
                case "transaction_discount":
                case "om transaction discount":
                case "om_transaction_discount":
                case "om_transactiondiscount":
                    type = "OM_TransactionDiscount";
                    break;
                case "category assignment":
                case "product category assignment":
                case "catalog category assignment":
                case "pc category assignment":
                case "pc_categoryassignment":
                    type = "PC_ProductCategoryAssignment";
                    break;
                case "variant category assignment":
                case "variant product category assignment":
                case "variant catalog category assignment":
                case "pc variant category assignment":
                case "pc_variantcategoryassignment":
                    type = "PC_ProductVariantCategoryAssignment";
                    break;
                default:
                    //type = userType;
                    break;
            }

            return type;
        }

        /// <summary>
        /// Checks if the dbType provided is within the correct API type.
        /// </summary>
        /// <param name="userType">Pretty name version of type provided by user</param> 
        /// <param name="dbType">Db Type version of type provided by user</param>
        /// <param name="apiType">Type of API being called from. Use transaction, giftcard, customer, or product.</param>
        /// <returns></returns>
        public static HttpResponseMessage CheckUserTypeAndApiCategory(string userType, string dbType, string apiType)
        {
            var message = new HttpResponseMessage();

            // Transaction table type provided by user, but not in transaction API
            if (dbType.ToLower().Contains("om_") && apiType.ToLower() != "transaction")
            {
                message = new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    ReasonPhrase = string.Format($"{userType} Table not found in {apiType} API. Please use Transaction API.")
                };
            }
            // Product table type provided by user, but not in product API
            else if (dbType.ToLower().Contains("pc_") && apiType.ToLower() != "product")
            {
                message = new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    ReasonPhrase = string.Format($"{userType} Table not found in {apiType} API. Please use Product API.")
                };
            }
            // GiftCard table type provided by user, but not in GiftCard API
            else if (dbType.ToLower().Contains("gm_") && apiType.ToLower() != "giftcard")
            {
                message = new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    ReasonPhrase = string.Format($"{userType} Table not found in {apiType} API. Please use Gift Card API.")
                };
            }
            // Customer table type provided by user, but not in Customer API
            else if (dbType.ToLower().Contains("cm_") && apiType.ToLower() != "customer")
            {
                message = new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    ReasonPhrase = string.Format($"{userType} Table not found in {apiType} API. Please use Customer API.")
                };
            }
            else
            {
                message = new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK
                };
            }

            return message;
        }

        /// <summary>
        /// Converts db table type to mapping collection type
        /// </summary>
        /// <param name="userType"></param>
        /// <returns></returns>
        public static string ConvertToMappingCollectionType(string userType)
        {
            string type = string.Empty;

            switch (userType.ToLower())
            {
                case "pc_product":
                case "pc product":
                case "product":
                    type = "Product";
                    break;
                case "variant":
                case "product variant":
                case "product_variant":
                case "pc product variant":
                case "pc_product_variant":
                    type = "Product Variant";
                    break;
                case "product unit":
                case "product_unit":
                case "pc product unit":
                case "pc_product_unit":
                    type = "Product Unit";
                    break;
                case "inventory":
                case "product inventory":
                case "product_inventory":
                case "pc product inventory":
                case "pc_product_inventory":
                    type = "Product Inventory";
                    break;
                case "variant inventory":
                case "variant_inventory":
                case "product variant inventory":
                case "product_variant_inventory":
                case "pc variant inventory":
                case "pc_variant_inventory":
                    type = "Product Variant Inventory";
                    break;
                case "catalog category":
                case "catalog_category":
                case "pc catalog category":
                case "pc_catalog_category":
                case "pc_catalogcategory":
                case "product category":
                case "product_category":
                case "category":
                    type = "Product Category";
                    break;
                case "location":
                case "pc location":
                case "pc_location":
                    type = "Location";
                    break;
                case "transaction":
                case "om transaction":
                case "om_transaction":
                    type = "Transaction";
                    break;
                case "transaction line":
                case "transaction_line":
                case "om transaction line":
                case "om_transaction_line":
                    type = "Transaction Line";
                    break;
                case "transaction address":
                case "transaction_address":
                case "om transaction address":
                case "om_transaction_address":
                    type = "Transaction Address";
                    break;
                case "transaction tax":
                case "transaction_tax":
                case "om transaction tax":
                case "om_transaction_tax":
                    type = "Transaction Tax";
                    break;
                case "transaction payment":
                case "transaction_payment":
                case "om transaction payment":
                case "om_transaction_payment":
                    type = "Transaction Payment";
                    break;
                case "transaction tracking number":
                case "transaction_tracking_number":
                case "om transaction tracking number":
                case "om_transaction_tracking_number":
                    type = "Transaction Tracking Number";
                    break;
                case "transaction note":
                case "transaction_note":
                case "om transaction note":
                case "om_transaction_note":
                case "om_transactioncomment":
                case "om transaction comment":
                case "om_transaction_comment":
                    type = "Transaction Note";
                    break;
                case "customer":
                case "cm customer":
                case "cm_customer":
                    type = "Customer";
                    break;
                case "customer category":
                case "customer_category":
                case "cm customer category":
                case "cm_customer_category":
                    type = "Customer Category";
                    break;
                case "customer address":
                case "customer_address":
                case "cm customer address":
                case "cm_customer_address":
                    type = "Customer Address";
                    break;
                case "customer contact":
                case "customer_contact":
                case "cm customer contact":
                case "cm_customer_contact":
                    type = "Customer Contact";
                    break;
                case "shipment":
                case "om shipment":
                case "om_shipment":
                    type = "Shipment";
                    break;
                case "shipment line":
                case "shipment_line":
                case "om shipment line":
                case "om_shipment_line":
                    type = "Shipment Line";
                    break;
                case "shipping method":
                case "shipping_method":
                case "om shipping method":
                case "om_shipping_method":
                    type = "Shipping Method";
                    break;
                case "payment method":
                case "payment_method":
                case "om payment method":
                case "om_payment_method":
                    type = "Payment Method";
                    break;
                case "product option":
                case "product_option":
                case "pc product option":
                case "pc_product_option":
                    type = "Product Option";
                    break;
                case "product option value":
                case "product_option_value":
                case "pc product option value":
                case "pc_product_option_value":
                    type = "Product Option Value";
                    break;
                case "product variant option":
                case "product_variant_option":
                case "pc product variant option":
                case "pc_product_variant_option":
                    type = "Product Variant Option";
                    break;
                case "product variant option value":
                case "product_variant_option_value":
                case "pc product variant option value":
                case "pc_product_variant_option_value":
                    type = "Product Variant Option Value";
                    break;
                case "gift card":
                case "gift_card":
                case "gm gift card":
                case "gm_gift_card":
                    type = "Gift Card";
                    break;
                case "gift card activity":
                case "gift_card_activity":
                case "gm gift card activity":
                case "gm_gift_card_activity":
                    type = "Gift Card Activity";
                    break;
                case "transaction discount":
                case "transaction_discount":
                case "om transaction discount":
                case "om_transaction_discount":
                    type = "Transaction Discount";
                    break;
                case "alternate id type":
                case "alternate_id_type":
                case "pc alternate id type":
                case "pc_alternate_id_type":
                    type = "Alternate Id Type";
                    break;
                case "alternate id":
                case "product alternate id":
                case "product_alternate_id":
                case "pc alternate id":
                case "pc_alternate_id":
                    type = "Product Alternate Id";
                    break;
                case "variant alternate id":
                case "product variant alternate id":
                case "product_variant_alternate_id":
                case "pc variant alternate id":
                case "pc_variant_alternate_id":
                    type = "Variant Alternate Id";
                    break;
                case "kit":
                case "product kit":
                case "catalog kit":
                case "pc kit":
                case "pc_kit":
                    type = "Kit";
                    break;
                case "kit component":
                case "product kit component":
                case "catalog kit component":
                case "pc kit component":
                case "pc_kitcomponent":
                    type = "Kit Component";
                    break;
                case "variant kit":
                case "product variant kit":
                case "variant_kit":
                case "pc variant kit":
                case "pc_variant_kit":
                    type = "Variant Kit";
                    break;
                case "variant kit component":
                case "product variant kit component":
                case "variant_kit_component":
                case "pc variant kit component":
                case "pc_variantkitcomponent":
                    type = "Variant Kit Component";
                    break;
                case "category assignment":
                case "product category assignment":
                case "catalog category assignment":
                case "pc category assignment":
                case "pc_categoryassignment":
                    type = "Product Category Assignment";
                    break;
                case "variant category assignment":
                case "variant product category assignment":
                case "variant catalog category assignment":
                case "pc variant category assignment":
                case "pc_variantcategoryassignment":
                    type = "Product Variant Category Assignment";
                    break;
                default:
                    break;
            }

            return type;
        }

        public static string ConvertObjectToTableName(Object o)
        {
            return ConvertTypeToTableName(o.GetType());
        }

        public static string ConvertTypeToTableName(Type type)
        {
            switch (type.Name)
            {
                case "ProductRequest":
                case "ProductResponse":
                    return "Product";
                case "CategoryResponse":
                    return "Product Category";
                case "ProductUnitRequest":
                case "ProductUnitResponse":
                    return "Product Unit";
                case "InventoryRequest":
                case "InventoryResponse":
                    return "Product Inventory";
                case "ProductVariantRequest":
                case "ProductVariantResponse":
                    return "Product Variant";
                //case "":
                //case "":
                //return "Product Variant Inventory";
                case "CustomerRequest":
                case "CustomerResponse":
                    return "Customer";
                case "TransactionRequest":
                case "TransactionResponse":
                    return "Transaction";
                case "TransactionLineRequest":
                case "TransactionLineResponse":
                    return "Transaction Line";
                case "ShippingAddressRequest":
                case "ShippingAddressResponse":
                    return "Transaction Address";
                case "TaxRequest":
                case "TaxResponse":
                    return "Transaction Tax";
                case "PaymentRequest":
                case "PaymentResponse":
                    return "Transaction Payment";
                case "TrackingRequest":
                case "TrackingResponse":
                    return "Transaction Tracking";
                case "NoteRequest":
                case "NoteResponse":
                    return "Transaction Comment";
                case "PaymentMethodRequest":
                case "PaymentMethodResponse":
                    return "Payment Method";
                case "CustomerCategoryRequest":
                case "CustomerCategoryResponse":
                    return "Customer Category";
                //case "":
                //case "":
                //return "Address";
                case "LocationRequest":
                case "LocationResponse":
                    return "Location";
                case "ShippingMethodRequest":
                case "ShippingMethodResponse":
                    return "Shipping Method";
                case "ProductCategoryRequest":
                    return "Product Category";
                case "OptionRequest":
                case "OptionResponse":
                    return "Product Option";
                case "OptionValueRequest":
                case "OptionValueResponse":
                    return "Product Option Value";
                //case "OptionValueRequest":
                //case "":
                //return "Product Variant Option";
                //case "":
                //case "":
                //return "Transaction Discount";
                case "GiftCardRequest":
                case "GiftCardResponse":
                    return "Gift Card";
                case "GiftCardActivityRequest":
                case "GiftCardActivityResponse":
                    return "Gift Card Activity";
                case "CategorySetAssignmentRequest":
                case "CategorySetAssignmentResponse":
                    return "Category Set";
                case "KitRequest":
                case "KitResponse":
                    return "Kit";
                case "KitComponentRequest":
                case "KitComponentResponse":
                    return "Kit Component";
                case "AlternateIdTypeRequest":
                case "AlternateIdTypeResponse":
                    return "Alternate Id Type";
                case "ProductAlternateIdRequest":
                case "ProductAlternateIdResponse":
                    return "Product Alternate Id";
                //case "":
                //case "":
                //return "Product Variant Alternate Id";
                default:
                    throw new Exception("ConvertTypeToTableName: Unable to determine table name from type: " + type.Name);
            }
        }
    }
}
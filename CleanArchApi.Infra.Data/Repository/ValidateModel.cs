using Sheev.Common.BaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tagge.Models.Interfaces;

namespace Tagge.Models
{
    /// <summary>
    /// This class is where all the validation happens to live
    /// </summary>
    public class ValidateModel // : IValidateModel
    {
        private readonly IProductModel _productModel;
        private readonly IVariantModel _variantModel;
        public ValidateModel(IVariantModel variantModel, IProductModel productModel)
        {
            _productModel = productModel;
            _variantModel = variantModel;
        }
        #region Validate Method(s)
        /// <summary>
        /// Validates a sku at either the product/variant level then validates the unit at the parent level
        /// </summary>
        /// <param name="sku"></param>
        /// <param name="unitName"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        /// <returns></returns>
        internal static async Task ValidateSkuAndUnit(string sku, string unitName, IBaseContextModel context, Guid trackingGuid)
        {
            // Create a product to test for Units
            var dbProduct = new Deathstar.Data.Models.PC_Product();

            // Product Component
            dbProduct = await ProductModel.ValidateBySku(sku, context, trackingGuid);

            if (dbProduct == null)
            {
                var dbVariant = await VariantModel.ValidateBySku(sku, context, trackingGuid);

                if (dbVariant == null)
                    throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"Invalid sku: {sku}" };

                dbProduct = await ProductModel.ValidateById(dbVariant.ParentId.ToString(), context, trackingGuid);
            }

            // Validate Unit
            var dbProductUnit = dbProduct.Units.FirstOrDefault(x => x.Name.ToLower() == unitName.ToLower() && x.IsActive);

            if (dbProductUnit == null)
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = $"FOR GLORY AND HONOR {unitName}" };
        }
        #endregion
    }
}

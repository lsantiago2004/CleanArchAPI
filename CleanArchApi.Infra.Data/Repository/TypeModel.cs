using MongoDB.Driver;
using Sheev.Common.BaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tagge.Models.Interfaces;

namespace Tagge.Models
{
    /// <summary>
    /// Type Model of course!
    /// </summary>
    public class TypeModel : ITypeModel
    {
        private readonly IBaseContextModel context;

        public TypeModel(IBaseContextModel contextModel)
        {
            context = contextModel;
        }
        #region Product Type
        /// <summary>
        /// Validate Product Types
        /// </summary>
        /// <param name="requestField"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        public async Task ValidateProductType(string requestField, Guid trackingGuid)
        {
            var productTypeCollection = context.Database.GetCollection<Deathstar.Data.Models.TM_Type>("TM_Type");

            // Filter & Field search
            var filters = Builders<Deathstar.Data.Models.TM_Type>.Filter.Eq(x => x.Value, requestField);
            filters = filters & Builders<Deathstar.Data.Models.TM_Type>.Filter.Eq(x => x.ST_TableName, "PC_Product");
            filters = filters & Builders<Deathstar.Data.Models.TM_Type>.Filter.Eq(x => x.TM_FieldName, "Type");
            var dbField = productTypeCollection.Find(filters).FirstOrDefault();

            if (dbField == null)
            {
                string reason = $"Product Type: {requestField} not found";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"{reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = reason };
            }
        }
        #endregion

        #region Related Type
        /// <summary>
        /// Validate Related Types
        /// </summary>
        /// <param name="requestField"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        public void ValidateRelatedType(string requestField, Guid trackingGuid)
        {
            var relatedTypeCollection = context.Database.GetCollection<Deathstar.Data.Models.TM_Type>("TM_Type");

            // Filter & Field search
            var filters = Builders<Deathstar.Data.Models.TM_Type>.Filter.Eq(x => x.Value, requestField);
            filters = filters & Builders<Deathstar.Data.Models.TM_Type>.Filter.Eq(x => x.ST_TableName, "PC_Product");
            filters = filters & Builders<Deathstar.Data.Models.TM_Type>.Filter.Eq(x => x.TM_FieldName, "RelatedType");
            var dbField = relatedTypeCollection.Find(filters).FirstOrDefault();

            if (dbField == null)
            {
                string reason = $"Related Type: {requestField} not found";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"{reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = reason };
            }
        }
        #endregion

        #region Status Type
        /// <summary>
        /// Validate Status Types
        /// </summary>
        /// <param name="requestField"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        public async Task ValidateStatusType(string requestField, Guid trackingGuid)
        {
            var statusTypeCollection = context.Database.GetCollection<Deathstar.Data.Models.TM_Type>("TM_Type");

            // Filter & Field search
            var filters = Builders<Deathstar.Data.Models.TM_Type>.Filter.Eq(x => x.Value, requestField);
            filters = filters & Builders<Deathstar.Data.Models.TM_Type>.Filter.Eq(x => x.ST_TableName, "PC_Product");
            filters = filters & Builders<Deathstar.Data.Models.TM_Type>.Filter.Eq(x => x.TM_FieldName, "Status");
            var dbField = await statusTypeCollection.Find(filters).FirstOrDefaultAsync();

            if (dbField == null)
            {
                string reason = $"Status: {requestField} not found";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"{reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = reason };
            }
        }
        #endregion

        #region Tracking Method Type
        /// <summary>
        /// Validate Tracking Method Types
        /// </summary>
        /// <param name="requestField"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        public async Task ValidateTrackingMethodType(string requestField, Guid trackingGuid)
        {
            var trackingMethodTypeCollection = context.Database.GetCollection<Deathstar.Data.Models.TM_Type>("TM_Type");

            // Filter & Field search
            var filters = Builders<Deathstar.Data.Models.TM_Type>.Filter.Eq(x => x.Value, requestField);
            filters = filters & Builders<Deathstar.Data.Models.TM_Type>.Filter.Eq(x => x.ST_TableName, "PC_Product");
            filters = filters & Builders<Deathstar.Data.Models.TM_Type>.Filter.Eq(x => x.TM_FieldName, "TrackingMethod");
            var dbField = trackingMethodTypeCollection.Find(filters).FirstOrDefault();

            if (dbField == null)
            {
                string reason = $"Tracking Method: {requestField} not found";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"{reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = reason };
            }
        }
        #endregion

        #region Kit Type
        /// <summary>
        /// Validate Kit Types
        /// </summary>
        /// <param name="requestField"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        public async Task ValidateKitType(string requestField, Guid trackingGuid)
        {
            var relatedTypeCollection = context.Database.GetCollection<Deathstar.Data.Models.TM_Type>("TM_Type");

            // Filter & Field search
            var filters = Builders<Deathstar.Data.Models.TM_Type>.Filter.Eq(x => x.Value, requestField);
            filters = filters & Builders<Deathstar.Data.Models.TM_Type>.Filter.Eq(x => x.ST_TableName, "PC_Kit");
            filters = filters & Builders<Deathstar.Data.Models.TM_Type>.Filter.Eq(x => x.TM_FieldName, "Type");
            var dbField = await relatedTypeCollection.Find(filters).FirstOrDefaultAsync();

            if (dbField == null)
            {
                string reason = $"Related Type: {requestField} not found";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"{reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = reason };
            }
        }
        #endregion

        #region Kit Component Type
        /// <summary>
        /// Validate Kit Component Types
        /// </summary>
        /// <param name="requestField"></param>
        /// <param name="context"></param>
        /// <param name="trackingGuid"></param>
        public async Task ValidateKitComponentType(string requestField, Guid trackingGuid)
        {
            var relatedTypeCollection = context.Database.GetCollection<Deathstar.Data.Models.TM_Type>("TM_Type");

            // Filter & Field search
            var filters = Builders<Deathstar.Data.Models.TM_Type>.Filter.Eq(x => x.Value, requestField);
            filters = filters & Builders<Deathstar.Data.Models.TM_Type>.Filter.Eq(x => x.ST_TableName, "PC_KitComponent");
            filters = filters & Builders<Deathstar.Data.Models.TM_Type>.Filter.Eq(x => x.TM_FieldName, "Type");
            var dbField = await relatedTypeCollection.Find(filters).FirstOrDefaultAsync();

            if (dbField == null)
            {
                string reason = $"Related Type: {requestField} not found";

                IG2000.Data.Utilities.Logging.LogTrackingEvent($"{reason}", $"Status Code: {Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest}", LT319.Common.Utilities.Constants.TrackingStatus.Error, context, trackingGuid);
                throw new HttpResponseException() { StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest, ReasonPhrase = reason };
            }
        }
        #endregion
    }
}

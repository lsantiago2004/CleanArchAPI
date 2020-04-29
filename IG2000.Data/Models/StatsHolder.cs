using System;
using System.Collections.Generic;
using System.Text;

namespace IG2000.Data.Models
{
    /// <summary>
    /// StatsHolder class holds information and methods for summary logging. 
    /// </summary>
    public class StatsHolder
    {
        /// <summary>
        /// ControllerMethod string value should state the controller method e.g. "Save Product"
        /// </summary>
        public string ControllerMethod { get; set; }

        /// <summary>
        /// Whether we are in detail logging mode or summary logging mode
        /// </summary>
        public bool DetailLogging { get; set; }

        /// <summary>
        /// Enum for which CRUD action is being performed
        /// </summary>
        public enum Action
        {
            CREATED, 
            UPDATED, 
            RETRIEVED, 
            DELETED, 
            REACTIVATED
        }

        /// <summary>
        /// FieldMaps = Dictionary of Action, FieldMap. 
        /// FieldMap is a dictionary that stores the field, and number of entries being acted on.e.g. ("Address external ids", 3)
        /// </summary>
        private Dictionary<Action, Dictionary<string, int>> FieldMaps = new Dictionary<Action, Dictionary<string, int>>();

        /// <summary>
        /// Setting a field in the field map. First, have to check if it already exists
        /// </summary>
        /// <param name="field"></param>
        /// <param name="numberCreated"></param>
        public void SetField(string field, int numberCreated, Action action)
        {
            // Getting FieldMap dictionary based on action
            if(FieldMaps.TryGetValue(action, out Dictionary<string, int> fieldMap))
            {
                // If field already exists in FieldMap, increase value by the number provided
                if (fieldMap.TryGetValue(field, out int value))
                {
                    int newValue = value + numberCreated;

                    fieldMap[field] = newValue;
                }
                else // Otherwise just add it to the FieldMap dictionary
                {
                    fieldMap.TryAdd(field, numberCreated);
                }
            }
            else // No FieldMap for this action yet, so add to FieldMaps
            {
                Dictionary<string, int> newFieldMap = new Dictionary<string, int>();
                newFieldMap.TryAdd(field, numberCreated);
                FieldMaps.TryAdd(action, newFieldMap);
            }
        }

       /// <summary>
       /// Looping through FieldMaps and sending information to logger
       /// </summary>
       /// <param name="context"></param>
       /// <param name="trackingGuid"></param>
        public void logSummaryInfo(Sheev.Common.BaseModels.BaseContextModel context, Guid trackingGuid)
        {
            // Looping through each action
            foreach (KeyValuePair<Action, Dictionary<string, int>> fieldMap in FieldMaps)
            {
                // Looping through each FieldMap entry for each action
                foreach (KeyValuePair<string, int> fieldMapEntry in fieldMap.Value)
                {
                    // Sending each summary to the logger. e.g. "Address external ids updated: 3"
                    Utilities.Logging.LogTrackingEvent($"{fieldMapEntry.Key} {fieldMap.Key.ToString().ToLower()}: {fieldMapEntry.Value}", ControllerMethod, LT319.Common.Utilities.Constants.TrackingStatus.Active, context, trackingGuid);
                }
                
            }
        }
    }
}

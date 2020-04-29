using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;

namespace IG2000.Data.Utilities
{
    public class ThreadedLogger_TechDetail : ThreadedLogger
    {
        public static void DummyMethod()
        {
            // Call this method to get a breakpoint in this class from an external executable
        }

        protected override void AsyncLogMessage(object row, Sheev.Common.BaseModels.IBaseContextModel context)
        {
            Sheev.Common.Models.BackupLogRequest logRequest = ((Sheev.Common.Models.BackupLogRequest)row);

            try
            {
                // This is now under MongoDB control
                // MongoDB Settings
                string connectionString = context.MongoDbSettings.Value.ConnectionString;
                var database = context.MongoDbSettings.Value.Databases.FirstOrDefault(x => x.Name == "IG2000");
                string collectionName = database.Collections.FirstOrDefault(x => x.Name == "IG_Log").Name;

                // Get MongoDB
                var client = new MongoClient(connectionString);
                var db = client.GetDatabase(database.Name);
                var logCollection = db.GetCollection<IG2000.Data.Models.IG_Log>(collectionName);

                // Word up... its the code word
                var dbLog = new IG2000.Data.Models.IG_Log();

                //Convert Request
                dbLog.ConvertToDatabaseObject(logRequest, (Guid)logRequest.CompanyId);

                //Add Timestamp
                dbLog.Timestamp = DateTimeOffset.Now.ToString("yyyy/MM/dd hh:mm:ss.fff tt zzz");

                //Insert
                logCollection.InsertOne(dbLog);
            }
            catch (Exception ex)
            {
                // Backup Logger
                var restClient = new RestSharp.RestClient(context.ApiUrlSettings.Value.BackupLogger_URL);
                RestSharp.RestRequest req = new RestSharp.RestRequest("/Todd/Damnit", RestSharp.Method.POST);
                var bodyJSON = JsonConvert.SerializeObject(logRequest);
                req.AddParameter("application/json", bodyJSON, ParameterType.RequestBody);
                var resp = (RestSharp.RestResponse)restClient.Execute(req);
            }
        }
    }
}

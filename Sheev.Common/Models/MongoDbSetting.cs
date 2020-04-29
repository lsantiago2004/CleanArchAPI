using System;
using System.Collections.Generic;
using System.Text;

namespace Sheev.Common.Models
{
    public class MongoDbSetting
    {
        #region Properties
        public string ConnectionString { get; set; }
        public double TimoutInSeconds { get; set; }
        public List<Database> Databases { get; set; }
        public List<Collection> Collections { get; set; }
        #endregion

        #region Constructor(s)
        public MongoDbSetting()
        {
            Databases = new List<Database>();
            Collections = new List<Collection>();
        }
        #endregion
    }

    public class Database
    {
        public string Name { get; set; }
        public List<Collection> Collections { get; set; }
    }

    public class Collection
    {
        public string Name { get; set; }
    }
}

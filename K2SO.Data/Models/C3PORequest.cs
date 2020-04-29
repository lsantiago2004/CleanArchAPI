using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace K2SO.Data.Models
{
    public class C3PORequest
    {
        #region Properties
        [JsonProperty("username", Order = 10)]
        public string Username { get; set; }

        [JsonProperty("password", Order = 20)]
        public string Password { get; set; }

        //[JsonProperty("run_type", Order = 30)]
        //public Spaceport.Data.Utilities.Constants.TS_RunType RunType { get; set; }

        [JsonProperty("run_type", Order = 30)]
        public string RunType { get; set; }
        #endregion

        #region Constructor(s)
        #endregion
    }
}

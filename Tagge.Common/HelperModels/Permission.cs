﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tagge.Common.HelperModels
{
    public class Permission
    {
        #region Properties
        public int Id { get; set; }
        public string Name { get; set; }
        public int AccessTypeId { get; set; }
        public string AccessType { get; set; }
        #endregion
    }
}

﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pegasus_backend.pegasusContext
{
    public partial class RoleAccess
    {
        public short? RoleId { get; set; }
        public int? PageId { get; set; }
        public short RoleAccessId { get; set; }

        public Page Page { get; set; }
        [JsonIgnore]
        public Role Role { get; set; }
    }
}

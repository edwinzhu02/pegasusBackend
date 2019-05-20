﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pegasus_backend.pegasusContext
{
    public partial class LearnerOthers
    {
        public int LearnerId { get; set; }
        public string OthersType { get; set; }
        public short? OthersValue { get; set; }
        public int LearnerOthersId { get; set; }
        public short? LearnerLevel { get; set; }

        [JsonIgnore]
        public Learner Learner { get; set; }
    }
}

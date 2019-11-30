using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class TestTodoList
    {
        public int Id { get; set; }
        public string Todo { get; set; }
        public DateTime? CreateAt { get; set; }
        public string Comments { get; set; }
    }
}

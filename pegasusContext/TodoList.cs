using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class TodoList
    {
        public int ListId { get; set; }
        public string ListName { get; set; }
        public string ListContent { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public short? ProcessFlag { get; set; }
        public short? UserId { get; set; }

        public User User { get; set; }
    }
}

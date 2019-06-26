using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class ChatMessage
    {
        public int? ChatMessageId { get; set; }
        public int? ChatGroupId { get; set; }
        public string MessageBody { get; set; }
        public short? UserId { get; set; }
        public DateTime? CreateAt { get; set; }

        public ChatGroup ChatGroup { get; set; }
        public User User { get; set; }
    }
}

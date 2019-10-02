using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class ChatMessage
    {
        public int ChatMessageId { get; set; }
        public string ChatGroupId { get; set; }
        public string MessageBody { get; set; }
        public short? SenderUserId { get; set; }
        public DateTime? CreateAt { get; set; }
        public short? ReceiverUserId { get; set; }

        public virtual ChatGroup ChatGroup { get; set; }
        public virtual User ReceiverUser { get; set; }
        public virtual User SenderUser { get; set; }
    }
}

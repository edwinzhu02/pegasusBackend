using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pegasus_backend.Models
{
    public class ChatMessageModel
    {
        public int ChatMessageId { get; set; }
        public string ChatGroupId { get; set; }
        public string MessageBody { get; set; }
        public short? SenderUserId { get; set; }
        public DateTime? CreateAt { get; set; }
        public short? ReceiverUserId { get; set; }
    }
}

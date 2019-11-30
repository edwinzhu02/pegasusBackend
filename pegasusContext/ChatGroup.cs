using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class ChatGroup
    {
        public ChatGroup()
        {
            ChatMessage = new HashSet<ChatMessage>();
        }

        public string ChatGroupId { get; set; }
        public short? UserId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public short? IsActive { get; set; }

        public User User { get; set; }
        public ICollection<ChatMessage> ChatMessage { get; set; }
    }
}

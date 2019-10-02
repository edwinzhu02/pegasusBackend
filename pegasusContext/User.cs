using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class User
    {
        public User()
        {
            AskOff = new HashSet<AskOff>();
            ChatGroup = new HashSet<ChatGroup>();
            ChatMessageReceiverUser = new HashSet<ChatMessage>();
            ChatMessageSenderUser = new HashSet<ChatMessage>();
            Learner = new HashSet<Learner>();
            LoginLog = new HashSet<LoginLog>();
            Staff = new HashSet<Staff>();
            Teacher = new HashSet<Teacher>();
            TodoList = new HashSet<TodoList>();
        }

        public short UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public DateTime? CreatedAt { get; set; }
        public short? RoleId { get; set; }
        public short? IsActivate { get; set; }
        public int? UnreadMessageId { get; set; }
        public string Signature { get; set; }
        public short? IsOnline { get; set; }

        public virtual Role Role { get; set; }
        public virtual OnlineUser OnlineUser { get; set; }
        public virtual ICollection<AskOff> AskOff { get; set; }
        public virtual ICollection<ChatGroup> ChatGroup { get; set; }
        public virtual ICollection<ChatMessage> ChatMessageReceiverUser { get; set; }
        public virtual ICollection<ChatMessage> ChatMessageSenderUser { get; set; }
        public virtual ICollection<Learner> Learner { get; set; }
        public virtual ICollection<LoginLog> LoginLog { get; set; }
        public virtual ICollection<Staff> Staff { get; set; }
        public virtual ICollection<Teacher> Teacher { get; set; }
        public virtual ICollection<TodoList> TodoList { get; set; }
    }
}

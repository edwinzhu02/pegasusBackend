using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class User
    {
        public User()
        {
            AskOff = new HashSet<AskOff>();
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

        public Role Role { get; set; }
        public OnlineUser OnlineUser { get; set; }
        public ICollection<AskOff> AskOff { get; set; }
        public ICollection<Learner> Learner { get; set; }
        public ICollection<LoginLog> LoginLog { get; set; }
        public ICollection<Staff> Staff { get; set; }
        public ICollection<Teacher> Teacher { get; set; }
        public ICollection<TodoList> TodoList { get; set; }
    }
}

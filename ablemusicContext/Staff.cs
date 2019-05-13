﻿using System;
using System.Collections.Generic;

namespace Pegasus_backend.ablemusicContext
{
    public partial class Staff
    {
        public Staff()
        {
            Payment = new HashSet<Payment>();
            StaffOrg = new HashSet<StaffOrg>();
            StockOrder = new HashSet<StockOrder>();
        }

        public string FirstName { get; set; }
        public string Visa { get; set; }
        public string LastName { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? Dob { get; set; }
        public short? IsActivate { get; set; }
        public short? Gender { get; set; }
        public short? UserId { get; set; }
        public string IrdNumber { get; set; }
        public short? IdType { get; set; }
        public string IdPhoto { get; set; }
        public string IdNumber { get; set; }
        public string HomePhone { get; set; }
        public string MobilePhone { get; set; }
        public string Email { get; set; }
        public string Photo { get; set; }
        public short StaffId { get; set; }

        public User User { get; set; }
        public ICollection<Payment> Payment { get; set; }
        public ICollection<StaffOrg> StaffOrg { get; set; }
        public ICollection<StockOrder> StockOrder { get; set; }
    }
}

using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace Pegasus_backend.Models
{
    public class PaymentMethod
    {
        public static int GetPaymentMethod(string stringPayment)
        {
            switch (stringPayment)
            {
                case "Cash":
                    return 1;
                case "Eftpos":
                    return 2;
                case "OnlineTransfer":
                    return 3;
                default:
                    throw new Exception("Payment method does not found.");
                    
            }
        }
    }
}
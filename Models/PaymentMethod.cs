using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace Pegasus_backend.Models
{
    public class PaymentMethod
    {
        public static string GetPaymentMethod(byte num)
        {
            switch (num)
            {
                case 1:
                    return "Cash";
                case 2:
                    return "Eftpos";
                case 3:
                    return "OnlineTransfer";
                default:
                    throw new Exception("Payment method does not found.");
                    
            }
        }
    }
}
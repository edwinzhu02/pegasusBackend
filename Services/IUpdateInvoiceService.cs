using Pegasus_backend.pegasusContext;
using System.Collections.Generic;

namespace Pegasus_backend.Services
{
    public interface IInvoiceUpdateService
    {
        bool InvoiceUpdate(List<int> lessonIds);
    }
}
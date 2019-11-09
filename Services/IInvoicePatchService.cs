using Pegasus_backend.pegasusContext;
using System.Collections.Generic;

namespace Pegasus_backend.Services
{
    public interface IInvoicePatchService
    {
        bool InvoicePatch(List<int> courseIds);
    }
}
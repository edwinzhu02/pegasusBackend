using Pegasus_backend.pegasusContext;

namespace Pegasus_backend.Services
{
    public interface IPromotionService
    {
        bool PromotionInvoice(ref InvoiceWaitingConfirm invoice);
    }
}
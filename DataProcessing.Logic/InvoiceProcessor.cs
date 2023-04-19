using DataProcessing.Contract;
using DataProcessing.Contract.Domain;

namespace DataProcessing.Logic;

public sealed class InvoiceProcessor: IInvoiceProcessor
{
    public bool Process(Invoice invoice)
    {
        // DO SOMETHING
        return true;
    }
}
using DataProcessing.Contract.Domain;

namespace DataProcessing.Contract;

public interface IInvoiceProcessor
{
    bool Process(Invoice invoice);
}
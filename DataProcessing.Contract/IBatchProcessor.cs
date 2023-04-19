using DataProcessing.Contract.Domain;
using DataProcessing.Contract.Outputs;

namespace DataProcessing.Contract;

public interface IBatchProcessor
{
    IEnumerable<BatchResult> ProcessInvoices(IReadOnlyCollection<Invoice> invoices);
}
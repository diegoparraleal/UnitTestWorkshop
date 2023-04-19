using System.Collections.Immutable;
using DataProcessing.Core;
using DataProcessing.Contract;
using DataProcessing.Contract.Domain;
using DataProcessing.Contract.Outputs;
using LaYumba.Functional;
using static LaYumba.Functional.F;

namespace DataProcessing.Logic;

public sealed class BatchProcessor: IBatchProcessor
{
    private readonly IInventoryManager _inventoryManager;
    private readonly IClientManager _clientManager;
    private readonly IInvoiceProcessor _invoiceProcessor;

    public BatchProcessor(
        IInventoryManager inventoryManager,
        IClientManager clientManager,
        IInvoiceProcessor invoiceProcessor
        )
    {
        _inventoryManager = inventoryManager;
        _clientManager = clientManager;
        _invoiceProcessor = invoiceProcessor;
    }

    public IEnumerable<BatchResult> ProcessInvoices(IReadOnlyCollection<Invoice> invoices) 
        => _inventoryManager.LoadInventory()
            .Pipe(inventory => 
                invoices.Select(ToBatchResult)
                    .Select(ValidateInvoice)
                    .Select(ValidateClient)
                    .Select(ValidateInventory, inventory)
                    .Select(ProcessValidInvoice)
            );
    private BatchResult ValidateInvoice(BatchResult input) 
        => input.Invoice switch
        {
            { Items: null } or {Items.Count: 0 } => input.AddFailure("The invoice should at least one item"),
            { Items: var items } when items.Any(HasZeroOrNegativeQuantity) => input.AddFailure("The invoice should contain positive quantities"),
            _ => input
        };
    private BatchResult ValidateClient(BatchResult input)
        => input.Invoice.Client == null
            ? input.AddFailure("The invoice should have a client")
            : _clientManager.Get(input.Invoice.Client.Id)
                .Match(
                    () => input.AddFailure($"Client with id {input.Invoice.Client.Id} does not exist"),
                    _ => input);
    private BatchResult ValidateInventory(BatchResult input, Inventory inventory) 
        => input.Invoice.Items
            .Bind(ValidateProductInInventory, inventory)
            .Match(() => input, input.AddFailures);
    private Option<string> ValidateProductInInventory(Item item, Inventory inventory) 
        => !inventory.HasProduct(item.Product) 
            ? $"Missing product {item.Product.Name} in inventory" 
            : !inventory.HasQuantity(item.Product, item.Quantity)
                ? $"Not enough quantity ({item.Quantity}) in inventory for product {item.Product.Name}"
                : None;
    private BatchResult ProcessValidInvoice(BatchResult input) 
        => input.Success
            ? ProcessInvoiceSafely(input.Invoice)
                .Match(() => input, input.AddFailure)
            : input;

    private Option<string> ProcessInvoiceSafely(Invoice invoice) 
        => Try(() => _invoiceProcessor.Process(invoice))
            .Invoke()
            .Match(e => e.Message.AsOption(),
                result => result ? None : $"Could not process invoice #{invoice.Id}");
    private BatchResult ToBatchResult(Invoice invoice) => new (invoice, true, ImmutableList<string>.Empty);
    private bool HasZeroOrNegativeQuantity(Item item) => item.Quantity <= 0;
}
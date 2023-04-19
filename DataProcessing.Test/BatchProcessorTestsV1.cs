using System.Collections.Immutable;
using DataProcessing.Contract;
using DataProcessing.Contract.Domain;
using DataProcessing.Contract.Outputs;
using DataProcessing.Logic;
using Newtonsoft.Json;
using NSubstitute;
using static LaYumba.Functional.F;

namespace DataProcessing.Test;

public class BatchProcessorTestsV1
{
    [Test]
    public void Test_everything_works()
    {
        var inventoryManager = Substitute.For<IInventoryManager>();
        inventoryManager.LoadInventory().Returns(new Inventory(new Dictionary<Product, int>
        {
            { new Product(1, "Wand", 10), 100 },
            { new Product(2, "Book", 30), 100 },
            { new Product(3, "Broom", 100), 100 },
            { new Product(4, "Cat litter box", 15), 100 },
            { new Product(6, "Chocolate", 1), 10 },
            { new Product(7, "Alchemy set", 1000), 1 }
        }));
        
        var clientManager = Substitute.For<IClientManager>();
        clientManager.Get(1).Returns(new Client(1, "Harry", "Potter"));
        clientManager.Get(2).Returns(new Client(2, "Hermione", "Granger"));
        clientManager.Get(3).Returns(new Client(3, "Ron", "Weasley"));
        clientManager.Get(4).Returns(new Client(4, "Neville", "Longbottom"));
        clientManager.Get(5).Returns(new Client(5, "Draco", "Malfoy"));
        clientManager.Get(6).Returns(None);
        
        var invoiceProcessor = Substitute.For<IInvoiceProcessor>();
        invoiceProcessor.Process(Arg.Any<Invoice>()).Returns(true, false);
        
        var batchProcessor = new BatchProcessor(inventoryManager, clientManager, invoiceProcessor);
        var results = batchProcessor.ProcessInvoices(new[]
        {
            // HAPPY CASE
            new Invoice(1, new Client(1, "Harry", "Potter"),
                new Item[]
                {
                    new(new Product(1, "Wand", 10), 2), 
                    new(new Product(2, "Book", 30), 5), 
                    new(new Product(3, "Broom", 100), 1)
                }),
            // VALID, BUT INVOICE PROCESSING FAILED
            new Invoice(2, new Client(2, "Hermione", "Granger"),
                new Item[]
                {
                    new(new Product(1, "Wand", 10), 1), 
                    new(new Product(2, "Book", 30), 20),
                    new(new Product(4, "Cat litter box", 15), 1)
                }),
            // NO PRODUCT: MOUSE FOOD
            new Invoice(3, new Client(3, "Ron", "Weasley"),
                new Item[]
                {
                    new(new Product(1, "Wand", 10), 2), 
                    new(new Product(2, "Book", 30), 1),
                    new(new Product(5, "Mouse food", 2), 5)
                }),
            // NO QUANTITY AVAILABLE: CHOCOLATE
            new Invoice(4, new Client(4, "Neville", "Longbottom"),
                new Item[]
                {
                    new(new Product(1, "Wand", 10), 2), 
                    new(new Product(2, "Book", 30), 1),
                    new(new Product(6, "Chocolate", 1), 20)
                }),
            // NO CLIENT IN INVOICE
            new Invoice(5, null!, new Item[]
            {
                new(new Product(1, "Wand", 10), 1)
            }),
            // NO ITEMS IN INVOICE
            new Invoice(6, new Client(5, "Draco", "Malfoy"), new Item[] { }),
            // NEGATIVE QUANTITIES IN INVOICE
            new Invoice(7, new Client(5, "Draco", "Malfoy"), 
                new Item[]
                {
                    new(new Product(1, "Wand", 10), -1),
                    new(new Product(2, "Book", 30), 1)
                }),
            // CLIENT DOES NOT EXIST
            new Invoice(8, new Client(6, "Severus", "Snape"), 
                new Item[]
                {
                    new(new Product(7, "Alchemy set", 1000), 1)
                })
        }).ToList();
        var expected = new BatchResult[]
        {
            new(new Invoice(1, new Client(1, "Harry", "Potter"),
                new Item[]
                {
                    new(new Product(1, "Wand", 10), 2), 
                    new(new Product(2, "Book", 30), 5), 
                    new(new Product(3, "Broom", 100), 1)
                }), true,ImmutableList<string>.Empty),
            new(new Invoice(2, new Client(2, "Hermione", "Granger"),
                new Item[]
                {
                    new(new Product(1, "Wand", 10), 1), 
                    new(new Product(2, "Book", 30), 20),
                    new(new Product(4, "Cat litter box", 15), 1)
                }), false, ImmutableList.Create("Could not process invoice #2")),
            // NO PRODUCT: MOUSE FOOD
            new(new Invoice(3, new Client(3, "Ron", "Weasley"),
                new Item[]
                {
                    new(new Product(1, "Wand", 10), 2), 
                    new(new Product(2, "Book", 30), 1),
                    new(new Product(5, "Mouse food", 2), 5)
                }), false, ImmutableList.Create("Missing product Mouse food in inventory")),
            // NO QUANTITY AVAILABLE: CHOCOLATE
            new( new Invoice(4, new Client(4, "Neville", "Longbottom"),
                new Item[]
                {
                    new(new Product(1, "Wand", 10), 2), 
                    new(new Product(2, "Book", 30), 1),
                    new(new Product(6, "Chocolate", 1), 20)
                }), false, ImmutableList.Create("Not enough quantity (20) in inventory for product Chocolate")),
            // NO CLIENT IN INVOICE
            new(new Invoice(5, null!, new Item[]
            {
                new(new Product(1, "Wand", 10), 1)
            }), false, ImmutableList.Create("The invoice should have a client")),
            // NO ITEMS IN INVOICE
            new(new Invoice(6, new Client(5, "Draco", "Malfoy"), new Item[] { })
                ,false, ImmutableList.Create("The invoice should at least one item")),
            // NEGATIVE QUANTITIES IN INVOICE
            new(new Invoice(7, new Client(5, "Draco", "Malfoy"), 
                new Item[]
                {
                    new(new Product(1, "Wand", 10), -1),
                    new(new Product(2, "Book", 30), 1)
                }),false, ImmutableList.Create("The invoice should contain positive quantities")),
            // CLIENT DOES NOT EXIST
            new(new Invoice(8, new Client(6, "Severus", "Snape"), 
                new Item[]
                {
                    new(new Product(7, "Alchemy set", 1000), 1)
                }),false, ImmutableList.Create("Client with id 6 does not exist"))
        };
        Assert.AreEqual(JsonConvert.SerializeObject(expected), JsonConvert.SerializeObject(results));
        inventoryManager.Received(1).LoadInventory();
        inventoryManager.ReceivedWithAnyArgs(0).PersistInventory(default!);
        inventoryManager.ReceivedWithAnyArgs(0).AddProduct(default!, default!, default!);
        inventoryManager.ReceivedWithAnyArgs(0).SubtractProduct(default!, default!, default!);
        clientManager.Received(1).Get(1);
        clientManager.Received(1).Get(2);
        clientManager.Received(1).Get(3);
        clientManager.Received(1).Get(4);
        clientManager.Received(2).Get(5);
        clientManager.Received(1).Get(6);
    }
}
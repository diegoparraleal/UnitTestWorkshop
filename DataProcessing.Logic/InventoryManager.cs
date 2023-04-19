using DataProcessing.Contract;
using DataProcessing.Contract.Domain;

namespace DataProcessing.Logic;

public sealed class InventoryManager: IInventoryManager
{
    // SHOULD LOAD FROM A DB FOR EXAMPLE
    public Inventory LoadInventory() => new();
    public bool PersistInventory(Inventory inventory)
    {
        // SHOULD PERSIST INTO A DB FOR EXAMPLE 
        return true;
    }

    public Inventory AddProduct(Inventory inventory, Product product, int quantity) 
        => inventory.Products.TryGetValue(product, out var currentQuantity) 
            ? new Inventory(inventory.Products.SetItem(product, currentQuantity + quantity)) 
            : new Inventory(inventory.Products.Add(product, quantity));

    public Inventory SubtractProduct(Inventory inventory, Product product, int quantity)
    => inventory.Products.TryGetValue(product, out var currentQuantity) 
            ? quantity < currentQuantity 
                ? new Inventory(inventory.Products.SetItem(product, currentQuantity - quantity))
                : throw new InvalidOperationException($"Not enough items for product {product.Name} in inventory") 
            : throw new InvalidOperationException($"Cannot subtract from product {product.Name} because it's not in inventory");
}
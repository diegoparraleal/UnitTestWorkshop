using DataProcessing.Contract.Domain;

namespace DataProcessing.Contract;

public interface IInventoryManager
{
    Inventory LoadInventory();
    bool PersistInventory(Inventory inventory);
    Inventory AddProduct(Inventory inventory, Product product, int quantity);
    Inventory SubtractProduct(Inventory inventory, Product product, int quantity);
}
using System.Collections.Immutable;
using LaYumba.Functional;

namespace DataProcessing.Contract.Domain;

public sealed class Inventory
{
    public ImmutableDictionary<Product, int> Products { get; } 

    public Inventory()
    {
        Products = ImmutableDictionary.Create<Product, int>();
    }
    public Inventory(IReadOnlyDictionary<Product, int> products)
    {
        Products = products.ToImmutableDictionary();
    }

    public bool HasProduct(Product product) => Products.ContainsKey(product);
    
    public bool HasQuantity(Product product, int quantity) 
        => Products.TryGetValue(product, out var currentQuantity) && currentQuantity >= quantity;
}
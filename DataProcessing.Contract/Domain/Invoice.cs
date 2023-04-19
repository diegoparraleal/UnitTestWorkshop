namespace DataProcessing.Contract.Domain;

public sealed record Invoice(int Id, Client Client, IReadOnlyCollection<Item> Items)
{
    public decimal Total => Items.Sum(x => x.Quantity * x.Product.UnitPrice);
}
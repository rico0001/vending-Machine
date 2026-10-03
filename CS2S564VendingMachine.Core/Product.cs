namespace CS2S564VendingMachine.Core;

public sealed class Product
{
    public Product(string id, string name, int unitPricePence, string imageFileName)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Product ID is required.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Product name is required.", nameof(name));
        }

        if (unitPricePence <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPricePence), "Price must be greater than zero.");
        }

        Id = id.Trim();
        Name = name.Trim();
        UnitPricePence = unitPricePence;
        ImageFileName = imageFileName?.Trim() ?? string.Empty;
    }

    public string Id { get; }
    public string Name { get; }
    public int UnitPricePence { get; }
    public string ImageFileName { get; }
}

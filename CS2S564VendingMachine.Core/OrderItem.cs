namespace CS2S564VendingMachine.Core;

public sealed class OrderItem
{
    public OrderItem(Product product, int quantity = 1)
    {
        Product = product ?? throw new ArgumentNullException(nameof(product));
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be at least 1.");
        }

        Quantity = quantity;
    }

    public Product Product { get; }
    public int Quantity { get; private set; }
    public int SubtotalPence => Product.UnitPricePence * Quantity;

    public void Increase(int by = 1)
    {
        if (by <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(by));
        }

        Quantity += by;
    }

    /// <summary>
    /// Decreases quantity by one. Returns true when the line still has remaining items.
    /// </summary>
    public bool DecreaseOne()
    {
        if (Quantity <= 1)
        {
            Quantity = 0;
            return false;
        }

        Quantity--;
        return true;
    }

    public string ToBasketLine()
    {
        return $"{Product.Name}  ×{Quantity}    {MoneyFormatter.FormatPence(SubtotalPence)}";
    }
}

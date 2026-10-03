namespace CS2S564VendingMachine.Core;

public sealed class ShoppingBasket
{
    private readonly List<OrderItem> _items = new();

    public IReadOnlyList<OrderItem> Items => _items;
    public bool IsEmpty => _items.Count == 0;
    public int TotalPence => _items.Sum(item => item.SubtotalPence);
    public int TotalItemCount => _items.Sum(item => item.Quantity);

    public OrderItem Add(Product product)
    {
        if (product is null)
        {
            throw new ArgumentNullException(nameof(product));
        }

        var existing = _items.FirstOrDefault(item => item.Product.Id == product.Id);
        if (existing is not null)
        {
            existing.Increase();
            return existing;
        }

        var created = new OrderItem(product);
        _items.Add(created);
        return created;
    }

    public bool RemoveOne(string productId)
    {
        var existing = _items.FirstOrDefault(item => item.Product.Id == productId);
        if (existing is null)
        {
            return false;
        }

        if (!existing.DecreaseOne())
        {
            _items.Remove(existing);
        }

        return true;
    }

    public bool RemoveLine(string productId)
    {
        var existing = _items.FirstOrDefault(item => item.Product.Id == productId);
        if (existing is null)
        {
            return false;
        }

        _items.Remove(existing);
        return true;
    }

    public void Clear() => _items.Clear();

    public IReadOnlyList<TransactionLine> ToTransactionLines()
    {
        return _items
            .Select(item => new TransactionLine(item.Product.Id, item.Product.Name, item.Quantity, item.Product.UnitPricePence, item.SubtotalPence))
            .ToList();
    }
}

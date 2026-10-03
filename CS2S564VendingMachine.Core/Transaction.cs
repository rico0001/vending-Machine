namespace CS2S564VendingMachine.Core;

public sealed class TransactionLine
{
    public TransactionLine(string productId, string productName, int quantity, int unitPricePence, int subtotalPence)
    {
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPricePence = unitPricePence;
        SubtotalPence = subtotalPence;
    }

    public string ProductId { get; }
    public string ProductName { get; }
    public int Quantity { get; }
    public int UnitPricePence { get; }
    public int SubtotalPence { get; }
}

public sealed class Transaction
{
    public Transaction(
        string transactionId,
        DateTimeOffset completedAt,
        int orderNumber,
        IReadOnlyList<TransactionLine> products,
        int orderTotalPence,
        int amountInsertedPence,
        int changeReturnedPence,
        string paymentStatus)
    {
        TransactionId = transactionId;
        CompletedAt = completedAt;
        OrderNumber = orderNumber;
        Products = products;
        OrderTotalPence = orderTotalPence;
        AmountInsertedPence = amountInsertedPence;
        ChangeReturnedPence = changeReturnedPence;
        PaymentStatus = paymentStatus;
    }

    public string TransactionId { get; }
    public DateTimeOffset CompletedAt { get; }
    public int OrderNumber { get; }
    public IReadOnlyList<TransactionLine> Products { get; }
    public int OrderTotalPence { get; }
    public int AmountInsertedPence { get; }
    public int ChangeReturnedPence { get; }
    public string PaymentStatus { get; }
}

namespace CS2S564VendingMachine.Core;

public sealed class PaymentManager
{
    public bool IsActive { get; private set; }
    public int OrderTotalPence { get; private set; }
    public int InsertedPence { get; private set; }

    public int OutstandingPence => Math.Max(0, OrderTotalPence - InsertedPence);
    public int ChangeDuePence => Math.Max(0, InsertedPence - OrderTotalPence);
    public bool IsComplete => IsActive && OrderTotalPence > 0 && InsertedPence >= OrderTotalPence;

    public void Begin(int orderTotalPence)
    {
        if (orderTotalPence <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(orderTotalPence), "A payment session requires a positive order total.");
        }

        OrderTotalPence = orderTotalPence;
        InsertedPence = 0;
        IsActive = true;
    }

    public bool TryInsertCoin(int denominationPence)
    {
        if (!IsActive || IsComplete)
        {
            return false;
        }

        if (!CoinDenominations.IsAccepted(denominationPence))
        {
            return false;
        }

        InsertedPence += denominationPence;
        return true;
    }

    public void Reset()
    {
        IsActive = false;
        OrderTotalPence = 0;
        InsertedPence = 0;
    }
}

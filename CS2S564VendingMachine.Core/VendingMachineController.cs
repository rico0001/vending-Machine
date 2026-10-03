namespace CS2S564VendingMachine.Core;

public sealed class CoinInsertedEventArgs : EventArgs
{
    public CoinInsertedEventArgs(int denominationPence, bool paymentComplete)
    {
        DenominationPence = denominationPence;
        PaymentComplete = paymentComplete;
    }

    public int DenominationPence { get; }
    public bool PaymentComplete { get; }
}

/// <summary>
/// Coordinates basket, payment and machine state. The WinForms layer only calls these methods
/// and then refreshes controls from the public properties.
/// </summary>
public sealed class VendingMachineController
{
    public const int StartingOrderNumber = 1001;

    private readonly TransactionHistory _history;
    private MachineState _state = MachineState.Idle;

    public VendingMachineController(TransactionHistory history)
    {
        _history = history ?? throw new ArgumentNullException(nameof(history));
        Basket = new ShoppingBasket();
        Payment = new PaymentManager();
        OrderNumber = StartingOrderNumber;
    }

    public event EventHandler? Changed;
    public event EventHandler<string>? StatusMessage;
    public event EventHandler<CoinInsertedEventArgs>? CoinAccepted;
    public event EventHandler<Transaction>? SaleCompleted;

    public ShoppingBasket Basket { get; }
    public PaymentManager Payment { get; }
    public MachineState State => _state;
    public int OrderNumber { get; private set; }
    public string? LastError { get; private set; }
    public Transaction? LastCompletedTransaction { get; private set; }

    public bool CanSelectProducts => _state is MachineState.Idle or MachineState.SelectingProducts;
    public bool CanEditBasket => CanSelectProducts && !Basket.IsEmpty;
    public bool CanCheckout => CanSelectProducts && !Basket.IsEmpty;
    public bool CanInsertCoins => _state == MachineState.AwaitingPayment && Payment.IsActive && !Payment.IsComplete;

    public bool TryAddProduct(Product product)
    {
        LastError = null;
        if (!CanSelectProducts)
        {
            return Fail("Products cannot be added while the machine is in checkout or payment mode.");
        }

        Basket.Add(product);
        SetState(MachineState.SelectingProducts);
        RaiseMessage($"Product added: {product.Name}");
        Notify();
        return true;
    }

    public bool TryRemoveOne(string productId)
    {
        LastError = null;
        if (!CanSelectProducts)
        {
            return Fail("The basket cannot be edited during payment.");
        }

        var line = Basket.Items.FirstOrDefault(item => item.Product.Id == productId);
        if (line is null)
        {
            return Fail("Select a product in the basket to remove.");
        }

        var name = line.Product.Name;
        Basket.RemoveOne(productId);
        RaiseMessage($"Product removed: {name}");

        if (Basket.IsEmpty)
        {
            SetState(MachineState.Idle);
        }

        Notify();
        return true;
    }

    public bool TryCheckout()
    {
        LastError = null;
        if (Basket.IsEmpty)
        {
            return Fail("Cannot checkout with an empty basket. Add a product first.");
        }

        if (!CanCheckout)
        {
            return Fail("Checkout is only available while selecting products.");
        }

        Payment.Begin(Basket.TotalPence);
        SetState(MachineState.AwaitingPayment);
        RaiseMessage("Checkout started");
        RaiseMessage("Please insert coins");
        Notify();
        return true;
    }

    public bool TryInsertCoin(int denominationPence)
    {
        LastError = null;
        if (!CanInsertCoins)
        {
            return Fail("Coins are only accepted while the machine is awaiting payment.");
        }

        if (!CoinDenominations.IsAccepted(denominationPence))
        {
            return Fail("That coin is not accepted by this machine.");
        }

        if (!Payment.TryInsertCoin(denominationPence))
        {
            return Fail("The coin could not be accepted.");
        }

        RaiseMessage($"Coin accepted: {MoneyFormatter.FormatCoin(denominationPence)}");
        CoinAccepted?.Invoke(this, new CoinInsertedEventArgs(denominationPence, Payment.IsComplete));

        if (Payment.IsComplete)
        {
            CompleteSale();
        }
        else
        {
            RaiseMessage($"Insufficient payment — {MoneyFormatter.FormatPence(Payment.OutstandingPence)} remaining");
        }

        Notify();
        return true;
    }

    public bool ConfirmCancel()
    {
        LastError = null;
        if (_state is MachineState.Idle && Basket.IsEmpty)
        {
            return Fail("There is no active purchase to cancel.");
        }

        SetState(MachineState.Cancelled);
        RaiseMessage("Purchase cancelled");
        ResetSession(advanceOrderNumber: true);
        Notify();
        return true;
    }

    public void ResetAfterCompletion()
    {
        if (_state != MachineState.PaymentComplete)
        {
            return;
        }

        ResetSession(advanceOrderNumber: true);
        Notify();
    }

    private void CompleteSale()
    {
        SetState(MachineState.PaymentComplete);
        RaiseMessage("Payment successful");

        if (Payment.ChangeDuePence > 0)
        {
            RaiseMessage($"Change returned: {MoneyFormatter.FormatPence(Payment.ChangeDuePence)}");
        }
        else
        {
            RaiseMessage("Exact payment — no change due");
        }

        var transaction = new Transaction(
            transactionId: $"TX-{OrderNumber}-{DateTimeOffset.Now:yyyyMMddHHmmss}",
            completedAt: DateTimeOffset.Now,
            orderNumber: OrderNumber,
            products: Basket.ToTransactionLines(),
            orderTotalPence: Payment.OrderTotalPence,
            amountInsertedPence: Payment.InsertedPence,
            changeReturnedPence: Payment.ChangeDuePence,
            paymentStatus: "Completed");

        LastCompletedTransaction = transaction;
        try
        {
            _history.RecordCompletedSale(transaction);
        }
        catch (Exception)
        {
            RaiseMessage("Payment successful, but the transaction could not be saved to history.");
        }

        SaleCompleted?.Invoke(this, transaction);
    }

    private void ResetSession(bool advanceOrderNumber)
    {
        Basket.Clear();
        Payment.Reset();
        if (advanceOrderNumber)
        {
            OrderNumber++;
        }

        LastCompletedTransaction = null;
        SetState(MachineState.Idle);
        RaiseMessage("Welcome to the vending machine");
    }

    private void SetState(MachineState next)
    {
        _state = next;
    }

    private bool Fail(string message)
    {
        LastError = message;
        RaiseMessage(message);
        return false;
    }

    private void RaiseMessage(string message) => StatusMessage?.Invoke(this, message);

    private void Notify() => Changed?.Invoke(this, EventArgs.Empty);
}

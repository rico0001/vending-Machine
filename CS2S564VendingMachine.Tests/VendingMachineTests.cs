using CS2S564VendingMachine.Core;
using Xunit;

namespace CS2S564VendingMachine.Tests;

public class ShoppingBasketTests
{
    private static readonly Product Cola = new("cola", "Coca-Cola", 120, "coca-cola.jpg");
    private static readonly Product Water = new("water", "Bottled Water", 90, "water.jpg");

    [Fact]
    public void Add_GroupsIdenticalProductsAndTotalsInPence()
    {
        var basket = new ShoppingBasket();
        basket.Add(Cola);
        basket.Add(Cola);
        basket.Add(Water);

        Assert.Equal(2, basket.Items.Count);
        Assert.Equal(2, basket.Items[0].Quantity);
        Assert.Equal(240, basket.Items[0].SubtotalPence);
        Assert.Equal(330, basket.TotalPence);
    }

    [Fact]
    public void RemoveOne_DecreasesQuantityThenRemovesLine()
    {
        var basket = new ShoppingBasket();
        basket.Add(Cola);
        basket.Add(Cola);

        Assert.True(basket.RemoveOne("cola"));
        Assert.Single(basket.Items);
        Assert.Equal(1, basket.Items[0].Quantity);

        Assert.True(basket.RemoveOne("cola"));
        Assert.True(basket.IsEmpty);
    }
}

public class PaymentManagerTests
{
    [Fact]
    public void ExactPayment_CompletesWithNoChange()
    {
        var payment = new PaymentManager();
        payment.Begin(235);
        Assert.True(payment.TryInsertCoin(200));
        Assert.False(payment.IsComplete);
        Assert.Equal(35, payment.OutstandingPence);
        Assert.True(payment.TryInsertCoin(20));
        Assert.True(payment.TryInsertCoin(10));
        Assert.True(payment.TryInsertCoin(5));
        Assert.True(payment.IsComplete);
        Assert.Equal(0, payment.ChangeDuePence);
    }

    [Fact]
    public void Overpayment_CalculatesChangeAndRejectsFurtherCoins()
    {
        var payment = new PaymentManager();
        payment.Begin(235);
        Assert.True(payment.TryInsertCoin(200));
        Assert.True(payment.TryInsertCoin(50));
        Assert.True(payment.IsComplete);
        Assert.Equal(15, payment.ChangeDuePence);
        Assert.False(payment.TryInsertCoin(5));
    }

    [Fact]
    public void InvalidDenomination_IsRejected()
    {
        var payment = new PaymentManager();
        payment.Begin(100);
        Assert.False(payment.TryInsertCoin(1));
        Assert.Equal(0, payment.InsertedPence);
    }
}

public class VendingMachineControllerTests
{
    private static VendingMachineController CreateMachine(string? historyPath = null)
    {
        historyPath ??= Path.Combine(Path.GetTempPath(), $"vm-history-{Guid.NewGuid():N}.json");
        return new VendingMachineController(new TransactionHistory(historyPath));
    }

    [Fact]
    public void Checkout_EmptyBasket_Fails()
    {
        var machine = CreateMachine();
        Assert.False(machine.TryCheckout());
        Assert.Equal(MachineState.Idle, machine.State);
    }

    [Fact]
    public void AddProduct_ThenCheckout_EnablesPaymentAndLocksSelection()
    {
        var machine = CreateMachine();
        var cola = ProductCatalog.CreateDefault()[0];
        Assert.True(machine.TryAddProduct(cola));
        Assert.Equal(MachineState.SelectingProducts, machine.State);
        Assert.True(machine.TryCheckout());
        Assert.Equal(MachineState.AwaitingPayment, machine.State);
        Assert.False(machine.CanSelectProducts);
        Assert.True(machine.CanInsertCoins);
        Assert.False(machine.TryAddProduct(cola));
    }

    [Fact]
    public void PaymentWorkflow_OverpayThenReset_StartsNewOrder()
    {
        var machine = CreateMachine();
        var cola = ProductCatalog.CreateDefault()[0];
        machine.TryAddProduct(cola);
        machine.TryCheckout();
        Assert.True(machine.TryInsertCoin(200));
        Assert.Equal(MachineState.PaymentComplete, machine.State);
        Assert.Equal(80, machine.Payment.ChangeDuePence);
        Assert.False(machine.CanInsertCoins);

        var previousOrder = machine.OrderNumber;
        machine.ResetAfterCompletion();
        Assert.Equal(MachineState.Idle, machine.State);
        Assert.True(machine.Basket.IsEmpty);
        Assert.Equal(previousOrder + 1, machine.OrderNumber);
    }

    [Fact]
    public void Cancel_ClearsBasket_AndDecliningIsUiOnly()
    {
        var machine = CreateMachine();
        machine.TryAddProduct(ProductCatalog.CreateDefault()[0]);
        Assert.True(machine.ConfirmCancel());
        Assert.Equal(MachineState.Idle, machine.State);
        Assert.True(machine.Basket.IsEmpty);
    }

    [Fact]
    public void InsertCoin_WhenNotAwaitingPayment_Fails()
    {
        var machine = CreateMachine();
        Assert.False(machine.TryInsertCoin(100));
    }
}

public class TransactionHistoryTests
{
    [Fact]
    public void CompletedSale_IsSavedAndReloaded()
    {
        var path = Path.Combine(Path.GetTempPath(), $"vm-history-{Guid.NewGuid():N}.json");
        var history = new TransactionHistory(path);
        var transaction = new Transaction(
            "TX-1",
            DateTimeOffset.Parse("2026-10-03T12:00:00+00:00"),
            1001,
            new[] { new TransactionLine("cola", "Coca-Cola", 2, 120, 240) },
            240,
            250,
            10,
            "Completed");

        history.RecordCompletedSale(transaction);
        var reloaded = new TransactionHistory(path);

        Assert.Single(reloaded.Transactions);
        Assert.Equal("TX-1", reloaded.Transactions[0].TransactionId);
        Assert.Equal(240, reloaded.Transactions[0].OrderTotalPence);
        Assert.Equal(2, reloaded.Transactions[0].Products[0].Quantity);
    }

    [Fact]
    public void IncompleteStatus_IsNotRecorded()
    {
        var path = Path.Combine(Path.GetTempPath(), $"vm-history-{Guid.NewGuid():N}.json");
        var history = new TransactionHistory(path);
        var cancelled = new Transaction("TX-2", DateTimeOffset.Now, 1002, Array.Empty<TransactionLine>(), 0, 0, 0, "Cancelled");
        Assert.Throws<InvalidOperationException>(() => history.RecordCompletedSale(cancelled));
        Assert.Empty(history.Transactions);
    }

    [Fact]
    public void Save_ToInvalidDirectory_Throws()
    {
        var invalidPath = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}", "nested", "transactions.json");
        // Creating the nested directory is handled by Save; simulate a file path that cannot be written
        // by pointing at an existing file used as a directory name.
        var blocker = Path.Combine(Path.GetTempPath(), $"blocker-{Guid.NewGuid():N}.txt");
        File.WriteAllText(blocker, "not-a-folder");
        var history = new TransactionHistory(Path.Combine(blocker, "transactions.json"));
        var transaction = new Transaction("TX-3", DateTimeOffset.Now, 1003, Array.Empty<TransactionLine>(), 100, 100, 0, "Completed");
        Assert.ThrowsAny<Exception>(() => history.RecordCompletedSale(transaction));
    }
}

public class MoneyFormatterTests
{
    [Theory]
    [InlineData(0, "£0.00")]
    [InlineData(5, "£0.05")]
    [InlineData(235, "£2.35")]
    [InlineData(250, "£2.50")]
    public void FormatPence_UsesTwoDecimalPlaces(int pence, string expected)
    {
        Assert.Equal(expected, MoneyFormatter.FormatPence(pence));
    }
}

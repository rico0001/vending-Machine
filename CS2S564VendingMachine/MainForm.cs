using CS2S564VendingMachine.Core;
using CS2S564VendingMachine.UI;

namespace CS2S564VendingMachine;

public partial class MainForm : Form
{
    private readonly VendingMachineController _machine;
    private readonly TransactionHistory _history;
    private readonly List<ProductCard> _productCards = new();
    private readonly List<CoinButton> _coinButtons = new();

    public MainForm()
    {
        InitializeComponent();

        _history = new TransactionHistory(ResolveHistoryPath());
        _machine = new VendingMachineController(_history);
        _machine.Changed += (_, _) => RunOnUi(RefreshUiFromState);
        _machine.StatusMessage += (_, message) => RunOnUi(() => AppendActivity(message));
        _machine.SaleCompleted += (_, transaction) => RunOnUi(() => OnSaleCompleted(transaction));

        AllowDrop = true;
        DragEnter += MainForm_DragEnter;
        DragDrop += MainForm_DragDrop;

        LoadProducts();
        WireEvents();
        RefreshUiFromState();
        AppendActivity("Welcome to the vending machine");
    }

    private void LoadProducts()
    {
        var catalogPath = Path.Combine(AppContext.BaseDirectory, "Data", "products.json");
        var products = ProductCatalog.LoadFromJsonOrDefault(catalogPath);

        const int columns = 3;
        const int cardWidth = 210;
        const int cardHeight = 248;
        const int gap = 12;
        var index = 0;
        foreach (var product in products)
        {
            var card = new ProductCard(product);
            var row = index / columns;
            var col = index % columns;
            card.Location = new Point(14 + col * (cardWidth + gap), 40 + row * (cardHeight + gap));
            card.AddRequested += ProductCard_AddRequested;
            _pnlProducts.Controls.Add(card);
            _productCards.Add(card);
            index++;
        }
    }

    private void WireEvents()
    {
        _btnCheckout.Click += BtnCheckout_Click;
        _btnRemove.Click += BtnRemove_Click;
        _btnCancel.Click += BtnCancel_Click;
        _btnHistory.Click += BtnHistory_Click;
        _lstBasket.DrawItem += LstBasket_DrawItem;
        _coinSlot.CoinDropped += CoinSlot_CoinDropped;
        _coinSlot.InvalidDrop += CoinSlot_InvalidDrop;
    }

    private void ProductCard_AddRequested(object? sender, Product product)
    {
        if (_machine.TryAddProduct(product) && sender is ProductCard card)
        {
            card.FlashSelected();
            FlashBasket();
        }
        else if (!string.IsNullOrWhiteSpace(_machine.LastError))
        {
            ShowNotice(_machine.LastError);
        }

        RefreshUiFromState();
    }

    private void BtnCheckout_Click(object? sender, EventArgs e)
    {
        if (!_machine.TryCheckout())
        {
            ShowNotice(_machine.LastError ?? "Checkout is not available.");
        }

        RefreshUiFromState();
    }

    private void BtnRemove_Click(object? sender, EventArgs e)
    {
        if (_lstBasket.SelectedItem is not OrderItem item)
        {
            ShowNotice("Select a product in the basket to remove.");
            return;
        }

        _machine.TryRemoveOne(item.Product.Id);
        RefreshUiFromState();
    }

    private void BtnCancel_Click(object? sender, EventArgs e)
    {
        if (_machine.State == MachineState.Idle && _machine.Basket.IsEmpty)
        {
            ShowNotice("There is no active purchase to cancel.");
            return;
        }

        var result = MessageBox.Show(
            this,
            "Cancel this purchase and return the machine to its start screen?",
            "Cancel purchase",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result != DialogResult.Yes)
        {
            AppendActivity("Cancellation declined — current transaction preserved");
            return;
        }

        _machine.ConfirmCancel();
        FlashCancel();
        RefreshUiFromState();
    }

    private void CoinSlot_CoinDropped(object? sender, int denominationPence)
    {
        if (!_machine.TryInsertCoin(denominationPence))
        {
            _coinSlot.FlashReject();
            ShowNotice(_machine.LastError ?? "The coin was not accepted.");
        }

        RefreshUiFromState();
    }

    private void CoinSlot_InvalidDrop(object? sender, string message)
    {
        AppendActivity(message);
        ShowNotice(message);
    }

    private void MainForm_DragEnter(object? sender, DragEventArgs e)
    {
        e.Effect = e.Data?.GetDataPresent(DataFormats.Text) == true
            ? DragDropEffects.Copy
            : DragDropEffects.None;
    }

    private void MainForm_DragDrop(object? sender, DragEventArgs e)
    {
        _coinSlot.FlashReject();
        const string message = "Drop coins into the coin slot.";
        AppendActivity(message);
        ShowNotice(message);
    }

    private void LstBasket_DrawItem(object? sender, DrawItemEventArgs e)
    {
        e.DrawBackground();
        if (e.Index < 0 || e.Index >= _lstBasket.Items.Count)
        {
            return;
        }

        var item = (OrderItem)_lstBasket.Items[e.Index];
        var selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
        using var background = new SolidBrush(selected ? Color.FromArgb(0, 90, 82) : Theme.PanelAlt);
        e.Graphics.FillRectangle(background, e.Bounds);
        TextRenderer.DrawText(
            e.Graphics,
            item.ToBasketLine(),
            Theme.BodyFont,
            e.Bounds,
            Theme.TextPrimary,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private void OnSaleCompleted(Transaction transaction)
    {
        RefreshUiFromState();
        var changeText = transaction.ChangeReturnedPence == 0
            ? "Exact payment — no change due."
            : $"Change returned: {MoneyFormatter.FormatPence(transaction.ChangeReturnedPence)}";

        MessageBox.Show(
            this,
            $"Payment successful.{Environment.NewLine}{Environment.NewLine}" +
            $"Order total: {MoneyFormatter.FormatPence(transaction.OrderTotalPence)}{Environment.NewLine}" +
            $"Amount paid: {MoneyFormatter.FormatPence(transaction.AmountInsertedPence)}{Environment.NewLine}" +
            changeText,
            "Transaction complete",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);

        _machine.ResetAfterCompletion();
        RefreshUiFromState();
        AppendActivity("Machine reset — ready for a new order");
    }

    private void BtnHistory_Click(object? sender, EventArgs e)
    {
        try
        {
            var records = _history.Reload();
            if (records.Count == 0)
            {
                ShowNotice("No completed transactions have been saved yet.");
                return;
            }

            var lines = records
                .Reverse()
                .Take(12)
                .Select(tx =>
                    $"{tx.CompletedAt:g}  {tx.TransactionId}  {MoneyFormatter.FormatPence(tx.OrderTotalPence)}  paid {MoneyFormatter.FormatPence(tx.AmountInsertedPence)}  change {MoneyFormatter.FormatPence(tx.ChangeReturnedPence)}  {tx.PaymentStatus}");

            MessageBox.Show(
                this,
                string.Join(Environment.NewLine, lines),
                "Completed transactions",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception)
        {
            ShowNotice("Transaction history could not be read.");
        }
    }

    private void RefreshUiFromState()
    {
        Text = $"Vending Machine | Student No: {AppSettings.StudentNumber}";
        _lblOrderNumber.Text = $"ORDER #{_machine.OrderNumber}";
        _lblStatus.Text = StatusText(_machine.State);
        _lblStatusDot.ForeColor = StatusColor(_machine.State);
        _lblResetHint.Visible = _machine.State == MachineState.PaymentComplete;

        foreach (var card in _productCards)
        {
            card.SetSelectionEnabled(_machine.CanSelectProducts);
        }

        _lstBasket.BeginUpdate();
        _lstBasket.Items.Clear();
        foreach (var item in _machine.Basket.Items)
        {
            _lstBasket.Items.Add(item);
        }

        _lstBasket.EndUpdate();
        _lstBasket.Enabled = _machine.CanSelectProducts;
        _lblEmptyBasket.Visible = _machine.Basket.IsEmpty;
        _lblBasketTotal.Text = MoneyFormatter.FormatPence(_machine.Basket.TotalPence);
        _btnRemove.Enabled = _machine.CanEditBasket;
        _btnCheckout.Enabled = _machine.CanCheckout;
        _btnCancel.Enabled = !(_machine.State == MachineState.Idle && _machine.Basket.IsEmpty) &&
                             _machine.State != MachineState.PaymentComplete;

        foreach (var coin in _coinButtons)
        {
            coin.Enabled = _machine.CanInsertCoins;
        }

        _coinSlot.AcceptingCoins = _machine.CanInsertCoins;
        _coinSlot.Invalidate();

        var total = _machine.State is MachineState.AwaitingPayment or MachineState.PaymentComplete
            ? _machine.Payment.OrderTotalPence
            : _machine.Basket.TotalPence;
        var inserted = _machine.Payment.InsertedPence;
        var outstanding = _machine.State == MachineState.AwaitingPayment ? _machine.Payment.OutstandingPence : 0;
        var change = _machine.State == MachineState.PaymentComplete ? _machine.Payment.ChangeDuePence : 0;

        _lblPayTotal.Text = MoneyFormatter.FormatPence(total);
        _lblPayInserted.Text = MoneyFormatter.FormatPence(inserted);
        _lblPayOutstanding.Text = MoneyFormatter.FormatPence(outstanding);
        _lblPayChange.Text = MoneyFormatter.FormatPence(change);
        _lblPayStatus.Text = PaymentStatusText();
        _lblPayStatus.ForeColor = _machine.State == MachineState.PaymentComplete ? Theme.StatusSuccess
            : _machine.State == MachineState.AwaitingPayment ? Theme.StatusPayment
            : Theme.TextMuted;
    }

    private string PaymentStatusText()
    {
        return _machine.State switch
        {
            MachineState.AwaitingPayment when _machine.Payment.OutstandingPence > 0 =>
                $"Awaiting {MoneyFormatter.FormatPence(_machine.Payment.OutstandingPence)}",
            MachineState.PaymentComplete when _machine.Payment.ChangeDuePence > 0 =>
                "Paid — dispensing change",
            MachineState.PaymentComplete => "Paid — exact amount",
            MachineState.Cancelled => "Cancelled",
            _ => "Waiting for checkout"
        };
    }

    private static string StatusText(MachineState state) => state switch
    {
        MachineState.Idle => "READY",
        MachineState.SelectingProducts => "SELECTING",
        MachineState.AwaitingPayment => "AWAITING PAYMENT",
        MachineState.PaymentComplete => "PAYMENT COMPLETE",
        MachineState.Cancelled => "CANCELLED",
        _ => state.ToString().ToUpperInvariant()
    };

    private static Color StatusColor(MachineState state) => state switch
    {
        MachineState.Idle => Theme.StatusIdle,
        MachineState.SelectingProducts => Theme.StatusSelecting,
        MachineState.AwaitingPayment => Theme.StatusPayment,
        MachineState.PaymentComplete => Theme.StatusSuccess,
        MachineState.Cancelled => Theme.StatusCancelled,
        _ => Theme.StatusIdle
    };

    private void AppendActivity(string message)
    {
        var line = $"{DateTime.Now:HH:mm:ss}  {message}";
        _lstActivity.Items.Insert(0, line);
        while (_lstActivity.Items.Count > 40)
        {
            _lstActivity.Items.RemoveAt(_lstActivity.Items.Count - 1);
        }
    }

    private void ShowNotice(string message)
    {
        MessageBox.Show(this, message, AppSettings.ApplicationName, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void FlashBasket()
    {
        var original = _pnlBasket.BackColor;
        _pnlBasket.BackColor = Color.FromArgb(28, 58, 56);
        var timer = new System.Windows.Forms.Timer { Interval = 180 };
        timer.Tick += (_, _) =>
        {
            _pnlBasket.BackColor = original;
            timer.Stop();
            timer.Dispose();
        };
        timer.Start();
    }

    private void FlashCancel()
    {
        var original = BackColor;
        BackColor = Color.FromArgb(40, 18, 22);
        var timer = new System.Windows.Forms.Timer { Interval = 220 };
        timer.Tick += (_, _) =>
        {
            BackColor = original;
            timer.Stop();
            timer.Dispose();
        };
        timer.Start();
    }

    private static string ResolveHistoryPath()
    {
        var local = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CS2S564VendingMachine",
            "transactions.json");
        return local;
    }

    private void RunOnUi(Action action)
    {
        if (IsDisposed)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(action);
            return;
        }

        action();
    }
}

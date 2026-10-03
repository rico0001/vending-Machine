using CS2S564VendingMachine.Core;
using CS2S564VendingMachine.UI;

namespace CS2S564VendingMachine;

partial class MainForm
{
    private Panel _pnlHeader = null!;
    private Label _lblTitle = null!;
    private Label _lblSubtitle = null!;
    private Label _lblStatusDot = null!;
    private Label _lblStatus = null!;
    private Label _lblOrderNumber = null!;
    private Label _lblResetHint = null!;
    private Panel _pnlProducts = null!;
    private Panel _pnlBasket = null!;
    private ListBox _lstBasket = null!;
    private Label _lblEmptyBasket = null!;
    private Label _lblBasketTotalCaption = null!;
    private Label _lblBasketTotal = null!;
    private RoundedButton _btnRemove = null!;
    private RoundedButton _btnCheckout = null!;
    private Panel _pnlCoins = null!;
    private CoinSlotControl _coinSlot = null!;
    private Panel _pnlPayment = null!;
    private Label _lblPayTotal = null!;
    private Label _lblPayInserted = null!;
    private Label _lblPayOutstanding = null!;
    private Label _lblPayChange = null!;
    private Label _lblPayStatus = null!;
    private RoundedButton _btnCancel = null!;
    private RoundedButton _btnHistory = null!;
    private Panel _pnlActivity = null!;
    private ListBox _lstActivity = null!;

    private void InitializeComponent()
    {
        SuspendLayout();

        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1280, 860);
        MinimumSize = new Size(1280, 860);
        MaximumSize = new Size(1280, 860);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Theme.WindowBackground;
        ForeColor = Theme.TextPrimary;
        Font = Theme.BodyFont;
        DoubleBuffered = true;
        Text = $"Vending Machine | Student No: {AppSettings.StudentNumber}";

        _pnlHeader = CreatePanel(new Point(16, 12), new Size(1248, 78), Theme.PanelBackground);
        _lblTitle = CreateLabel(AppSettings.ApplicationName, new Point(20, 12), new Size(640, 34), Theme.TitleFont, Theme.TextPrimary);
        _lblSubtitle = CreateLabel(AppSettings.Subtitle, new Point(22, 46), new Size(420, 22), Theme.SubtitleFont, Theme.TextMuted);
        _lblStatusDot = CreateLabel("●", new Point(860, 18), new Size(24, 24), Theme.HeadingFont, Theme.StatusIdle);
        _lblStatus = CreateLabel("READY", new Point(884, 18), new Size(210, 24), Theme.HeadingFont, Theme.TextPrimary);
        _lblOrderNumber = CreateLabel("ORDER #1001", new Point(860, 44), new Size(220, 22), Theme.MonoFont, Theme.Gold);
        _lblResetHint = CreateLabel("New order starts after the success message", new Point(480, 48), new Size(360, 22), Theme.BodyFont, Theme.StatusSuccess);
        _lblResetHint.Visible = false;
        _btnHistory = new RoundedButton
        {
            Text = "HISTORY",
            Location = new Point(1118, 14),
            Size = new Size(110, 28),
            BackColor = Color.FromArgb(62, 74, 96),
            HoverBackColor = Color.FromArgb(82, 96, 122),
            CornerRadius = 8
        };
        _pnlHeader.Controls.AddRange(new Control[] { _lblTitle, _lblSubtitle, _lblStatusDot, _lblStatus, _lblOrderNumber, _lblResetHint, _btnHistory });

        _pnlProducts = CreatePanel(new Point(16, 102), new Size(790, 548), Theme.PanelBackground);
        var productsHeading = CreateLabel("PRODUCTS", new Point(16, 12), new Size(200, 22), Theme.HeadingFont, Theme.Accent);
        _pnlProducts.Controls.Add(productsHeading);

        _pnlBasket = CreatePanel(new Point(822, 102), new Size(442, 548), Theme.PanelBackground);
        var basketHeading = CreateLabel("SHOPPING BASKET", new Point(16, 12), new Size(240, 22), Theme.HeadingFont, Theme.Accent);
        _lstBasket = new ListBox
        {
            Location = new Point(16, 42),
            Size = new Size(410, 360),
            DrawMode = DrawMode.OwnerDrawFixed,
            ItemHeight = 36,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Theme.PanelAlt,
            ForeColor = Theme.TextPrimary,
            Font = Theme.BodyFont,
            IntegralHeight = false
        };
        _lblEmptyBasket = CreateLabel("Your basket is empty.\nAdd a product to begin.", new Point(40, 170), new Size(360, 60), Theme.SubtitleFont, Theme.TextMuted);
        _lblEmptyBasket.TextAlign = ContentAlignment.MiddleCenter;
        _lblBasketTotalCaption = CreateLabel("ORDER TOTAL", new Point(16, 414), new Size(140, 20), Theme.BodyFont, Theme.TextMuted);
        _lblBasketTotal = CreateLabel("£0.00", new Point(250, 408), new Size(176, 32), Theme.TitleFont, Theme.TextPrimary);
        _lblBasketTotal.TextAlign = ContentAlignment.MiddleRight;
        _btnRemove = new RoundedButton
        {
            Text = "REMOVE SELECTED",
            Location = new Point(16, 456),
            Size = new Size(196, 40),
            BackColor = Color.FromArgb(62, 74, 96),
            HoverBackColor = Color.FromArgb(82, 96, 122)
        };
        _btnCheckout = new RoundedButton
        {
            Text = "CHECKOUT",
            Location = new Point(226, 456),
            Size = new Size(200, 40),
            BackColor = Theme.Accent,
            HoverBackColor = Theme.AccentDark,
            ForeColor = Color.FromArgb(8, 18, 20)
        };
        var checkoutHint = CreateLabel("Checkout locks products and opens the coin slot.", new Point(16, 504), new Size(410, 28), Theme.BodyFont, Theme.TextMuted);
        _pnlBasket.Controls.AddRange(new Control[]
        {
            basketHeading, _lstBasket, _lblEmptyBasket, _lblBasketTotalCaption, _lblBasketTotal, _btnRemove, _btnCheckout, checkoutHint
        });

        _pnlCoins = CreatePanel(new Point(16, 662), new Size(790, 112), Theme.PanelBackground);
        var coinsHeading = CreateLabel("INSERT COINS  —  drag a coin onto the slot", new Point(16, 8), new Size(430, 20), Theme.HeadingFont, Theme.Accent);
        _pnlCoins.Controls.Add(coinsHeading);

        var x = 16;
        foreach (var pence in CoinDenominations.AllPence)
        {
            var coin = new CoinButton(pence)
            {
                Location = new Point(x, 32),
                Enabled = false
            };
            _pnlCoins.Controls.Add(coin);
            _coinButtons.Add(coin);
            x += 84;
        }

        _coinSlot = new CoinSlotControl
        {
            Location = new Point(548, 8),
            Size = new Size(226, 96)
        };
        _pnlCoins.Controls.Add(_coinSlot);

        _pnlPayment = CreatePanel(new Point(822, 662), new Size(442, 112), Theme.PanelBackground);
        var payHeading = CreateLabel("PAYMENT SUMMARY", new Point(16, 8), new Size(200, 20), Theme.HeadingFont, Theme.Accent);
        _lblPayTotal = AddSummaryRow(_pnlPayment, "Due", "£0.00", 16, 32, out _);
        _lblPayInserted = AddSummaryRow(_pnlPayment, "Inserted", "£0.00", 230, 32, out _);
        _lblPayOutstanding = AddSummaryRow(_pnlPayment, "Outstanding", "£0.00", 16, 54, out _);
        _lblPayChange = AddSummaryRow(_pnlPayment, "Change", "£0.00", 230, 54, out _);
        _lblPayStatus = CreateLabel("Waiting for checkout", new Point(16, 78), new Size(240, 22), Theme.HeadingFont, Theme.TextMuted);
        _btnCancel = new RoundedButton
        {
            Text = "CANCEL PURCHASE",
            Location = new Point(250, 74),
            Size = new Size(176, 30),
            BackColor = Theme.Danger,
            HoverBackColor = Theme.DangerDark,
            CornerRadius = 8
        };
        _pnlPayment.Controls.AddRange(new Control[] { payHeading, _lblPayStatus, _btnCancel });

        _pnlActivity = CreatePanel(new Point(16, 782), new Size(1248, 64), Theme.PanelBackground);
        var activityHeading = CreateLabel("ACTIVITY", new Point(16, 6), new Size(120, 18), Theme.HeadingFont, Theme.Accent);
        _lstActivity = new ListBox
        {
            Location = new Point(140, 8),
            Size = new Size(1092, 48),
            BorderStyle = BorderStyle.None,
            BackColor = Theme.PanelBackground,
            ForeColor = Theme.TextMuted,
            Font = Theme.BodyFont,
            IntegralHeight = false
        };
        _pnlActivity.Controls.AddRange(new Control[] { activityHeading, _lstActivity });

        Controls.AddRange(new Control[]
        {
            _pnlHeader, _pnlProducts, _pnlBasket, _pnlCoins, _pnlPayment, _pnlActivity
        });

        ResumeLayout(false);
    }

    private static Panel CreatePanel(Point location, Size size, Color backColor)
    {
        return new Panel
        {
            Location = location,
            Size = size,
            BackColor = backColor
        };
    }

    private static Label CreateLabel(string text, Point location, Size size, Font font, Color color)
    {
        return new Label
        {
            Text = text,
            Location = location,
            Size = size,
            Font = font,
            ForeColor = color,
            BackColor = Color.Transparent,
            AutoSize = false
        };
    }

    private static Label AddSummaryRow(Control parent, string caption, string value, int x, int y, out Label captionLabel)
    {
        captionLabel = CreateLabel(caption.ToUpperInvariant(), new Point(x, y), new Size(100, 18), Theme.BodyFont, Theme.TextMuted);
        var valueLabel = CreateLabel(value, new Point(x + 100, y), new Size(90, 18), Theme.MonoFont, Theme.TextPrimary);
        parent.Controls.Add(captionLabel);
        parent.Controls.Add(valueLabel);
        return valueLabel;
    }
}

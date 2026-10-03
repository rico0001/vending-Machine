using CS2S564VendingMachine.Core;

namespace CS2S564VendingMachine.UI;

internal sealed class ProductCard : Panel
{
    private readonly PictureBox _image;
    private readonly Label _name;
    private readonly Label _price;
    private readonly RoundedButton _add;
    private readonly Product _product;
    private Color _baseColor;
    private System.Windows.Forms.Timer? _flashTimer;
    private int _flashTicks;

    public event EventHandler<Product>? AddRequested;

    public Product Product => _product;

    public ProductCard(Product product)
    {
        _product = product;
        _baseColor = Theme.CardBackground;
        DoubleBuffered = true;
        BackColor = _baseColor;
        Size = new Size(210, 248);
        Padding = new Padding(10);
        Cursor = Cursors.Hand;

        _image = new PictureBox
        {
            Size = new Size(188, 128),
            Location = new Point(11, 12),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.FromArgb(16, 20, 28)
        };

        _name = new Label
        {
            AutoSize = false,
            Location = new Point(10, 146),
            Size = new Size(190, 22),
            Font = Theme.HeadingFont,
            ForeColor = Theme.TextPrimary,
            Text = product.Name,
            TextAlign = ContentAlignment.MiddleLeft
        };

        _price = new Label
        {
            AutoSize = false,
            Location = new Point(10, 168),
            Size = new Size(190, 20),
            Font = Theme.PriceFont,
            ForeColor = Theme.Accent,
            Text = MoneyFormatter.FormatPence(product.UnitPricePence),
            TextAlign = ContentAlignment.MiddleLeft
        };

        _add = new RoundedButton
        {
            Text = "ADD TO ORDER",
            Size = new Size(188, 34),
            Location = new Point(11, 198),
            BackColor = Theme.Accent,
            HoverBackColor = Theme.AccentDark,
            ForeColor = Color.FromArgb(8, 18, 20)
        };
        _add.Click += (_, _) => RequestAdd();

        Controls.Add(_image);
        Controls.Add(_name);
        Controls.Add(_price);
        Controls.Add(_add);

        Click += (_, _) => RequestAdd();
        _image.Click += (_, _) => RequestAdd();
        _name.Click += (_, _) => RequestAdd();
        _price.Click += (_, _) => RequestAdd();

        LoadImage();
    }

    public void SetSelectionEnabled(bool enabled)
    {
        Enabled = enabled;
        _add.Enabled = enabled;
        Cursor = enabled ? Cursors.Hand : Cursors.Default;
        BackColor = enabled ? _baseColor : Color.FromArgb(26, 32, 42);
    }

    public void FlashSelected()
    {
        _flashTimer?.Stop();
        _flashTicks = 0;
        _flashTimer = new System.Windows.Forms.Timer { Interval = 40 };
        _flashTimer.Tick += (_, _) =>
        {
            _flashTicks++;
            var amount = _flashTicks < 6 ? 40 : Math.Max(0, 40 - ((_flashTicks - 6) * 8));
            BackColor = Color.FromArgb(
                Math.Min(255, _baseColor.R + amount),
                Math.Min(255, _baseColor.G + amount),
                Math.Min(255, _baseColor.B + amount));
            if (_flashTicks >= 12)
            {
                BackColor = _baseColor;
                _flashTimer.Stop();
            }
        };
        _flashTimer.Start();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using var pen = new Pen(Theme.CardBorder);
        e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
    }

    private void RequestAdd()
    {
        if (!Enabled)
        {
            return;
        }

        AddRequested?.Invoke(this, _product);
    }

    private void LoadImage()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "Products", _product.ImageFileName);
        try
        {
            if (File.Exists(path))
            {
                using var stream = File.OpenRead(path);
                using var loaded = Image.FromStream(stream);
                _image.Image = new Bitmap(loaded);
                return;
            }
        }
        catch (Exception)
        {
            // Fall through to a generated placeholder so missing assets never crash the UI.
        }

        _image.Image = CreatePlaceholder(_product.Name);
    }

    private static Image CreatePlaceholder(string name)
    {
        var bitmap = new Bitmap(188, 128);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.FromArgb(24, 32, 48));
        using var font = new Font("Segoe UI Semibold", 11f, FontStyle.Bold);
        TextRenderer.DrawText(
            graphics,
            string.IsNullOrWhiteSpace(name) ? "No image" : name,
            font,
            new Rectangle(0, 0, 188, 128),
            Theme.TextMuted,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
        return bitmap;
    }
}

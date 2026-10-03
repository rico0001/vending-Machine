using CS2S564VendingMachine.Core;

namespace CS2S564VendingMachine.UI;

internal sealed class CoinSlotControl : Panel
{
    public event EventHandler<int>? CoinDropped;
    public event EventHandler<string>? InvalidDrop;

    private bool _dragOver;
    private bool _acceptFlash;
    private bool _rejectFlash;
    private System.Windows.Forms.Timer? _feedbackTimer;

    public bool AcceptingCoins { get; set; }

    public CoinSlotControl()
    {
        DoubleBuffered = true;
        AllowDrop = true;
        Size = new Size(220, 168);
        BackColor = Theme.SlotIdle;
    }

    protected override void OnDragEnter(DragEventArgs drgevent)
    {
        if (TryReadDenomination(drgevent.Data, out _) && AcceptingCoins)
        {
            drgevent.Effect = DragDropEffects.Copy;
            _dragOver = true;
            Invalidate();
        }
        else
        {
            drgevent.Effect = DragDropEffects.None;
            FlashReject();
        }

        base.OnDragEnter(drgevent);
    }

    protected override void OnDragOver(DragEventArgs drgevent)
    {
        if (TryReadDenomination(drgevent.Data, out _) && AcceptingCoins)
        {
            drgevent.Effect = DragDropEffects.Copy;
        }
        else
        {
            drgevent.Effect = DragDropEffects.None;
        }

        base.OnDragOver(drgevent);
    }

    protected override void OnDragLeave(EventArgs e)
    {
        _dragOver = false;
        Invalidate();
        base.OnDragLeave(e);
    }

    protected override void OnDragDrop(DragEventArgs drgevent)
    {
        _dragOver = false;

        if (!AcceptingCoins)
        {
            InvalidDrop?.Invoke(this, "Coins can only be inserted after checkout.");
            FlashReject();
            return;
        }

        if (!TryReadDenomination(drgevent.Data, out var pence) || !CoinDenominations.IsAccepted(pence))
        {
            InvalidDrop?.Invoke(this, "Only machine coins can be inserted.");
            FlashReject();
            return;
        }

        // DragDrop fires once per completed DoDragDrop, so a single drag cannot pay twice.
        CoinDropped?.Invoke(this, pence);
        FlashAccept();
        base.OnDragDrop(drgevent);
    }

    public void FlashAccept()
    {
        _acceptFlash = true;
        _rejectFlash = false;
        Pulse();
    }

    public void FlashReject()
    {
        _rejectFlash = true;
        _acceptFlash = false;
        Pulse();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(BackColor);

        var border = _dragOver ? Theme.SlotHover
            : _acceptFlash ? Theme.StatusSuccess
            : _rejectFlash ? Theme.Danger
            : Theme.CardBorder;

        using var pathBrush = new SolidBrush(Color.FromArgb(14, 18, 26));
        var bounds = new Rectangle(8, 8, Width - 17, Height - 17);
        g.FillRectangle(pathBrush, bounds);
        using var pen = new Pen(border, 3);
        g.DrawRectangle(pen, bounds);

        var slot = new Rectangle(28, 58, Width - 57, 18);
        using var slotFill = new SolidBrush(Color.FromArgb(6, 8, 12));
        g.FillRectangle(slotFill, slot);
        using var slotPen = new Pen(_dragOver ? Theme.Gold : Color.FromArgb(70, 80, 96), 2);
        g.DrawRectangle(slotPen, slot);

        var caption = _dragOver ? "RELEASE TO INSERT" : "COIN SLOT";
        TextRenderer.DrawText(
            g,
            caption,
            Theme.HeadingFont,
            new Rectangle(8, 18, Width - 16, 28),
            _dragOver ? Theme.Gold : Theme.TextPrimary,
            TextFormatFlags.HorizontalCenter);

        TextRenderer.DrawText(
            g,
            AcceptingCoins ? "Drag a coin here" : "Locked until checkout",
            Theme.BodyFont,
            new Rectangle(8, 92, Width - 16, 48),
            Theme.TextMuted,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.WordBreak);
    }

    private void Pulse()
    {
        Invalidate();
        _feedbackTimer?.Stop();
        _feedbackTimer = new System.Windows.Forms.Timer { Interval = 420 };
        _feedbackTimer.Tick += (_, _) =>
        {
            _acceptFlash = false;
            _rejectFlash = false;
            _feedbackTimer.Stop();
            Invalidate();
        };
        _feedbackTimer.Start();
    }

    private static bool TryReadDenomination(IDataObject? data, out int pence)
    {
        pence = 0;
        if (data is null || !data.GetDataPresent(DataFormats.Text))
        {
            return false;
        }

        var raw = data.GetData(DataFormats.Text) as string;
        return int.TryParse(raw, out pence);
    }
}

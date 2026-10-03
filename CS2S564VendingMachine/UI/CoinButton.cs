using CS2S564VendingMachine.Core;

namespace CS2S564VendingMachine.UI;

internal sealed class CoinButton : Control
{
    private bool _hover;
    private bool _press;

    public int DenominationPence { get; }

    public CoinButton(int denominationPence)
    {
        DenominationPence = denominationPence;
        Size = new Size(72, 72);
        Cursor = Cursors.Hand;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        _hover = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _hover = false;
        _press = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && Enabled)
        {
            _press = true;
            Invalidate();
            // Native WinForms drag-and-drop. The denomination travels as text so the slot can parse it once.
            DoDragDrop(DenominationPence.ToString(), DragDropEffects.Copy);
            _press = false;
            Invalidate();
        }

        base.OnMouseDown(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(Parent?.BackColor ?? Theme.PanelBackground);

        var rect = new Rectangle(3, 3, Width - 7, Height - 7);
        var fill = !Enabled
            ? Color.FromArgb(72, 76, 84)
            : _press ? Theme.GoldDark : _hover ? Color.FromArgb(232, 200, 96) : Theme.Gold;

        using (var brush = new SolidBrush(fill))
        {
            g.FillEllipse(brush, rect);
        }

        using (var pen = new Pen(Enabled ? Color.FromArgb(120, 90, 20) : Color.FromArgb(50, 50, 50), 2))
        {
            g.DrawEllipse(pen, rect);
        }

        var inner = Rectangle.Inflate(rect, -8, -8);
        using (var pen = new Pen(Color.FromArgb(Enabled ? 140 : 70, 255, 236, 180), 1))
        {
            g.DrawEllipse(pen, inner);
        }

        TextRenderer.DrawText(
            g,
            MoneyFormatter.FormatCoin(DenominationPence),
            Theme.HeadingFont,
            rect,
            Enabled ? Color.FromArgb(40, 28, 8) : Color.FromArgb(90, 90, 90),
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}

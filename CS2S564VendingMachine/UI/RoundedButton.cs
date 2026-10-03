namespace CS2S564VendingMachine.UI;

internal sealed class RoundedButton : Button
{
    public Color HoverBackColor { get; set; }
    public Color DisabledBackColor { get; set; } = Color.FromArgb(48, 56, 70);
    public int CornerRadius { get; set; } = 10;

    private bool _hover;

    public RoundedButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        Cursor = Cursors.Hand;
        ForeColor = Color.White;
        Font = Theme.ButtonFont;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        _hover = true;
        base.OnMouseEnter(e);
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _hover = false;
        base.OnMouseLeave(e);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        var fill = !Enabled
            ? DisabledBackColor
            : _hover && HoverBackColor.A > 0 ? HoverBackColor : BackColor;

        using var path = RoundedRect(ClientRectangle, CornerRadius);
        using var brush = new SolidBrush(fill);
        e.Graphics.Clear(Parent?.BackColor ?? Theme.PanelBackground);
        e.Graphics.FillPath(brush, path);

        TextRenderer.DrawText(
            e.Graphics,
            Text,
            Font,
            ClientRectangle,
            Enabled ? ForeColor : Theme.TextMuted,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    private static System.Drawing.Drawing2D.GraphicsPath RoundedRect(Rectangle bounds, int radius)
    {
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        var d = Math.Max(2, radius * 2);
        var rect = Rectangle.Inflate(bounds, -1, -1);
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}

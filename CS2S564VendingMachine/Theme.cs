namespace CS2S564VendingMachine;

internal static class Theme
{
    public static readonly Color WindowBackground = Color.FromArgb(12, 16, 24);
    public static readonly Color PanelBackground = Color.FromArgb(22, 30, 44);
    public static readonly Color PanelAlt = Color.FromArgb(28, 38, 56);
    public static readonly Color CardBackground = Color.FromArgb(32, 44, 64);
    public static readonly Color CardBorder = Color.FromArgb(58, 74, 102);
    public static readonly Color Accent = Color.FromArgb(0, 194, 168);
    public static readonly Color AccentDark = Color.FromArgb(0, 140, 122);
    public static readonly Color Gold = Color.FromArgb(214, 178, 72);
    public static readonly Color GoldDark = Color.FromArgb(156, 118, 28);
    public static readonly Color Danger = Color.FromArgb(214, 74, 74);
    public static readonly Color DangerDark = Color.FromArgb(156, 40, 40);
    public static readonly Color TextPrimary = Color.FromArgb(244, 247, 250);
    public static readonly Color TextMuted = Color.FromArgb(139, 155, 180);
    public static readonly Color SlotIdle = Color.FromArgb(18, 22, 30);
    public static readonly Color SlotHover = Color.FromArgb(0, 194, 168);
    public static readonly Color StatusIdle = Color.FromArgb(120, 136, 160);
    public static readonly Color StatusSelecting = Color.FromArgb(80, 170, 255);
    public static readonly Color StatusPayment = Color.FromArgb(240, 176, 64);
    public static readonly Color StatusSuccess = Color.FromArgb(46, 204, 113);
    public static readonly Color StatusCancelled = Color.FromArgb(214, 74, 74);

    public static readonly Font TitleFont = new("Segoe UI Semibold", 20f, FontStyle.Bold);
    public static readonly Font SubtitleFont = new("Segoe UI", 10f, FontStyle.Regular);
    public static readonly Font HeadingFont = new("Segoe UI Semibold", 11f, FontStyle.Bold);
    public static readonly Font BodyFont = new("Segoe UI", 9.5f, FontStyle.Regular);
    public static readonly Font PriceFont = new("Segoe UI Semibold", 10f, FontStyle.Bold);
    public static readonly Font ButtonFont = new("Segoe UI Semibold", 9f, FontStyle.Bold);
    public static readonly Font MonoFont = new("Consolas", 10f, FontStyle.Bold);
}

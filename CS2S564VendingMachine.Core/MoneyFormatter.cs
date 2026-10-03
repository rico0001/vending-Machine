namespace CS2S564VendingMachine.Core;

/// <summary>
/// Formats integer pence as UK sterling. All money in this application is stored as pence
/// so that totals never suffer floating-point rounding errors.
/// </summary>
public static class MoneyFormatter
{
    public static string FormatPence(int pence)
    {
        var sign = pence < 0 ? "-" : string.Empty;
        var absolute = Math.Abs(pence);
        return $"{sign}£{absolute / 100}.{absolute % 100:00}";
    }

    public static string FormatCoin(int pence)
    {
        return pence switch
        {
            5 => "5p",
            10 => "10p",
            20 => "20p",
            50 => "50p",
            100 => "£1",
            200 => "£2",
            _ => FormatPence(pence)
        };
    }
}

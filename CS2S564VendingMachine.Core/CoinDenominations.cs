namespace CS2S564VendingMachine.Core;

public static class CoinDenominations
{
    public static readonly int[] AllPence = { 5, 10, 20, 50, 100, 200 };

    public static bool IsAccepted(int pence) => Array.IndexOf(AllPence, pence) >= 0;
}

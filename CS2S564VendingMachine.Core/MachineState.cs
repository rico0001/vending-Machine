namespace CS2S564VendingMachine.Core;

/// <summary>
/// Valid vending-machine modes. Transitions are enforced by <see cref="VendingMachineController"/>.
/// </summary>
public enum MachineState
{
    Idle,
    SelectingProducts,
    AwaitingPayment,
    PaymentComplete,
    Cancelled
}

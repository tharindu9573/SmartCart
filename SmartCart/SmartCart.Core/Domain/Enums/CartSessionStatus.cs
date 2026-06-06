namespace SmartCart.Core.Domain.Enums;

public enum CartSessionStatus
{
    Started,
    Scanning,
    Checkout,
    PaymentProcessing,
    Completed
}

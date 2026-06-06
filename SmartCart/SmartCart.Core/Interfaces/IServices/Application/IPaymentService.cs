namespace SmartCart.Core.Interfaces.IServices.Application;

public record MockCardDetails(
    string MaskedCardNumber,
    string HolderName,
    string Expiry,
    decimal Balance);

public interface IPaymentService
{
    Task<MockCardDetails?> ResolvePaymentTokenAsync(string paymentUid);
    Task<(bool success, string invoiceNumber)> ProcessPaymentAsync(
        int cartId, int sessionId, int userId,
        string paymentMethod, string paymentToken, decimal amount);
}

using SmartCart.Core.Domain.Models;

namespace SmartCart.Core.Interfaces.IServices.Application;

public interface INotificationService
{
    Task SendOtpAsync(string mobileNumber, string otp);
    Task SendInvoiceEmailAsync(string email, string name, string invoiceNumber, decimal totalAmount, IEnumerable<InvoiceLineItem> items);
    Task SendSmsGreetingAsync(string mobileNumber, string name, string invoiceNumber);
}

using System.Net.Http.Headers;
using System.Text;
using Azure.Communication.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SmartCart.Core.Domain.Models;
using SmartCart.Core.Interfaces.IServices.Application;

namespace SmartCart.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IConfiguration _config;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IConfiguration config, ILogger<NotificationService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendOtpAsync(string mobileNumber, string otp)
    {
        var message = $"Your SmartCart OTP is: {otp}. Valid for 5 minutes.";
        await SendTextLkSmsAsync(mobileNumber, message);
    }

    public async Task SendSmsGreetingAsync(string mobileNumber, string name, string invoiceNumber)
    {
        var message = $"Hi {name}, thank you for shopping at SmartCart! Invoice #{invoiceNumber} has been emailed to you. See you again soon!";
        await SendTextLkSmsAsync(mobileNumber, message);
    }

    public async Task SendInvoiceEmailAsync(string email, string name, string invoiceNumber, decimal totalAmount, IEnumerable<InvoiceLineItem> items)
    {
        try
        {
            var connectionString = _config["AzureCommunication:ConnectionString"];
            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogWarning("ACS not configured. Invoice email skipped for {Email}", email);
                return;
            }

            var lineItemsList = items.ToList();
            const decimal vatRate = 0.21m;
            var subtotal = Math.Round(totalAmount / (1 + vatRate), 2);
            var vatAmount = Math.Round(totalAmount - subtotal, 2);
            var date = DateTime.UtcNow.ToString("dd MMM yyyy");
            var firstName = name.Split(' ')[0];

            var rowsSb = new StringBuilder();
            foreach (var item in lineItemsList)
            {
                rowsSb.Append($@"
                <tr>
                  <td style=""padding:12px 16px;border-bottom:1px solid #f0f0f0;"">
                    <div style=""font-weight:600;color:#1a1a1a;"">{item.Name}</div>
                    <div style=""font-size:12px;color:#888;margin-top:2px;"">{item.Category}</div>
                  </td>
                  <td style=""padding:12px 16px;border-bottom:1px solid #f0f0f0;text-align:center;color:#555;"">
                    {item.Quantity}
                  </td>
                  <td style=""padding:12px 16px;border-bottom:1px solid #f0f0f0;text-align:right;color:#555;"">
                    ${item.UnitPrice:F2}
                  </td>
                  <td style=""padding:12px 16px;border-bottom:1px solid #f0f0f0;text-align:right;font-weight:600;color:#1a1a1a;"">
                    ${item.LineTotal:F2}
                  </td>
                </tr>");
            }

            var html = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""utf-8"">
  <meta name=""viewport"" content=""width=device-width,initial-scale=1"">
  <title>SmartCart Invoice #{invoiceNumber}</title>
</head>
<body style=""margin:0;padding:0;background:#f4f6f8;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f4f6f8;padding:40px 0;"">
    <tr><td align=""center"">
      <table width=""620"" cellpadding=""0"" cellspacing=""0"" style=""max-width:620px;width:100%;"">

        <!-- Header -->
        <tr>
          <td style=""background:linear-gradient(135deg,#1a3a2a 0%,#2d6a4f 100%);border-radius:12px 12px 0 0;padding:36px 40px;"">
            <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
              <tr>
                <td>
                  <div style=""font-size:26px;font-weight:800;color:#ffffff;letter-spacing:-0.5px;"">🛒 SmartCart</div>
                  <div style=""font-size:13px;color:#a8d5ba;margin-top:4px;"">Smart Shopping, Seamless Checkout</div>
                </td>
                <td align=""right"">
                  <div style=""font-size:13px;color:#a8d5ba;text-transform:uppercase;letter-spacing:1px;"">Invoice</div>
                  <div style=""font-size:18px;font-weight:700;color:#ffffff;margin-top:2px;"">#{invoiceNumber}</div>
                  <div style=""font-size:12px;color:#a8d5ba;margin-top:4px;"">{date}</div>
                </td>
              </tr>
            </table>
          </td>
        </tr>

        <!-- White card body -->
        <tr>
          <td style=""background:#ffffff;padding:36px 40px;"">

            <!-- Greeting -->
            <p style=""margin:0 0 24px;font-size:16px;color:#333;"">
              Hi <strong>{firstName}</strong>, thank you for shopping at SmartCart! 🎉<br>
              <span style=""font-size:14px;color:#666;"">Here's your receipt for today's purchase.</span>
            </p>

            <!-- Items table -->
            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""border:1px solid #e8ecf0;border-radius:8px;overflow:hidden;"">
              <thead>
                <tr style=""background:#f8faf9;"">
                  <th style=""padding:12px 16px;text-align:left;font-size:12px;font-weight:700;color:#888;text-transform:uppercase;letter-spacing:0.5px;border-bottom:2px solid #e8ecf0;"">Item</th>
                  <th style=""padding:12px 16px;text-align:center;font-size:12px;font-weight:700;color:#888;text-transform:uppercase;letter-spacing:0.5px;border-bottom:2px solid #e8ecf0;"">Qty</th>
                  <th style=""padding:12px 16px;text-align:right;font-size:12px;font-weight:700;color:#888;text-transform:uppercase;letter-spacing:0.5px;border-bottom:2px solid #e8ecf0;"">Unit Price</th>
                  <th style=""padding:12px 16px;text-align:right;font-size:12px;font-weight:700;color:#888;text-transform:uppercase;letter-spacing:0.5px;border-bottom:2px solid #e8ecf0;"">Total</th>
                </tr>
              </thead>
              <tbody>
                {rowsSb}
              </tbody>
            </table>

            <!-- Totals -->
            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin-top:20px;"">
              <tr>
                <td width=""60%""></td>
                <td width=""40%"">
                  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f8faf9;border-radius:8px;padding:16px;"">
                    <tr>
                      <td style=""padding:6px 0;font-size:14px;color:#555;"">Subtotal (ex. VAT)</td>
                      <td style=""padding:6px 0;font-size:14px;color:#555;text-align:right;"">${subtotal:F2}</td>
                    </tr>
                    <tr>
                      <td style=""padding:6px 0;font-size:14px;color:#555;"">VAT (21%)</td>
                      <td style=""padding:6px 0;font-size:14px;color:#555;text-align:right;"">${vatAmount:F2}</td>
                    </tr>
                    <tr>
                      <td style=""padding:10px 0 6px;font-size:16px;font-weight:700;color:#1a1a1a;border-top:2px solid #e8ecf0;"">Total Paid</td>
                      <td style=""padding:10px 0 6px;font-size:16px;font-weight:700;color:#2d6a4f;text-align:right;border-top:2px solid #e8ecf0;"">${totalAmount:F2}</td>
                    </tr>
                  </table>
                </td>
              </tr>
            </table>

            <!-- Payment info chip -->
            <div style=""margin-top:24px;padding:14px 18px;background:#f0fdf4;border:1px solid #bbf7d0;border-radius:8px;display:inline-block;"">
              <span style=""color:#16a34a;font-size:14px;font-weight:600;"">✔ Payment Successful</span>
              <span style=""color:#555;font-size:13px;margin-left:12px;"">Contactless Card</span>
            </div>

          </td>
        </tr>

        <!-- Footer -->
        <tr>
          <td style=""background:#f8faf9;border-top:1px solid #e8ecf0;border-radius:0 0 12px 12px;padding:24px 40px;text-align:center;"">
            <p style=""margin:0;font-size:13px;color:#888;"">
              Questions? Visit us in-store or email <a href=""mailto:support@smartcart.lk"" style=""color:#2d6a4f;"">support@smartcart.lk</a>
            </p>
            <p style=""margin:8px 0 0;font-size:12px;color:#bbb;"">
              © {DateTime.UtcNow.Year} SmartCart · Automated RFID Checkout System
            </p>
          </td>
        </tr>

      </table>
    </td></tr>
  </table>
</body>
</html>";

            var plainText = new StringBuilder();
            plainText.AppendLine($"SmartCart Invoice #{invoiceNumber}");
            plainText.AppendLine($"Date: {date}");
            plainText.AppendLine($"\nDear {name},\nThank you for shopping at SmartCart!\n");
            plainText.AppendLine("ITEMS:");
            foreach (var item in lineItemsList)
                plainText.AppendLine($"  {item.Name} x{item.Quantity}  @  ${item.UnitPrice:F2} = ${item.LineTotal:F2}");
            plainText.AppendLine($"\nSubtotal (ex. VAT): ${subtotal:F2}");
            plainText.AppendLine($"VAT (21%):          ${vatAmount:F2}");
            plainText.AppendLine($"Total Paid:         ${totalAmount:F2}");
            plainText.AppendLine($"\nPayment: Contactless Card");
            plainText.AppendLine("\nThank you for your business!");

            var emailClient = new EmailClient(connectionString);
            var sender = _config["AzureCommunication:EmailFrom"]!;

            var emailMessage = new EmailMessage(
                senderAddress: sender,
                content: new EmailContent($"Your SmartCart Receipt – #{invoiceNumber}")
                {
                    PlainText = plainText.ToString(),
                    Html = html
                },
                recipients: new EmailRecipients(new[] { new EmailAddress(email, name) })
            );

            await emailClient.SendAsync(Azure.WaitUntil.Completed, emailMessage);
            _logger.LogInformation("Invoice email sent to {Email} for invoice {InvoiceNumber}", email, invoiceNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send invoice email to {Email}", email);
        }
    }

    private async Task SendTextLkSmsAsync(string mobileNumber, string message)
    {
        try
        {
            var apiKey   = _config["TextLK:ApiKey"];
            var senderId = _config["TextLK:SenderId"] ?? "TextLKDemo";
            var baseUrl  = _config["TextLK:BaseUrl"] ?? "https://app.text.lk/api/v3/sms/send";

            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("TextLK not configured. SMS skipped. Message for {Mobile}: {Message}", mobileNumber, message);
                return;
            }

            // Strip leading '+' — text.lk expects e.g. 94776562173
            var recipient = mobileNumber.TrimStart('+');

            using var client = new HttpClient();
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var body = JsonConvert.SerializeObject(new
            {
                recipient,
                sender_id = senderId,
                type = "plain",
                message
            });

            var response = await client.SendAsync(new HttpRequestMessage
            {
                Method  = HttpMethod.Post,
                Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json")
            });

            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                _logger.LogInformation("TextLK SMS sent to {Mobile}", mobileNumber);
            else
                _logger.LogError("TextLK SMS failed for {Mobile}: {Status} — {Body}", mobileNumber, response.StatusCode, responseBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send TextLK SMS to {Mobile}", mobileNumber);
        }
    }
}

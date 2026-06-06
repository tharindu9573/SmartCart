using SmartCart.Core.Domain.Enums;

namespace SmartCart.Core.Interfaces.IServices.Application;

public record ScanEventPayload(string Uid, int CartId, DateTime Timestamp, EventType EventType);

public interface ICartScanService
{
    Task ProcessScanEventAsync(ScanEventPayload payload);
}

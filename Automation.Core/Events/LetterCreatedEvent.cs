using MediatR;

namespace Automation.Core.Events;

/// <summary>
/// رویداد ایجاد نامه/سند در ماژول دبیرخانه — برای واکنش ماژول فرآیند (شروع گردش کار و ...).
/// </summary>
public record LetterCreatedEvent(
    int DocumentId,
    int? FormId,
    long? FormRecordId,
    int CreatedByUserId,
    string? Subject,
    DateTime CreatedAt
) : INotification;

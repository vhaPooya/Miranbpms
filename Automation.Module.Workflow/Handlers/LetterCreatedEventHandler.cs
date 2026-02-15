using Automation.Core.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Automation.Module.Workflow.Handlers;

/// <summary>
/// واکنش به رویداد ایجاد نامه در دبیرخانه (مثلاً شروع خودکار فرآیند مرتبط).
/// </summary>
public class LetterCreatedEventHandler : INotificationHandler<LetterCreatedEvent>
{
    private readonly ILogger<LetterCreatedEventHandler> _logger;

    public LetterCreatedEventHandler(ILogger<LetterCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(LetterCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "LetterCreated received: DocumentId={DocumentId}, FormId={FormId}, RecordId={RecordId}, ByUser={UserId}",
            notification.DocumentId, notification.FormId, notification.FormRecordId, notification.CreatedByUserId);
        // می‌توان اینجا بر اساس FormId/Workflow متصل، نمونه فرآیند را با IWorkflowEngineSqlService شروع کرد.
        return Task.CompletedTask;
    }
}

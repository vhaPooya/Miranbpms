-- =============================================
-- BPMS Workflow Engine - SQL Stored Procedures
-- اجرای منطق گردش کار در دیتابیس (بدون بارگذاری کامل در حافظه C#)
-- =============================================

-- sp_Workflow_MoveToNextStep: انتقال نمونه فرآیند به مرحله بعد با اعتبارسنجی و لاگ در یک تراکنش
IF OBJECT_ID('dbo.sp_Workflow_MoveToNextStep', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Workflow_MoveToNextStep;
GO
CREATE PROCEDURE dbo.sp_Workflow_MoveToNextStep
    @InstanceId        INT,
    @Action            NVARCHAR(100),   -- مثلاً 'Approve', 'Reject', 'Complete'
    @PerformedByUserId INT,
    @Notes             NVARCHAR(2000) = NULL,
    @Result            NVARCHAR(1000) = NULL,
    @TargetNodeId      INT = NULL,     -- در صورت چند مسیره مشخص شود
    @Success           BIT OUTPUT,
    @ErrorMessage      NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Success = 0;
    SET @ErrorMessage = NULL;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @CurrentState NVARCHAR(50), @WorkflowId INT, @CurrentNodeId INT;

        SELECT @CurrentState = State, @WorkflowId = WorkflowId
        FROM WorkflowInstances WITH (UPDLOCK)
        WHERE Id = @InstanceId AND IsDeleted = 0;

        IF @CurrentState IS NULL
        BEGIN
            SET @ErrorMessage = N'نمونه فرآیند یافت نشد.';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        IF @CurrentState IN (N'Completed', N'Cancelled', N'Terminated')
        BEGIN
            SET @ErrorMessage = N'فرآیند قبلاً به پایان رسیده است.';
            ROLLBACK TRANSACTION;
            RETURN;
        END

        -- آخرین انتقال انجام‌شده برای این نمونه
        SELECT TOP 1 @CurrentNodeId = TargetNodeId
        FROM WorkflowInstanceTransitions WITH (READUNCOMMITTED)
        WHERE WorkflowInstanceId = @InstanceId AND IsDeleted = 0
        ORDER BY PerformedAt DESC;

        -- اگر اولین قدم است، گره شروع را پیدا کن
        IF @CurrentNodeId IS NULL
            SELECT TOP 1 @CurrentNodeId = Id
            FROM WorkflowNodes WITH (READUNCOMMITTED)
            WHERE WorkflowId = @WorkflowId AND IsDeleted = 0
              AND LOWER(ISNULL(NodeType, N'')) IN (N'start', N'startevent', N'start_event');

        IF @TargetNodeId IS NULL
        BEGIN
            -- یافتن گره بعدی بر اساس اتصال (اولین مسیر مجاز)
            SELECT TOP 1 @TargetNodeId = wc.TargetNodeId
            FROM WorkflowConnections wc WITH (READUNCOMMITTED)
            WHERE wc.SourceNodeId = @CurrentNodeId AND wc.WorkflowId = @WorkflowId AND wc.IsDeleted = 0
              AND (wc.Condition IS NULL OR LEN(RTRIM(wc.Condition)) = 0 OR wc.Condition = @Action);
        END

        -- در صورت پایان فرآیند (گره End)
        IF @TargetNodeId IS NOT NULL
        BEGIN
            DECLARE @TargetType NVARCHAR(50);
            SELECT @TargetType = NodeType FROM WorkflowNodes WITH (READUNCOMMITTED) WHERE Id = @TargetNodeId AND IsDeleted = 0;

            IF LOWER(ISNULL(@TargetType, N'')) IN (N'end', N'endevent', N'end_event')
            BEGIN
                UPDATE WorkflowInstances
                SET State = N'Completed', CompletedAt = SYSUTCDATETIME(), UpdatedAt = SYSUTCDATETIME()
                WHERE Id = @InstanceId;
            END
        END

        -- ثبت انتقال
        INSERT INTO WorkflowInstanceTransitions (WorkflowInstanceId, SourceNodeId, TargetNodeId, PerformedAt, Result, Notes, IsDeleted, CreationDate)
        VALUES (@InstanceId, @CurrentNodeId, @TargetNodeId, SYSUTCDATETIME(), @Result, @Notes, 0, SYSUTCDATETIME());

        SET @Success = 1;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @ErrorMessage = ERROR_MESSAGE();
        SET @Success = 0;
    END CATCH
END
GO

-- fn_Workflow_GetNextApprover: بازگرداندن شناسه کاربر/سمت بعدی تاییدکننده (مقدار ساده برای استفاده در SPها)
IF OBJECT_ID('dbo.fn_Workflow_GetNextApprover', 'FN') IS NOT NULL
    DROP FUNCTION dbo.fn_Workflow_GetNextApprover;
GO
CREATE FUNCTION dbo.fn_Workflow_GetNextApprover
(
    @InstanceId   INT,
    @NextNodeId   INT
)
RETURNS INT
AS
BEGIN
    DECLARE @ApproverUserId INT = NULL;

    -- بر اساس تنظیمات گره (AssigneeType, AssigneeId در جدول WorkflowNodes یا JSON در Settings)
    -- در این نمونه ساده: اگر گره دارای AssigneeId باشد برگردانده می‌شود (ستون در صورت وجود)
    SELECT @ApproverUserId = NULL; -- در اسکیما فعلی WorkflowNode ممکن است AssigneeId نباشد؛ از Settings JSON قابل استخراج است

    RETURN @ApproverUserId;
END
GO

-- sp_Workflow_StartInstance: شروع یک نمونه فرآیند جدید
IF OBJECT_ID('dbo.sp_Workflow_StartInstance', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Workflow_StartInstance;
GO
CREATE PROCEDURE dbo.sp_Workflow_StartInstance
    @WorkflowId      INT,
    @WorkflowVersionId INT = NULL,
    @StartedByUserId INT,
    @FormRecordId    BIGINT = NULL,
    @FormId          INT = NULL,
    @NewInstanceId   INT OUTPUT,
    @Success         BIT OUTPUT,
    @ErrorMessage    NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @NewInstanceId = 0;
    SET @Success = 0;
    SET @ErrorMessage = NULL;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @WorkflowVersionId IS NULL
            SELECT TOP 1 @WorkflowVersionId = Id FROM WorkflowVersions WHERE WorkflowId = @WorkflowId AND IsDeleted = 0 ORDER BY Version DESC;

        INSERT INTO WorkflowInstances (WorkflowId, WorkflowVersionId, State, StartedAt, CompletedAt, IsDeleted, CreationDate)
        VALUES (@WorkflowId, @WorkflowVersionId, N'Running', SYSUTCDATETIME(), NULL, 0, SYSUTCDATETIME());

        SET @NewInstanceId = SCOPE_IDENTITY();

        -- ثبت اولین انتقال به گره شروع
        DECLARE @StartNodeId INT;
        SELECT TOP 1 @StartNodeId = Id
        FROM WorkflowNodes
        WHERE WorkflowId = @WorkflowId AND IsDeleted = 0
          AND LOWER(ISNULL(NodeType, N'')) IN (N'start', N'startevent', N'start_event');

        IF @StartNodeId IS NOT NULL
            INSERT INTO WorkflowInstanceTransitions (WorkflowInstanceId, SourceNodeId, TargetNodeId, PerformedAt, Result, Notes, IsDeleted, CreationDate)
            VALUES (@NewInstanceId, NULL, @StartNodeId, SYSUTCDATETIME(), N'Started', N'شروع فرآیند', 0, SYSUTCDATETIME());

        SET @Success = 1;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @ErrorMessage = ERROR_MESSAGE();
        SET @Success = 0;
    END CATCH
END
GO

-- View: وضعیت جاری نمونه فرآیند برای نمایش در UI
IF OBJECT_ID('dbo.vw_Workflow_InstanceCurrentState', 'V') IS NOT NULL
    DROP VIEW dbo.vw_Workflow_InstanceCurrentState;
GO
CREATE VIEW dbo.vw_Workflow_InstanceCurrentState
AS
SELECT
    wi.Id AS InstanceId,
    wi.WorkflowId,
    wi.State AS CurrentState,
    wi.StartedAt,
    wi.CompletedAt,
    t.TargetNodeId AS CurrentNodeId,
    wn.TitleFa AS CurrentNodeTitleFa,
    wn.NodeType AS CurrentNodeType
FROM WorkflowInstances wi
LEFT JOIN (
    SELECT WorkflowInstanceId, TargetNodeId,
           ROW_NUMBER() OVER (PARTITION BY WorkflowInstanceId ORDER BY PerformedAt DESC) AS rn
    FROM WorkflowInstanceTransitions
    WHERE IsDeleted = 0
) t ON t.WorkflowInstanceId = wi.Id AND t.rn = 1
LEFT JOIN WorkflowNodes wn ON wn.Id = t.TargetNodeId AND wn.IsDeleted = 0
WHERE wi.IsDeleted = 0;
GO

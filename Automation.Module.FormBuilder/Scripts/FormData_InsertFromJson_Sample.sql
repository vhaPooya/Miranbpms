-- =============================================
-- نمونه Stored Procedure برای درج داده فرم با پارامتر JSON
-- (طبق پرامپت: داده داینامیک با SP و JSON/TVP)
-- برای هر جدول فرمی یک SP مشابه با نام SP_[TableName]_InsertFromJson ساخته می‌شود یا از این الگو استفاده کنید.
-- =============================================

-- مثال: درج یک ردیف در جدول داینامیک با JSON
-- SP_[YourTableName]_InsertFromJson @JsonData NVARCHAR(MAX), @CreatorUserId INT
/*
CREATE OR ALTER PROCEDURE dbo.SP_YourTableName_InsertFromJson
    @JsonData NVARCHAR(MAX),
    @CreatorUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Id INT;

    INSERT INTO dbo.YourTableName (CreatedAt, CreatedBy, ... )
    SELECT
        SYSUTCDATETIME(),
        @CreatorUserId,
        JSON_VALUE(@JsonData, '$.Field1'),
        JSON_VALUE(@JsonData, '$.Field2'),
        CAST(JSON_VALUE(@JsonData, '$.Amount') AS DECIMAL(18,4))
    FROM OPENJSON(@JsonData);

    SET @Id = SCOPE_IDENTITY();
    SELECT @Id AS Id;
END
GO
*/

-- برای ساخت خودکار SP از سرویس DynamicTableGenerator و متادیتای FormField
-- می‌توان در C# اسکریپت مشابه را با نام جدول و ستون‌ها تولید کرد.

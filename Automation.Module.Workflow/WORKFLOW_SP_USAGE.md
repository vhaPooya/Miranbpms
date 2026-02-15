# فراخوانی Stored Procedure و Function در ماژول گردش کار

در معماری Hybrid، داده‌های **اجرای گردش کار** (WorkflowInstance، WorkflowInstanceVariable، WorkflowInstanceTransition) و داده‌های پویای فرم با **Dapper / Stored Procedure** انجام می‌شود، نه EF Core.

## نمونه فراخوانی SP از داخل گردش کار

```csharp
using System.Data; // CommandType
using Automation.Core.Interfaces;

// تزریق IDapperService در سرویس/کنترلر گردش کار
public class MyWorkflowStepService
{
    private readonly IDapperService _dapperService;

    public MyWorkflowStepService(IDapperService dapperService) => _dapperService = dapperService;

    public async Task<decimal> CalculateSalaryAsync(int personnelId, int year, int month)
    {
        var result = await _dapperService.QueryFirstOrDefaultAsync<decimal>(
            "sp_CalculateSalary",
            new { PersonnelId = personnelId, Year = year, Month = month },
            commandType: CommandType.StoredProcedure);
        return result;
    }

    public async Task<IEnumerable<SalaryRow>> GetSalaryDetailsAsync(int personnelId, int year)
    {
        var rows = await _dapperService.QueryAsync<SalaryRow>(
            "fn_GetSalaryDetails",
            new { PersonnelId = personnelId, Year = year },
            commandType: CommandType.StoredProcedure);
        return rows;
    }
}
```

## نکات

- برای **اجرای** SP بدون خروجی جدولی: `_dapperService.ExecuteAsync("sp_YourProc", params, commandType: CommandType.StoredProcedure)`
- برای **خواندن** نتیجه: `_dapperService.QueryAsync<T>("sp_YourProc", params, commandType: CommandType.StoredProcedure)` یا `QueryFirstOrDefaultAsync<T>`
- ماژول Workflow باید به `IDapperService` (و در صورت نیاز `IDbConnectionFactory`) وابسته باشد و از آن برای تمام عملیات روی داده‌های اجرای گردش و محاسبات (مثل حقوق) استفاده کند.

# BPMS – خلاصه تغییرات معماری و آیتم‌های زمینه جاری

**نکته:** اگر پروژه Infrastructure به‌خاطر خطاهای قبلی (نوع‌های گم‌شده مثل `WorkflowToken`, `UserActivityLog`, یا پکیج‌ها) بیلد نشد، پکیج‌های `Microsoft.Extensions.Logging.Abstractions`, `Microsoft.Extensions.Configuration.Abstractions`, `Microsoft.Extensions.Http`, `Microsoft.Extensions.Diagnostics.HealthChecks` به Infrastructure اضافه شده‌اند. رفع بقیه خطاها نیاز به تعریف یا جابه‌جایی آن نوع‌ها در Core/Infrastructure دارد.

## ۱. آیتم‌های زمینه جاری (در سراسر پروژه)

این آیتم‌ها با **نام‌های ثابت** در دسترس هستند:

| نام       | معنی |
|----------|------|
| **OUserId** | کاربر جاری (کاربر لاگین‌کرده) – شناسه کاربر |
| **OPosId** | سمت/پوزیشن جاری – شناسه پوزیشن |
| **OFEIC** | کد/شناسه فرم جاری (فرم در حال اجرا یا فرم متصل به فرآیند) |
| **OFEC** | شناسه رکورد در حال ثبت یا ویرایش |
| **OOrganizationId** | سازمان جاری کاربر |
| **ODepartmentId** | دپارتمان جاری کاربر |
| **OSecretariatId** | دبیرخانه جاری |
| **OWorkflowInstanceId** | نمونه فرآیند جاری (در صورت اجرا در زمینه فرآیند) |

### استفاده در کد

- **خواندن:** تزریق `ICurrentContext` و استفاده از پراپرتی‌ها یا `GetValue(CurrentContextNames.OUserId)` و غیره.
- **تنظیم فرم/رکورد برای همین درخواست:** تزریق `ICurrentContextSetter` و فراخوانی `SetFormContext(formId, recordId)` در کنترلر فرم/داده.
- **در View:** از طریق فیلتر، مقادیر در `ViewBag` قرار می‌گیرند: `ViewBag.OUserId`, `ViewBag.OPosId`, `ViewBag.OFEIC`, `ViewBag.OFEC` و غیره. ثابت نام‌ها در `CurrentContextNames` (مثلاً `CurrentContextNames.OUserId`) موجود است.

### لاگین و Session

- در لاگین، علاوه بر Claims، مقدار **OUserId** و در صورت وجود **OPosId** (اولین پوزیشن کاربر) در Session قرار می‌گیرد.
- برای تغییر سمت جاری: `ISessionService.SetCurrentPositionId(positionId)`.

---

## ۲. معماری Modular Monolith

- **Automation.Core:** اینترفیس‌های مشترک (`IModule`, `IRepository`), ثابت‌ها (`CurrentContextNames`), رویدادها (`LetterCreatedEvent`), و دامنه مشترک.
- **Automation.Module.FormBuilder:** سرویس‌های فرم (FormSchema, FormTable, FormXml, Word, **DynamicTableGenerator**).
- **Automation.Module.Workflow:** سرویس موتور گردش کار مبتنی بر SQL (**WorkflowEngineSqlService**) و هندلر رویداد `LetterCreated`.
- **Automation.Module.Secretariat:** ماژول دبیرخانه (امکان گسترش سرویس‌ها و انتشار رویداد).
- **Automation.Web:** پروژه UI (Views، کنترلرهای فعلی).
- **Automation.Web.Host:** نقطه ورود اصلی — فقط startup، بارگذاری ماژول‌ها، پیکربندی سراسری؛ بدون منطق کسب‌وکار. پروژه استارتاپ را روی **Automation.Web.Host** قرار دهید.

### تفکیک دیتابیس (Hybrid)

- **IdentityDbContext** (دامنه استاتیک/ادمین): Users, Roles, Permissions, Organization, Department, Position, SystemSettings, Group, PermissionGroup و ... با EF Core Code-First. در لاگین و UserContext از همین Context استفاده می‌شود.
- **AutomationDbContext**: متادیتای فرم‌ها، فرآیندها، اسناد و ... (همان دیتابیس؛ جدا برای لایه منطقی).
- داده داینامیک فرم و اجرای موتور Workflow با **Dapper + Stored Procedure** (اسکریپت‌ها در ماژول Workflow و نمونه SP با JSON در FormBuilder).

### بارگذاری ماژول‌ها و کنترلرها (Module Loader)

در `Program.cs` (در Host یا Web):
- `AddModules(configuration, typeof(FormBuilderModule), typeof(WorkflowModule), typeof(SecretariatModule))` برای ثبت سرویس‌های ماژول‌ها.
- `AddControllersWithViews(...).AddApplicationPart(typeof(FormBuilderModule).Assembly).AddApplicationPart(typeof(WorkflowModule).Assembly).AddApplicationPart(typeof(SecretariatModule).Assembly)` تا کنترلرها و Viewهای داخل اسمبلی ماژول‌ها به pipeline اضافه شوند.
- هر ماژول می‌تواند کنترلر داشته باشد (مثلاً `FormBuilderApiController`, `WorkflowApiController`, `SecretariatApiController`). کنترلرهای فعلی در Automation.Web هستند؛ می‌توان به‌تدریج به ماژول مربوط منتقل شد.

---

## ۳. موتور گردش کار مبتنی بر SQL

- اسکریپت‌ها در **Automation.Module.Workflow/Scripts/WorkflowEngine_StoredProcedures.sql** قرار دارند.
- پس از اجرای این اسکریپت روی دیتابیس، از **IWorkflowEngineSqlService** استفاده کنید:
  - `StartInstanceAsync`: شروع نمونه فرآیند.
  - `MoveToNextStepAsync`: انتقال به مرحله بعد با اعتبارسنجی و لاگ در یک تراکنش.

---

## ۴. ارتباط بین ماژول‌ها (MediatR)

- رویداد **LetterCreatedEvent** در Core تعریف شده است.
- **Secretariat** پس از ایجاد نامه/سند می‌تواند این رویداد را منتشر کند:
  - تزریق `IPublisher` (MediatR) و فراخوانی:  
    `await _publisher.Publish(new LetterCreatedEvent(documentId, formId, formRecordId, userId, subject, DateTime.UtcNow), cancellationToken);`
- **Workflow** با **LetterCreatedEventHandler** این رویداد را دریافت می‌کند (فعلاً لاگ؛ می‌توان شروع خودکار فرآیند را اضافه کرد).

---

## ۵. Dynamic Table Generator (ماژول FormBuilder)

- **IDynamicTableGeneratorService** طراحی فرم را به اسکریپت `CREATE TABLE` تبدیل می‌کند.
- `EnsureTableFromFormDesignAsync(form, fields)`: در صورت نبود جدول آن را می‌سازد و در صورت وجود، ستون‌های جدید را اضافه می‌کند.
- `GenerateCreateTableScript(tableName, fields)`: فقط اسکریپت را برمی‌گرداند (بدون اجرا).

---

## ۶. درج/بازیابی داده فرم با SP و JSON

طبق پرامپت، داده داینامیک فرم باید با Stored Procedure (و ترجیحاً JSON یا TVP) انجام شود. پس از ساخت جدول با **DynamicTableGenerator**، می‌توان برای هر جدول فرم SPهایی مثل زیر تعریف کرد و از Dapper فراخوانی کرد:

- **درج:** `SP_[TableName]_Insert @JsonData NVARCHAR(MAX), @CreatorUserId INT` که داخل SP با `OPENJSON` ردیف درج شود.
- **بازیابی:** `SP_[TableName]_GetAll`, `SP_[TableName]_GetById` (موجود در پروژه). برای یکپارچگی با معماری، این SPها را در دیتابیس نگه دارید و فقط از Dapper در C# استفاده کنید، نه EF برای جداول داینامیک.

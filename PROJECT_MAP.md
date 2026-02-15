# نقشه پروژه BPMS (سیستم مدیریت فرآیند کسب‌وکار)

**Framework:** ASP.NET Core 9.0 | **Database:** SQL Server | **زبان:** C# با پشتیبانی فارسی
**آخرین بروزرسانی:** 2026-02-11

---

## 1. معماری کلی (4 پروژه اصلی)

```
Automation.Web.sln
├── Automation.Core           ← لایه دامنه (Entities + Interfaces + DTOs)
├── Automation.Infrastructure ← لایه داده و سرویس‌ها
├── Automation.Web            ← لایه ارائه (MVC)
└── WinServices/
    ├── EmailService          ← سرویس ایمیل (Windows Service)
    └── FaxService            ← سرویس فکس (Windows Service)
```

---

## 2. لایه‌ها

### Automation.Core — لایه دامنه

| پوشه | تعداد | محتوا |
|------|-------|--------|
| Entities/ | 66+ | موجودیت‌های دامنه |
| DTOs/ | 71+ | اشیاء انتقال داده |
| Interfaces/ | 16+ | قراردادهای سرویس |

**گروه‌بندی موجودیت‌ها:**
- **فرم‌ساز:** `Form`, `FormField`, `FieldType`, `FormScript`, `FormButton`, `FormValidation`, `FormDesign`, `FormDataTable`, `ComboboxItem`, `FormButtonType`, `ButtonStylePreset`, `FormStaticAsset`, `ScalarFunction`, `PrintTemplate`, `FormNumberingRule`
- **موتور گردش کار:** `Workflow`, `WorkflowNode`, `WorkflowInstance`, `WorkflowVersion`, `WorkflowConnection`, `WorkflowVariable`, `WorkflowInstanceVariable`, `WorkflowInstanceTransition`, `WorkflowEngine`
- **مدیریت اسناد:** `Document`, `IncomingDocument`, `OutgoingDocument`, `DocumentReferral`, `DocumentAttachment`, `DocumentTracking`, `DocumentRegister`, `DocumentKeyword`, `DocumentCopyRecipient`
- **کاربران/امنیت:** `User`, `Role`, `Position`, `UserRole`, `UserPosition`, `RolePosition`, `UserSignature`, `Permission`, `RolePermission`, `Group`, `GroupMember`, `GroupPermission`, `PermissionGroup`, `RoleGroup`
- **سازمان:** `Organization`, `Department`, `Secretariat`
- **پیام‌رسانی:** `EmailMessage`, `EmailServiceSettings`, `FaxMessage`, `FaxServiceSettings`
- **تنظیمات سیستم:** `SystemSetting`, `ConnectionString`, `WebService`, `Theme`, `GlobalVariables`, `NumberingRule`, `Report`

**Enumerations مهم:**
- `WorkflowNodeType`: Start, End, Task, Decision, Parallel, Merge, Wait, Service, Script, Human
- `WorkflowInstanceState`: Running, Completed, Cancelled, Suspended, Error
- `FormCategoryType`: Administrative, Scheduled, Financial, Archive, Recruitment, General
- `FormLayoutType`: Flex, Grid, Table

---

### Automation.Infrastructure — لایه زیرساخت

```
Data/
├── AutomationDbContext.cs      (100+ DbSet)
├── SeedData.cs
└── SuperAdminSeeder.cs

Migrations/
└── 20260209000511_AddWorkformbuilder.cs  (آخرین migration)

Services/
├── DocumentService.cs
├── FormSchemaService.cs
├── FormXmlService.cs
├── FormTableService.cs
├── FormButtonService.cs
├── WorkflowEngine.cs
├── PermissionService.cs
├── ArchiveService.cs
├── CabinetService.cs
├── EmailService.cs
├── FaxService.cs
├── SearchService.cs
├── FileUploadService.cs
├── MemoryCacheService.cs
├── UserContextService.cs
├── DapperService.cs
├── PasswordHasher.cs
├── DocumentValidationService.cs
├── WordDocumentService.cs
├── EncryptedFileService.cs
├── Administration/
│   ├── LicenseManagementService.cs
│   ├── SystemMonitoringDashboard.cs
│   └── UserProvisioningService.cs
├── Audit/AuditTrailService.cs
├── Caching/RedisCacheService.cs
├── Documents/DocumentLifecycleManager.cs
├── Notifications/NotificationService.cs
├── Performance/
│   ├── DatabaseOptimizer.cs
│   ├── LoadBalancer.cs
│   └── PerformanceMonitor.cs
├── Security/
│   ├── EncryptionService.cs
│   ├── SecurityAuditor.cs
│   └── TwoFactorAuthenticationService.cs
├── Workflow/WorkflowEngine.cs
└── Integration/
    ├── ApiGateway/ApiGatewayService.cs
    ├── Database/DbConnectorService.cs
    ├── WebServices/WebServiceClient.cs
    └── ThirdPartyConnectors/
        ├── CrmConnector.cs
        └── ErpConnector.cs
```

**NuGet Packages:** EF Core 9.0, Dapper 2.1, BCrypt.Net-Next 4.0.3, StackExchange.Redis 2.8.16, MailKit 4.9.0

---

### Automation.Web — لایه ارائه (MVC)

**23 کنترلر:**
```
Controllers/
├── AccountController.cs          ← احراز هویت
├── HomeController.cs
├── FormBuilderController.cs      ← طراح فرم
├── FormDataController.cs         ← داده‌های فرم
├── DocumentController.cs         ← مدیریت اسناد
├── NewDocumentController.cs      ← ایجاد سند جدید
├── InboxController.cs            ← صندوق ورودی
├── CabinetController.cs          ← کارتابل
├── ReferredController.cs         ← ارجاع‌ها
├── ArchiveController.cs          ← بایگانی
├── SearchController.cs           ← جستجو
├── WorkflowController.cs         ← گردش کار
├── EmailController.cs
├── EmailSettingsController.cs
├── FaxController.cs
├── FaxSettingsController.cs
├── PermissionController.cs       ← مدیریت دسترسی
├── OrganizationController.cs     ← مدیریت سازمان
├── GroupController.cs
├── MetadataController.cs
├── SecretariatController.cs      ← دبیرخانه
├── SettingsController.cs
└── ReservedNumberController.cs
```

**ساختار wwwroot:**
```
wwwroot/
├── fonts/
│   ├── IRANSansWeb/     (فونت فارسی - eot, ttf, woff, woff2)
│   ├── vazir-font/
│   └── bootstrap-icons/
├── js/
│   ├── formbuilder/formbuilder.js   ← منطق اصلی فرم‌ساز
│   ├── form-action-buttons.js
│   ├── site.js
│   └── utils.js
├── lib/  (Bootstrap, jQuery, jQuery-Validation)
├── images/users/
└── uploads/
    ├── avatars/
    └── signatures/
```

**ویژگی‌های خاص:**
- `Hubs/NotificationHub.cs` ← SignalR برای اعلان‌های آنی
- `ViewComponents/MainPageViewComponent.cs`
- احراز هویت Cookie با انقضای 8 ساعته، Session 30 دقیقه‌ای

---

### WinServices — سرویس‌های پس‌زمینه

| سرویس | فناوری | وظیفه |
|--------|--------|--------|
| EmailService | MailKit 4.9 + Windows Service | ارسال ایمیل در پس‌زمینه |
| FaxService | Worker Service | مدیریت فکس |

---

## 3. قابلیت‌های اصلی

| قابلیت | توضیح |
|--------|--------|
| **فرم‌ساز** | طراح بصری با Drag & Drop، 3 نوع چیدمان (Flex, Grid, Table) |
| **موتور گردش کار** | 10 نوع گره، نسخه‌بندی، مدیریت وضعیت، متغیرها |
| **مدیریت اسناد** | ورودی/خروجی/داخلی، ارجاع، پیگیری، امضا |
| **کارتابل** | صندوق ورودی، ارسالی، شخصی، داخلی، ارجاع‌شده |
| **بایگانی/دبیرخانه** | طبقه‌بندی، جستجو، چرخه حیات سند |
| **سیستم مجوز** | RBAC چند سطحی، سطح فیلد، سطح گردش کار، 7 نوع ActionType |
| **یکپارچه‌سازی** | ERP, CRM, API Gateway, Web Services |
| **اعلان آنی** | SignalR Hub |
| **کش** | In-Memory + Redis |
| **امنیت** | BCrypt, Encryption, 2FA, Audit Trail |

---

## 4. سیستم مجوز (ActionType)

| نوع | وضعیت فرم |
|-----|-----------|
| ACTION_FOR_SIGNATURE | قابل ویرایش |
| ACTION_FOR_REVIEW | قفل |
| ACTION_FOR_ORDER | قفل |
| ACTION_FOR_OUTGOING | قابل ویرایش |
| ACTION_FOR_ARCHIVE | قفل |
| ACTION_FOR_EDIT | قابل ویرایش |
| ACTION_FOR_VIEW_COPY | قفل |

---

## 5. پیکربندی (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=CodexDb;...",
    "Redis": "localhost:6379"
  },
  "ExternalServices": {
    "ERP": { "BaseUrl": "...", "ApiKey": "...", "Enabled": false },
    "CRM": { "BaseUrl": "...", "ApiKey": "...", "Enabled": false },
    "Payment": { ... },
    "SMS": { ... }
  }
}
```

---

## 6. پشته فناوری

```
Backend:  ASP.NET Core 9 + EF Core 9 + Dapper + SignalR
Database: SQL Server + Redis
Security: BCrypt + Cookie Auth + Encryption + 2FA
Frontend: Razor Views + Bootstrap + jQuery + Persian Fonts
Services: Windows Services (Email + Fax)
```

---

## 7. مستندات مرتبط

- `Automation.Web/Documents/PermissionSystemDesign.md` ← طراحی کامل سیستم مجوز
- `Graph1.dgml` ← نمودار وابستگی پروژه‌ها

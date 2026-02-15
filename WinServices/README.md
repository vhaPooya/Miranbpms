# سرویس‌های اتوماسیون (ایمیل و فکس)

این راه‌حل شامل **دو پروژه سرویس ویندوزی** مستقل برای استفاده در اتوماسیون است:

| پروژه | پوشه | توضیح |
|--------|------|--------|
| **Automation.EmailService** | `EmailService/` | ارسال (SMTP) و دریافت (IMAP) ایمیل |
| **Automation.FaxService** | `FaxService/` | ارسال و دریافت فکس (Twilio + وب‌هوک) |

---

## یکپارچه‌سازی با اتوماسیون

### سرویس ایمیل

- **ارسال:** در پوشه `EmailOutbox` (قابل تغییر در `appsettings`) یک فایل با پسوند `*.email.json` قرار دهید. محتوای نمونه:

```json
{
  "To": "recipient@example.com",
  "Subject": "موضوع",
  "Body": "<p>متن HTML</p>",
  "IsBodyHtml": true,
  "ReferenceId": "شناسه از طرف اتوماسیون",
  "AttachmentPaths": ["file1.pdf"]
}
```

پس از ارسال موفق، فایل JSON حذف می‌شود.

- **دریافت:** ایمیل‌های جدید از IMAP در پوشه `EmailInbox` ذخیره می‌شوند: هر ایمیل یک فایل `.eml` و یک فایل `.json` (فرستنده، گیرنده، موضوع، تاریخ) دارد. اتوماسیون می‌تواند این پوشه را بخواند.

### سرویس فکس

- **ارسال:** در پوشه `FaxOutbox` یک فایل `*.fax.json` و در صورت نیاز فایل PDF قرار دهید. نمونه JSON:

```json
{
  "To": "+982112345678",
  "MediaUrl": "https://public-url-to-your.pdf/file.pdf"
}
```

یا با فایل محلی (در این حالت سرویس فکس باید از طریق اینترنت در دسترس باشد و `BaseUrlForMedia` تنظیم شود):

```json
{
  "To": "+982112345678",
  "FileName": "document.pdf",
  "ReferenceId": "شناسه"
}
```

- **دریافت:** فکس‌های دریافتی Twilio از طریق وب‌هوک `POST /fax/incoming` به سرویس ارسال می‌شوند و در پوشه `FaxInbox` به صورت PDF ذخیره می‌شوند.

---

## تنظیمات

### سرویس ایمیل (`EmailService/appsettings.json`)

```json
"Email": {
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UseSsl": true,
    "UserName": "your-email@gmail.com",
    "Password": "app-password",
    "DefaultFromAddress": "your-email@gmail.com",
    "DefaultFromName": "اتوماسیون"
  },
  "Imap": {
    "Host": "imap.gmail.com",
    "Port": 993,
    "UseSsl": true,
    "UserName": "your-email@gmail.com",
    "Password": "app-password",
    "InboxFolderName": "INBOX"
  },
  "OutboxPath": "EmailOutbox",
  "InboxPath": "EmailInbox",
  "PollIntervalSeconds": 60,
  "ReceiveEnabled": true,
  "SendEnabled": true
}
```

برای Gmail از «رمز عبور برنامه» (App Password) استفاده کنید.

### سرویس فکس (`FaxService/appsettings.json`)

```json
"Fax": {
  "SendEnabled": true,
  "TwilioAccountSid": "AC...",
  "TwilioAuthToken": "...",
  "TwilioFromNumber": "+1234567890",
  "OutboxPath": "FaxOutbox",
  "InboxPath": "FaxInbox",
  "PollIntervalSeconds": 45,
  "BaseUrlForMedia": "https://your-server.com:5002/faxfiles",
  "IncomingWebhookPath": "/fax/incoming"
}
```

در پنل Twilio برای شماره فکس خود آدرس وب‌هوک را روی  
`https://your-server.com:5002/fax/incoming`  
قرار دهید.

---

## نصب به عنوان سرویس ویندوز

هر دو پروژه با `Microsoft.Extensions.Hosting.WindowsServices` پیکربندی شده‌اند.

### سرویس ایمیل

```powershell
cd "مسیر\Automation.Services"
dotnet publish EmailService\Automation.EmailService.csproj -c Release
sc create "Automation.EmailService" binPath= "مسیر\Automation.Services\EmailService\bin\Release\net9.0\Automation.EmailService.exe"
sc start Automation.EmailService
```

### سرویس فکس

سرویس فکس علاوه بر Worker یک وب‌سرور (Kestrel) روی پورت 5002 اجرا می‌کند برای وب‌هوک Twilio و سرو فایل PDF. در فایروال پورت 5002 را باز کنید و در پنل Twilio آدرس وب‌هوک را تنظیم کنید.

```powershell
dotnet publish FaxService\Automation.FaxService.csproj -c Release
sc create "Automation.FaxService" binPath= "مسیر\Automation.Services\FaxService\bin\Release\net9.0\Automation.FaxService.exe"
sc start Automation.FaxService
```

---

## ساخت و اجرا

```powershell
# ساخت هر دو سرویس
dotnet build Automation.Services.sln

# اجرای سرویس ایمیل (تست)
dotnet run --project EmailService\Automation.EmailService.csproj

# اجرای سرویس فکس (تست)
dotnet run --project FaxService\Automation.FaxService.csproj
```

پیش از نصب، در `appsettings.json` (و در صورت نیاز `appsettings.Development.json`) مقادیر واقعی SMTP/IMAP و Twilio را تنظیم کنید.

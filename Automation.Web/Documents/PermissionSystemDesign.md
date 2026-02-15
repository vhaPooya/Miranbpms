# طراحی سیستم مجوزدهی (Permission System Design)
## سیستم اتوماسیون اداری

---

## 📋 فهرست مطالب
1. [مقدمه](#مقدمه)
2. [ساختار کلی مجوزها](#ساختار-کلی-مجوزها)
3. [عملگرها (Action Types)](#عملگرها-action-types)
4. [مجوزهای مربوط به فرم‌ساز](#مجوزهای-مربوط-به-فرمساز)
5. [پیوست و عطف مدارک](#پیوست-و-عطف-مدارک)
6. [دکمه‌های استاندارد فرم](#دکمههای-استاندارد-فرم)
7. [مجوزهای مربوط به کارتابل‌ها](#مجوزهای-مربوط-به-کارتابلها)
8. [مجوزهای مربوط به دبیرخانه](#مجوزهای-مربوط-به-دبیرخانه)
9. [مجوزهای مربوط به گردش کار](#مجوزهای-مربوط-به-گردش-کار)
10. [مجوزهای مربوط به جستجو و گزارش](#مجوزهای-مربوط-به-جستجو-و-گزارش)
11. [مجوزهای مربوط به تنظیمات](#مجوزهای-مربوط-به-تنظیمات)
12. [پیاده‌سازی پیشنهادی](#پیادهسازی-پیشنهادی)
13. [جداول و ساختار دیتابیس](#جداول-و-ساختار-دیتابیس)

---

## 🎯 مقدمه

سیستم مجوزدهی باید بر اساس **Role-Based Access Control (RBAC)** پیاده‌سازی شود که در آن:
- هر کاربر می‌تواند یک یا چند نقش داشته باشد
- هر نقش می‌تواند یک یا چند مجوز داشته باشد
- مجوزها در سطح **عملیات** و **منابع** تعریف می‌شوند
- دسترسی به فرم‌ها می‌تواند در سطح **فرم کامل** یا **بخش‌های خاص** باشد

---

## 🏗️ ساختار کلی مجوزها

### فرمت کد مجوز
```
[Category]_[Resource]_[Action]
```

مثال:
- `FORM_DESIGN_CREATE` - طراحی فرم جدید
- `FORM_DATA_VIEW` - مشاهده اطلاعات فرم
- `CABINET_INBOX_VIEW` - مشاهده کارتابل ورودی

### دسته‌بندی‌های اصلی (Categories)

1. **FORM** - مجوزهای مربوط به فرم‌ها
2. **CABINET** - مجوزهای مربوط به کارتابل‌ها
3. **WORKFLOW** - مجوزهای مربوط به گردش کار
4. **ARCHIVE** - مجوزهای مربوط به دبیرخانه
5. **SEARCH** - مجوزهای مربوط به جستجو
6. **REPORT** - مجوزهای مربوط به گزارش‌ها
7. **SETTINGS** - مجوزهای مربوط به تنظیمات
8. **DASHBOARD** - مجوزهای مربوط به داشبورد

---

## ⚡ عملگرها (Action Types)

عملگرها یا Action Types برای مشخص کردن **دلیل ارجاع** یک مدرک به گیرنده آن استفاده می‌شوند. هر عملگر رفتار خاصی در قفل/باز بودن مدرک دارد.

### انواع عملگرها

| کد عملگر | نام عملگر | قفل/باز | توضیحات |
|----------|-----------|---------|---------|
| `ACTION_FOR_SIGNATURE` | جهت امضاء | **باز** | نامه برای امضاء ارجاع می‌شود و باید قابل ویرایش باشد |
| `ACTION_FOR_REVIEW` | جهت بررسی | **قفل** | نامه فقط برای بررسی و مشاهده ارجاع می‌شود |
| `ACTION_FOR_ORDER` | جهت دستور | **قفل** | نامه برای دریافت دستور ارجاع می‌شود |
| `ACTION_FOR_OUTGOING` | جهت ثبت صادره | **باز** | نامه برای ثبت به عنوان صادره ارجاع می‌شود |
| `ACTION_FOR_ARCHIVE` | جهت بایگانی | **قفل** | نامه برای بایگانی ارجاع می‌شود |
| `ACTION_FOR_EDIT` | جهت اصلاح و ویرایش | **باز** | نامه برای اصلاح و ویرایش ارجاع می‌شود |
| `ACTION_FOR_VIEW_COPY` | جهت مشاهده رونوشت | **قفل** | نامه فقط برای مشاهده رونوشت ارجاع می‌شود |

### منطق قفل/باز بودن

#### عملگرهای باز (Editable)
- `ACTION_FOR_SIGNATURE` - جهت امضاء
- `ACTION_FOR_OUTGOING` - جهت ثبت صادره
- `ACTION_FOR_EDIT` - جهت اصلاح و ویرایش

**رفتار:** نامه در این حالت **قابل ویرایش** است و کاربر می‌تواند تغییرات را اعمال کند.

#### عملگرهای قفل (Read-Only)
- `ACTION_FOR_REVIEW` - جهت بررسی
- `ACTION_FOR_ORDER` - جهت دستور
- `ACTION_FOR_ARCHIVE` - جهت بایگانی
- `ACTION_FOR_VIEW_COPY` - جهت مشاهده رونوشت

**رفتار:** نامه در این حالت **فقط خواندنی** است و کاربر نمی‌تواند تغییراتی اعمال کند.

### قفل خودکار بعد از امضاء

**قانون مهم:** بعد از امضاء کردن یک نامه/فرم، نامه **حتما باید قفل شود** و دیگر قابل ویرایش نباشد (مگر برای نقش‌های خاص با مجوز `FORM_SIGNED_EDIT`).

### جدول ActionTypes در دیتابیس

```sql
ActionTypes
- Id (Guid, PK)
- ActionCode (nvarchar(50), Unique) -- مثل ACTION_FOR_SIGNATURE
- ActionNameFa (nvarchar(200)) -- مثل "جهت امضاء"
- ActionNameEn (nvarchar(200)) -- مثل "For Signature"
- IsEditable (bit) -- true = باز, false = قفل
- Description (nvarchar(500))
- DisplayOrder (int)
- IsActive (bit)
- IsDeleted (bit)
- CreationDate, EditDate, CreatorUserId, CreatorRoleId
```

### مجوزهای مربوط به عملگرها

| کد مجوز | نام مجوز | توضیحات |
|---------|----------|---------|
| `ACTION_TYPE_VIEW` | مشاهده عملگرها | امکان مشاهده لیست عملگرها |
| `ACTION_TYPE_MANAGE` | مدیریت عملگرها | امکان ایجاد/ویرایش/حذف عملگر |

---

## 📝 مجوزهای مربوط به فرم‌ساز

### 1. طراحی و ساختار فرم

| کد مجوز | نام مجوز | توضیحات |
|---------|----------|---------|
| `FORM_DESIGN_CREATE` | طراحی فرم جدید | امکان ایجاد فرم جدید در فرم‌ساز |
| `FORM_DESIGN_EDIT` | ویرایش ساختار فرم | امکان ویرایش ساختار و کنترل‌های یک فرم |
| `FORM_DESIGN_DELETE` | حذف فرم | امکان حذف فرم از سیستم |
| `FORM_DESIGN_VIEW` | مشاهده فرم‌ها | امکان مشاهده لیست فرم‌های طراحی شده |
| `FORM_DESIGN_PUBLISH` | انتشار فرم | امکان انتشار فرم برای استفاده |
| `FORM_DESIGN_UNPUBLISH` | لغو انتشار | امکان لغو انتشار فرم |

### 2. ثبت و ویرایش اطلاعات فرم

| کد مجوز | نام مجوز | توضیحات |
|---------|----------|---------|
| `FORM_DATA_CREATE` | ثبت اطلاعات فرم | امکان ثبت اطلاعات جدید در یک فرم |
| `FORM_DATA_VIEW` | مشاهده اطلاعات فرم | امکان مشاهده اطلاعات ثبت شده در فرم |
| `FORM_DATA_EDIT` | ویرایش اطلاعات فرم | امکان ویرایش اطلاعات ثبت شده (قبل از امضاء) |
| `FORM_DATA_DELETE` | حذف اطلاعات فرم | امکان حذف اطلاعات ثبت شده (قبل از امضاء) |
| `FORM_DATA_PRINT` | استفاده از قالب چاپ | امکان چاپ فرم با استفاده از قالب‌های تعریف شده |
| `FORM_DATA_EXPORT` | خروجی اطلاعات | امکان خروجی گرفتن از اطلاعات فرم (Excel, PDF, ...) |

### 3. پیوست و عطف

#### پیوست (Attachment)

| کد مجوز | نام مجوز | توضیحات |
|---------|----------|---------|
| `FORM_ATTACHMENT_UPLOAD` | پیوست کردن مدرک خارجی | امکان آپلود فایل خارج از سیستم به عنوان پیوست |
| `FORM_ATTACHMENT_ATTACH_DOCUMENT` | پیوست کردن مدرک داخلی | امکان پیوست کردن یک مدرک/نامه از سیستم به فرم |
| `FORM_ATTACHMENT_VIEW` | مشاهده پیوست | امکان مشاهده و دانلود پیوست‌های فرم |
| `FORM_ATTACHMENT_DELETE` | حذف پیوست | امکان حذف پیوست‌های فرم (قبل از امضاء) |

#### عطف (Reference)

| کد مجوز | نام مجوز | توضیحات |
|---------|----------|---------|
| `FORM_REFERENCE_ATTACH_DOCUMENT` | عطف کردن مدرک داخلی | امکان عطف کردن یک مدرک/نامه از سیستم به فرم |
| `FORM_REFERENCE_ATTACH_EXTERNAL` | عطف کردن مدرک خارجی | امکان عطف کردن فایل خارج از سیستم |
| `FORM_REFERENCE_VIEW` | مشاهده عطف | امکان مشاهده و دانلود عطف‌های فرم |
| `FORM_REFERENCE_DELETE` | حذف عطف | امکان حذف عطف‌های فرم (قبل از امضاء) |

**تفاوت پیوست و عطف:**
- **پیوست (Attachment):** مدرکی که به عنوان ضمیمه به فرم اضافه می‌شود
- **عطف (Reference):** مدرکی که به عنوان مرجع یا ارتباط با مدرک دیگر به فرم اضافه می‌شود

### 4. امضاء و قفل اطلاعات

| کد مجوز | نام مجوز | توضیحات |
|---------|----------|---------|
| `FORM_SIGN` | امضاء فرم | امکان امضاء کردن فرم (قفل شدن اطلاعات) |
| `FORM_SIGNED_VIEW` | مشاهده فرم امضاء شده | امکان مشاهده فرم‌های امضاء شده (بدون امکان ویرایش) |
| `FORM_SIGNED_EDIT` | ویرایش فرم امضاء شده | امکان ویرایش فرم‌های امضاء شده (فقط برای نقش‌های خاص) |
| `FORM_SIGN_CANCEL` | لغو امضاء | امکان لغو امضاء فرم (فقط برای نقش‌های خاص) |

### 5. مجوزهای سطحی (Field-Level Permissions)

برای کنترل دسترسی به بخش‌های خاص یک فرم:

| کد مجوز | نام مجوز | توضیحات |
|---------|----------|---------|
| `FORM_FIELD_VIEW_[FieldName]` | مشاهده فیلد خاص | مشاهده یک فیلد خاص در فرم |
| `FORM_FIELD_EDIT_[FieldName]` | ویرایش فیلد خاص | ویرایش یک فیلد خاص در فرم |

**نکته:** این مجوزها به صورت داینامیک برای هر فیلد ایجاد می‌شوند.

---

## 🔘 دکمه‌های استاندارد فرم

هر فرم طراحی شده در سیستم باید دارای دکمه‌های استاندارد زیر باشد که بر اساس مجوزهای کاربر نمایش داده می‌شوند:

### لیست دکمه‌های استاندارد

| دکمه | کد مجوز | نمایش در حالت | توضیحات |
|------|---------|---------------|---------|
| **ذخیره** | `FORM_DATA_SAVE` | ایجاد/ویرایش | ذخیره اطلاعات فرم (قبل از امضاء) |
| **ثبت** | `FORM_DATA_SUBMIT` | ایجاد/ویرایش | ثبت نهایی اطلاعات فرم |
| **پیوست** | `FORM_ATTACHMENT_UPLOAD` | همه حالات | باز کردن مدال پیوست کردن مدرک |
| **عطف** | `FORM_REFERENCE_ATTACH_DOCUMENT` | همه حالات | باز کردن مدال عطف کردن مدرک |
| **قالب چاپ** | `FORM_DATA_PRINT_TEMPLATE` | همه حالات | باز کردن لیست قالب‌های چاپ |
| **ثبت تغییرات** | `FORM_DATA_SAVE_CHANGES` | ویرایش | ذخیره تغییرات اعمال شده |
| **ارجاع** | `WORKFLOW_ASSIGN` | همه حالات | ارجاع فرم به کاربر/نقش دیگر |
| **مشاهده گردش** | `WORKFLOW_VIEW` | همه حالات | مشاهده گردش کار فرم |
| **امضاء** | `FORM_SIGN` | قبل از امضاء | امضاء کردن فرم (قفل شدن) |
| **بستن** | - | همه حالات | بستن فرم و بازگشت به لیست |

### منطق نمایش دکمه‌ها

#### حالت ایجاد (Create Mode)
- ✅ ذخیره (`FORM_DATA_SAVE`)
- ✅ ثبت (`FORM_DATA_SUBMIT`)
- ✅ پیوست (`FORM_ATTACHMENT_UPLOAD`)
- ✅ عطف (`FORM_REFERENCE_ATTACH_DOCUMENT`)
- ✅ قالب چاپ (`FORM_DATA_PRINT_TEMPLATE`)
- ✅ ارجاع (`WORKFLOW_ASSIGN`)
- ✅ مشاهده گردش (`WORKFLOW_VIEW`) - اگر در گردش باشد
- ✅ بستن

#### حالت ویرایش (Edit Mode - قبل از امضاء)
- ✅ ثبت تغییرات (`FORM_DATA_SAVE_CHANGES`)
- ✅ پیوست (`FORM_ATTACHMENT_UPLOAD`)
- ✅ عطف (`FORM_REFERENCE_ATTACH_DOCUMENT`)
- ✅ قالب چاپ (`FORM_DATA_PRINT_TEMPLATE`)
- ✅ ارجاع (`WORKFLOW_ASSIGN`)
- ✅ مشاهده گردش (`WORKFLOW_VIEW`)
- ✅ امضاء (`FORM_SIGN`)
- ✅ بستن

#### حالت مشاهده (View Mode - بعد از امضاء یا با Action Type قفل)
- ❌ ثبت تغییرات
- ❌ امضاء
- ✅ پیوست (`FORM_ATTACHMENT_VIEW`) - فقط مشاهده
- ✅ عطف (`FORM_REFERENCE_VIEW`) - فقط مشاهده
- ✅ قالب چاپ (`FORM_DATA_PRINT_TEMPLATE`)
- ✅ ارجاع (`WORKFLOW_ASSIGN`) - اگر مجوز داشته باشد
- ✅ مشاهده گردش (`WORKFLOW_VIEW`)
- ✅ بستن

### بررسی مجوز برای نمایش دکمه‌ها

```javascript
// مثال: بررسی مجوز برای نمایش دکمه
async function checkButtonPermissions() {
    const permissions = await getUserPermissions();
    
    // دکمه ثبت تغییرات
    if (permissions.includes('FORM_DATA_SAVE_CHANGES') && !isSigned) {
        document.getElementById('btn-save-changes').style.display = 'block';
    }
    
    // دکمه پیوست
    if (permissions.includes('FORM_ATTACHMENT_UPLOAD') && !isSigned) {
        document.getElementById('btn-attachment').style.display = 'block';
    } else if (permissions.includes('FORM_ATTACHMENT_VIEW')) {
        document.getElementById('btn-attachment').style.display = 'block';
        document.getElementById('btn-attachment').disabled = true; // فقط مشاهده
    }
    
    // دکمه قالب چاپ
    if (permissions.includes('FORM_DATA_PRINT_TEMPLATE')) {
        document.getElementById('btn-print-template').style.display = 'block';
    }
    
    // دکمه ارجاع
    if (permissions.includes('WORKFLOW_ASSIGN')) {
        document.getElementById('btn-assign').style.display = 'block';
    }
    
    // دکمه مشاهده گردش
    if (permissions.includes('WORKFLOW_VIEW')) {
        document.getElementById('btn-workflow').style.display = 'block';
    }
    
    // دکمه امضاء
    if (permissions.includes('FORM_SIGN') && !isSigned) {
        document.getElementById('btn-sign').style.display = 'block';
    }
}
```

### چیدمان دکمه‌ها در UI

```
┌─────────────────────────────────────────────────────────┐
│  [ذخیره] [ثبت] [پیوست] [عطف] [قالب چاپ] [ثبت تغییرات]  │
│  [ارجاع] [مشاهده گردش] [امضاء] [بستن]                   │
└─────────────────────────────────────────────────────────┘
```

**نکته:** دکمه‌ها باید به صورت **Responsive** و با استفاده از **Bootstrap Button Group** یا **Toolbar** نمایش داده شوند.

---

## 📬 مجوزهای مربوط به کارتابل‌ها

### 1. کارتابل ورودی

| کد مجوز | نام مجوز | توضیحات |
|---------|----------|---------|
| `CABINET_INBOX_VIEW` | مشاهده کارتابل ورودی | امکان مشاهده نامه‌ها و فرم‌های دریافتی |
| `CABINET_INBOX_DETAIL` | مشاهده جزئیات ورودی | امکان مشاهده جزئیات کامل نامه/فرم دریافتی |
| `CABINET_INBOX_ACTION` | اقدام روی ورودی | امکان انجام اقدام (ارجاع، پاسخ، ...) روی ورودی |

### 2. کارتابل ارجاعی

| کد مجوز | نام مجوز | توضیحات |
|---------|----------|---------|
| `CABINET_REFERRED_VIEW` | مشاهده کارتابل ارجاعی | امکان مشاهده نامه‌ها و فرم‌های ارجاع داده شده |
| `CABINET_REFERRED_DETAIL` | مشاهده جزئیات ارجاعی | امکان مشاهده جزئیات کامل نامه/فرم ارجاعی |
| `CABINET_REFERRED_EDIT` | ویرایش ارجاع | امکان ویرایش یا لغو ارجاع |

### 3. کارتابل صادره

| کد مجوز | نام مجوز | توضیحات |
|---------|----------|---------|
| `CABINET_OUTBOX_VIEW` | مشاهده کارتابل صادره | امکان مشاهده نامه‌ها و فرم‌های صادره |
| `CABINET_OUTBOX_DETAIL` | مشاهده جزئیات صادره | امکان مشاهده جزئیات کامل نامه/فرم صادره |
| `CABINET_OUTBOX_CREATE` | ایجاد نامه صادره | امکان ایجاد نامه/فرم صادره جدید |

### 4. کارتابل داخلی

| کد مجوز | نام مجوز | توضیحات |
|---------|----------|---------|
| `CABINET_INTERNAL_VIEW` | مشاهده کارتابل داخلی | امکان مشاهده نامه‌ها و فرم‌های داخلی |
| `CABINET_INTERNAL_DETAIL` | مشاهده جزئیات داخلی | امکان مشاهده جزئیات کامل نامه/فرم داخلی |
| `CABINET_INTERNAL_CREATE` | ایجاد نامه داخلی | امکان ایجاد نامه/فرم داخلی جدید |

### 5. کارتابل شخصی

| کد مجوز | نام مجوز | توضیحات |
|---------|----------|---------|
| `CABINET_PERSONAL_VIEW` | مشاهده کارتابل شخصی | امکان مشاهده نامه‌ها و فرم‌های شخصی |
| `CABINET_PERSONAL_DETAIL` | مشاهده جزئیات شخصی | امکان مشاهده جزئیات کامل نامه/فرم شخصی |

---

## 🔄 مجوزهای مربوط به گردش کار

| کد مجوز | نام مجوز | توضیحات |
|---------|----------|---------|
| `WORKFLOW_VIEW` | مشاهده گردش کار | امکان مشاهده گردش کار یک فرم/نامه |
| `WORKFLOW_CREATE` | ایجاد گردش کار | امکان تعریف گردش کار جدید |
| `WORKFLOW_EDIT` | ویرایش گردش کار | امکان ویرایش گردش کار تعریف شده |
| `WORKFLOW_DELETE` | حذف گردش کار | امکان حذف گردش کار |
| `WORKFLOW_ASSIGN` | ارجاع در گردش | امکان ارجاع نامه/فرم به کاربر دیگر در گردش |
| `WORKFLOW_APPROVE` | تائید در گردش | امکان تائید در مرحله گردش |
| `WORKFLOW_REJECT` | رد در گردش | امکان رد در مرحله گردش |
| `WORKFLOW_RETURN` | برگشت در گردش | امکان برگشت به مرحله قبلی |
| `WORKFLOW_CANCEL` | لغو گردش | امکان لغو گردش کار |

### دسترسی به مدارک در گردش

| کد مجوز | نام مجوز | توضیحات |
|---------|----------|---------|
| `WORKFLOW_DOCUMENT_ACCESS` | دسترسی به مدرک در گردش | فقط افرادی که در گردش هستند می‌توانند به مدرک دسترسی داشته باشند |
| `WORKFLOW_HISTORY_VIEW` | مشاهده تاریخچه گردش | امکان مشاهده تاریخچه کامل گردش کار |

---

## 📚 مجوزهای مربوط به دبیرخانه

| کد مجوز | نام مجوز | توضیحات |
|---------|----------|---------|
| `ARCHIVE_VIEW` | مشاهده دبیرخانه | امکان مشاهده مدارک بایگانی شده |
| `ARCHIVE_CREATE` | بایگانی مدرک | امکان بایگانی کردن مدرک |
| `ARCHIVE_EDIT` | ویرایش بایگانی | امکان ویرایش اطلاعات بایگانی |
| `ARCHIVE_DELETE` | حذف از بایگانی | امکان حذف مدرک از بایگانی |
| `ARCHIVE_CLASSIFY` | طبقه‌بندی | امکان طبقه‌بندی مدارک |
| `ARCHIVE_SEARCH` | جستجو در بایگانی | امکان جستجو در مدارک بایگانی شده |
| `ARCHIVE_EXPORT` | خروجی بایگانی | امکان خروجی گرفتن از بایگانی |

---

## 🔍 مجوزهای مربوط به جستجو و گزارش

### جستجو

| کد مجوز | نام مجوز | توضیحات |
|---------|----------|---------|
| `SEARCH_GLOBAL` | جستجوی سراسری | امکان جستجو در تمام سیستم |
| `SEARCH_FORMS` | جستجو در فرم‌ها | امکان جستجو در فرم‌ها |
| `SEARCH_DOCUMENTS` | جستجو در مدارک | امکان جستجو در مدارک |
| `SEARCH_ADVANCED` | جستجوی پیشرفته | امکان استفاده از جستجوی پیشرفته |
| `SEARCH_EXPORT` | خروجی نتایج جستجو | امکان خروجی گرفتن از نتایج جستجو |

### گزارش‌ها

| کد مجوز | نام مجوز | توضیحات |
|---------|----------|---------|
| `REPORT_VIEW` | مشاهده گزارش‌ها | امکان مشاهده گزارش‌های تعریف شده |
| `REPORT_CREATE` | ایجاد گزارش | امکان ایجاد گزارش جدید |
| `REPORT_EDIT` | ویرایش گزارش | امکان ویرایش گزارش |
| `REPORT_DELETE` | حذف گزارش | امکان حذف گزارش |
| `REPORT_EXECUTE` | اجرای گزارش | امکان اجرا و مشاهده نتایج گزارش |
| `REPORT_EXPORT` | خروجی گزارش | امکان خروجی گرفتن از گزارش |

---

## ⚙️ مجوزهای مربوط به تنظیمات

| کد مجوز | نام مجوز | توضیحات |
|---------|----------|---------|
| `SETTINGS_USERS_VIEW` | مشاهده کاربران | امکان مشاهده لیست کاربران |
| `SETTINGS_USERS_CREATE` | ایجاد کاربر | امکان ایجاد کاربر جدید |
| `SETTINGS_USERS_EDIT` | ویرایش کاربر | امکان ویرایش اطلاعات کاربر |
| `SETTINGS_USERS_DELETE` | حذف کاربر | امکان حذف کاربر |
| `SETTINGS_ROLES_VIEW` | مشاهده نقش‌ها | امکان مشاهده لیست نقش‌ها |
| `SETTINGS_ROLES_CREATE` | ایجاد نقش | امکان ایجاد نقش جدید |
| `SETTINGS_ROLES_EDIT` | ویرایش نقش | امکان ویرایش نقش |
| `SETTINGS_ROLES_DELETE` | حذف نقش | امکان حذف نقش |
| `SETTINGS_ORGANIZATIONS_VIEW` | مشاهده سازمان‌ها | امکان مشاهده لیست سازمان‌ها |
| `SETTINGS_ORGANIZATIONS_MANAGE` | مدیریت سازمان‌ها | امکان ایجاد/ویرایش/حذف سازمان |
| `SETTINGS_DEPARTMENTS_VIEW` | مشاهده واحدها | امکان مشاهده لیست واحدها |
| `SETTINGS_DEPARTMENTS_MANAGE` | مدیریت واحدها | امکان ایجاد/ویرایش/حذف واحد |
| `SETTINGS_PERMISSIONS_VIEW` | مشاهده مجوزها | امکان مشاهده لیست مجوزها |
| `SETTINGS_PERMISSIONS_MANAGE` | مدیریت مجوزها | امکان ایجاد/ویرایش/حذف مجوز |
| `SETTINGS_SYSTEM_VIEW` | مشاهده تنظیمات سیستم | امکان مشاهده تنظیمات سیستم |
| `SETTINGS_SYSTEM_EDIT` | ویرایش تنظیمات سیستم | امکان ویرایش تنظیمات سیستم |

---

## 📊 مجوزهای مربوط به داشبورد

| کد مجوز | نام مجوز | توضیحات |
|---------|----------|---------|
| `DASHBOARD_VIEW` | مشاهده داشبورد | امکان مشاهده داشبورد اصلی |
| `DASHBOARD_INBOX_CARD` | کارت کارتابل ورودی | نمایش کارت کارتابل ورودی در داشبورد |
| `DASHBOARD_REFERRED_CARD` | کارت کارتابل ارجاعی | نمایش کارت کارتابل ارجاعی در داشبورد |
| `DASHBOARD_OUTBOX_CARD` | کارت کارتابل صادره | نمایش کارت کارتابل صادره در داشبورد |
| `DASHBOARD_INTERNAL_CARD` | کارت کارتابل داخلی | نمایش کارت کارتابل داخلی در داشبورد |
| `DASHBOARD_STATISTICS` | آمار و نمودارها | نمایش آمار و نمودارها در داشبورد |
| `DASHBOARD_NOTIFICATIONS` | اعلان‌ها | نمایش اعلان‌ها در داشبورد |

---

## 🏛️ پیاده‌سازی پیشنهادی

### 1. ساختار جداول دیتابیس

#### جدول Permissions (موجود)
```sql
Permissions
- Id (Guid, PK)
- PermissionCode (nvarchar(100), Unique)
- PermissionName (nvarchar(200))
- Description (nvarchar(500))
- Category (nvarchar(100))
- IsActive (bit)
- IsDeleted (bit)
- CreationDate, EditDate, CreatorUserId, CreatorRoleId
```

#### جدول FormPermissions (جدید - برای مجوزهای سطح فرم)
```sql
FormPermissions
- Id (Guid, PK)
- FormId (Guid, FK -> Forms)
- PermissionId (Guid, FK -> Permissions)
- RoleId (Guid, FK -> Roles)
- FieldName (nvarchar(200), Nullable) -- برای مجوزهای سطح فیلد
- IsGranted (bit) -- true = مجوز دارد, false = مجوز ندارد
- CreationDate, EditDate, CreatorUserId, CreatorRoleId
```

#### جدول WorkflowPermissions (جدید - برای مجوزهای گردش کار)
```sql
WorkflowPermissions
- Id (Guid, PK)
- WorkflowId (Guid, FK -> Workflows)
- RoleId (Guid, FK -> Roles)
- PermissionId (Guid, FK -> Permissions)
- StageId (Guid, FK -> WorkflowStages, Nullable) -- برای مجوزهای سطح مرحله
- IsGranted (bit)
- CreationDate, EditDate, CreatorUserId, CreatorRoleId
```

#### جدول DocumentAttachments (جدید - برای پیوست‌ها)
```sql
DocumentAttachments
- Id (Guid, PK)
- DocumentId (Guid, FK -> FormData/Documents)
- AttachmentType (nvarchar(50)) -- 'EXTERNAL_FILE' یا 'INTERNAL_DOCUMENT'
- FileName (nvarchar(500)) -- برای فایل خارجی
- FilePath (nvarchar(1000)) -- برای فایل خارجی
- FileSize (bigint) -- برای فایل خارجی
- MimeType (nvarchar(100)) -- برای فایل خارجی
- ReferencedDocumentId (Guid, FK -> FormData/Documents, Nullable) -- برای مدرک داخلی
- ReferencedFormId (Guid, FK -> Forms, Nullable) -- برای مدرک داخلی
- AttachmentCategory (nvarchar(50)) -- 'ATTACHMENT' یا 'REFERENCE'
- Description (nvarchar(1000))
- UploadedBy (Guid, FK -> Users)
- UploadedDate (datetime)
- IsActive (bit)
- IsDeleted (bit)
- CreationDate, EditDate, CreatorUserId, CreatorRoleId
```

#### جدول DocumentReferrals (جدید - برای ارجاع‌ها)
```sql
DocumentReferrals
- Id (Guid, PK)
- DocumentId (Guid, FK -> FormData/Documents)
- ActionTypeId (Guid, FK -> ActionTypes) -- نوع عملگر
- ReferredToUserId (Guid, FK -> Users, Nullable)
- ReferredToRoleId (Guid, FK -> Roles, Nullable)
- ReferredToDepartmentId (Guid, FK -> Departments, Nullable)
- ReferredByUserId (Guid, FK -> Users)
- ReferredDate (datetime)
- DueDate (datetime, Nullable)
- Status (nvarchar(50)) -- 'PENDING', 'COMPLETED', 'CANCELLED'
- Notes (nvarchar(2000))
- IsRead (bit)
- ReadDate (datetime, Nullable)
- IsActive (bit)
- IsDeleted (bit)
- CreationDate, EditDate, CreatorUserId, CreatorRoleId
```

### 2. سرویس بررسی مجوز (Permission Service)

```csharp
public interface IPermissionService
{
    Task<bool> HasPermissionAsync(Guid userId, string permissionCode);
    Task<bool> HasPermissionAsync(Guid userId, string permissionCode, Guid? resourceId);
    Task<bool> HasFormPermissionAsync(Guid userId, Guid formId, string permissionCode);
    Task<bool> HasFieldPermissionAsync(Guid userId, Guid formId, string fieldName, string permissionCode);
    Task<bool> HasWorkflowPermissionAsync(Guid userId, Guid workflowId, string permissionCode);
    Task<bool> CanAccessDocumentAsync(Guid userId, Guid documentId);
    Task<bool> CanEditDocumentAsync(Guid userId, Guid documentId);
    Task<bool> IsDocumentLockedAsync(Guid documentId);
    Task<List<string>> GetUserPermissionsAsync(Guid userId);
    Task<List<string>> GetRolePermissionsAsync(Guid roleId);
    Task<ActionType> GetActionTypeByCodeAsync(string actionCode);
}
```

### 3. سرویس مدیریت مدارک (Document Service)

```csharp
public interface IDocumentService
{
    Task<bool> AttachExternalFileAsync(Guid documentId, IFormFile file, string description);
    Task<bool> AttachInternalDocumentAsync(Guid documentId, Guid referencedDocumentId, string category);
    Task<bool> ReferenceDocumentAsync(Guid documentId, Guid referencedDocumentId, string description);
    Task<List<DocumentAttachment>> GetDocumentAttachmentsAsync(Guid documentId);
    Task<List<DocumentAttachment>> GetDocumentReferencesAsync(Guid documentId);
    Task<bool> DeleteAttachmentAsync(Guid attachmentId, Guid userId);
    Task<bool> SignDocumentAsync(Guid documentId, Guid userId);
    Task<bool> IsDocumentSignedAsync(Guid documentId);
    Task<bool> ReferDocumentAsync(Guid documentId, Guid actionTypeId, Guid? userId, Guid? roleId, Guid? departmentId, string notes);
}
```

### 4. منطق قفل/باز بودن مدرک

```csharp
public async Task<bool> CanEditDocumentAsync(Guid userId, Guid documentId)
{
    var document = await _context.Documents
        .Include(d => d.Referrals)
            .ThenInclude(r => r.ActionType)
        .FirstOrDefaultAsync(d => d.Id == documentId);

    if (document == null) return false;

    // 1. بررسی آیا مدرک امضاء شده است؟
    if (document.IsSigned)
    {
        // فقط نقش‌های خاص می‌توانند فرم امضاء شده را ویرایش کنند
        return await HasPermissionAsync(userId, "FORM_SIGNED_EDIT");
    }

    // 2. بررسی Action Type در ارجاع فعلی
    var currentReferral = document.Referrals
        .Where(r => r.Status == "PENDING" && 
                    (r.ReferredToUserId == userId || 
                     r.ReferredToRoleId.HasValue && await UserHasRoleAsync(userId, r.ReferredToRoleId.Value)))
        .OrderByDescending(r => r.ReferredDate)
        .FirstOrDefault();

    if (currentReferral != null && currentReferral.ActionType != null)
    {
        // اگر Action Type قفل باشد، مدرک قابل ویرایش نیست
        return currentReferral.ActionType.IsEditable;
    }

    // 3. بررسی مجوزهای عادی
    return await HasFormPermissionAsync(userId, document.FormId, "FORM_DATA_EDIT");
}
```

### 5. Attribute برای بررسی مجوز

```csharp
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequirePermissionAttribute : Attribute
{
    public string PermissionCode { get; }
    public bool CheckResource { get; set; } = false;
    
    public RequirePermissionAttribute(string permissionCode)
    {
        PermissionCode = permissionCode;
    }
}

// استفاده:
[RequirePermission("FORM_DATA_CREATE")]
public async Task<IActionResult> CreateFormData(...)
{
    // ...
}
```

### 6. Middleware برای بررسی مجوز

```csharp
public class PermissionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IPermissionService _permissionService;

    public PermissionMiddleware(RequestDelegate next, IPermissionService permissionService)
    {
        _next = next;
        _permissionService = permissionService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // بررسی مجوزها قبل از اجرای Action
        // ...
        await _next(context);
    }
}
```

### 7. JavaScript Helper برای Frontend

```javascript
// بررسی مجوز در Frontend
async function hasPermission(permissionCode) {
    const response = await fetch(`/api/permissions/check?code=${permissionCode}`);
    const result = await response.json();
    return result.hasPermission;
}

// استفاده:
if (await hasPermission('FORM_DATA_CREATE')) {
    // نمایش دکمه ایجاد
}

// بررسی آیا مدرک قابل ویرایش است؟
async function canEditDocument(documentId) {
    const response = await fetch(`/api/documents/${documentId}/can-edit`);
    const result = await response.json();
    return result.canEdit;
}

// دریافت لیست مجوزهای کاربر
async function getUserPermissions() {
    const response = await fetch('/api/permissions/user-permissions');
    const result = await response.json();
    return result.permissions || [];
}

// بررسی و نمایش/مخفی کردن دکمه‌ها
async function initializeFormButtons(documentId, isSigned) {
    const permissions = await getUserPermissions();
    const canEdit = !isSigned && await canEditDocument(documentId);
    
    // دکمه ذخیره
    if (permissions.includes('FORM_DATA_SAVE') && canEdit) {
        document.getElementById('btn-save').style.display = 'block';
    }
    
    // دکمه ثبت
    if (permissions.includes('FORM_DATA_SUBMIT') && canEdit) {
        document.getElementById('btn-submit').style.display = 'block';
    }
    
    // دکمه پیوست
    if (permissions.includes('FORM_ATTACHMENT_UPLOAD') && canEdit) {
        document.getElementById('btn-attachment').style.display = 'block';
    } else if (permissions.includes('FORM_ATTACHMENT_VIEW')) {
        document.getElementById('btn-attachment').style.display = 'block';
        document.getElementById('btn-attachment').disabled = true;
    }
    
    // دکمه عطف
    if (permissions.includes('FORM_REFERENCE_ATTACH_DOCUMENT') && canEdit) {
        document.getElementById('btn-reference').style.display = 'block';
    } else if (permissions.includes('FORM_REFERENCE_VIEW')) {
        document.getElementById('btn-reference').style.display = 'block';
        document.getElementById('btn-reference').disabled = true;
    }
    
    // دکمه قالب چاپ
    if (permissions.includes('FORM_DATA_PRINT_TEMPLATE')) {
        document.getElementById('btn-print-template').style.display = 'block';
    }
    
    // دکمه ثبت تغییرات
    if (permissions.includes('FORM_DATA_SAVE_CHANGES') && canEdit) {
        document.getElementById('btn-save-changes').style.display = 'block';
    }
    
    // دکمه ارجاع
    if (permissions.includes('WORKFLOW_ASSIGN')) {
        document.getElementById('btn-assign').style.display = 'block';
    }
    
    // دکمه مشاهده گردش
    if (permissions.includes('WORKFLOW_VIEW')) {
        document.getElementById('btn-workflow').style.display = 'block';
    }
    
    // دکمه امضاء
    if (permissions.includes('FORM_SIGN') && !isSigned && canEdit) {
        document.getElementById('btn-sign').style.display = 'block';
    }
}
```

---

## 🔐 منطق دسترسی به مدارک در گردش

### الگوریتم بررسی دسترسی

```
1. دریافت شناسه مدرک (DocumentId)
2. بررسی آیا مدرک در گردش است؟
   - اگر بله:
     a. دریافت لیست کاربران در گردش کار
     b. بررسی آیا کاربر فعلی در لیست است؟
        - اگر بله: دسترسی مجاز
        - اگر خیر: بررسی مجوز WORKFLOW_DOCUMENT_ACCESS
   - اگر خیر:
     a. بررسی مجوزهای عادی (FORM_DATA_VIEW, ...)
3. بررسی مجوزهای سطح فرم (FormPermissions)
4. بررسی مجوزهای سطح فیلد (اگر نیاز باشد)
5. بازگشت نتیجه
```

### پیاده‌سازی

```csharp
public async Task<bool> CanAccessDocumentAsync(Guid userId, Guid documentId)
{
    var document = await _context.Documents
        .Include(d => d.Workflow)
            .ThenInclude(w => w.Stages)
                .ThenInclude(s => s.AssignedUsers)
        .FirstOrDefaultAsync(d => d.Id == documentId);

    if (document == null) return false;

    // اگر در گردش است
    if (document.Workflow != null && document.Workflow.IsActive)
    {
        // بررسی آیا کاربر در گردش است؟
        var isInWorkflow = document.Workflow.Stages
            .Any(s => s.AssignedUsers.Any(u => u.UserId == userId && !s.IsCompleted));

        if (isInWorkflow)
            return true; // دسترسی مجاز

        // اگر در گردش نیست، بررسی مجوز خاص
        return await HasPermissionAsync(userId, "WORKFLOW_DOCUMENT_ACCESS");
    }

    // اگر در گردش نیست، بررسی مجوزهای عادی
    return await HasFormPermissionAsync(userId, document.FormId, "FORM_DATA_VIEW");
}
```

---

## 📋 چک‌لیست پیاده‌سازی

### فاز 1: زیرساخت
- [ ] ایجاد جداول FormPermissions و WorkflowPermissions
- [ ] ایجاد جداول DocumentAttachments و DocumentReferrals
- [ ] ایجاد جدول ActionTypes (اگر وجود ندارد)
- [ ] ایجاد Migration برای جداول جدید
- [ ] ایجاد Interface و Service برای PermissionService
- [ ] ایجاد Interface و Service برای DocumentService
- [ ] پیاده‌سازی PermissionService
- [ ] پیاده‌سازی DocumentService

### فاز 2: Backend
- [ ] ایجاد Attribute برای بررسی مجوز
- [ ] ایجاد Middleware برای بررسی مجوز
- [ ] پیاده‌سازی منطق دسترسی به مدارک در گردش
- [ ] پیاده‌سازی منطق قفل/باز بودن بر اساس Action Type
- [ ] پیاده‌سازی منطق قفل خودکار بعد از امضاء
- [ ] ایجاد API برای پیوست و عطف
- [ ] ایجاد API برای ارجاع با Action Type
- [ ] اضافه کردن بررسی مجوز به تمام Controllerها

### فاز 3: Frontend
- [ ] ایجاد JavaScript Helper برای بررسی مجوز
- [ ] ایجاد UI برای دکمه‌های استاندارد فرم
- [ ] پیاده‌سازی منطق نمایش/مخفی کردن دکمه‌ها بر اساس مجوز
- [ ] ایجاد مدال پیوست (فایل خارجی و مدرک داخلی)
- [ ] ایجاد مدال عطف (فایل خارجی و مدرک داخلی)
- [ ] ایجاد مدال ارجاع با انتخاب Action Type
- [ ] پیاده‌سازی قفل کردن فیلدها بر اساس Action Type
- [ ] پیاده‌سازی بررسی مجوز در Componentها
- [ ] نمایش پیام خطا برای دسترسی غیرمجاز

### فاز 4: Seed Data
- [ ] ایجاد مجوزهای پایه در Seed Data
- [ ] ایجاد Action Types پیش‌فرض در Seed Data
- [ ] اختصاص مجوزهای پیش‌فرض به نقش‌ها
- [ ] تست سیستم مجوزدهی
- [ ] تست منطق قفل/باز بودن
- [ ] تست پیوست و عطف
- [ ] تست ارجاع با Action Type

---

## 🎯 نکات مهم

1. **مجوزهای سطح فرم**: برای کنترل دسترسی به فرم‌های خاص، از جدول `FormPermissions` استفاده می‌شود.

2. **مجوزهای سطح فیلد**: برای کنترل دسترسی به فیلدهای خاص یک فرم، از فیلد `FieldName` در `FormPermissions` استفاده می‌شود.

3. **دسترسی به مدارک در گردش**: فقط کاربرانی که در گردش کار هستند یا مجوز `WORKFLOW_DOCUMENT_ACCESS` دارند می‌توانند به مدرک دسترسی داشته باشند.

4. **قفل اطلاعات بعد از امضاء**: بعد از امضاء، مجوز `FORM_SIGNED_EDIT` فقط برای نقش‌های خاص (مثل مدیر سیستم) فعال می‌شود.

5. **کارتابل‌ها**: هر کارتابل مجوزهای خاص خود را دارد و فقط کاربرانی که مجوز دارند می‌توانند آن کارتابل را مشاهده کنند.

6. **داشبورد**: کارت‌های داشبورد بر اساس مجوزهای کاربر نمایش داده می‌شوند.

7. **عملگرها (Action Types)**: هر ارجاع باید دارای یک Action Type باشد که مشخص می‌کند نامه قفل است یا باز.

8. **قفل خودکار بعد از امضاء**: بعد از امضاء، نامه حتما باید قفل شود و دیگر قابل ویرایش نباشد.

9. **پیوست و عطف**: امکان پیوست/عطف کردن هم فایل خارجی و هم مدرک داخلی (نامه/فرم دیگر) وجود دارد.

10. **دکمه‌های استاندارد**: همه فرم‌ها باید دکمه‌های استاندارد (ذخیره، ثبت، پیوست، عطف، قالب چاپ، ثبت تغییرات، ارجاع، مشاهده گردش، امضاء، بستن) را داشته باشند که بر اساس مجوز نمایش داده می‌شوند.

---

## 📝 مثال‌های استفاده

### مثال 1: بررسی مجوز در Controller
```csharp
[RequirePermission("FORM_DATA_CREATE")]
[HttpPost]
public async Task<IActionResult> CreateFormData([FromBody] FormDataDto dto)
{
    // منطق ایجاد
}
```

### مثال 2: بررسی مجوز در JavaScript
```javascript
// در View
if (await hasPermission('FORM_DATA_EDIT')) {
    document.getElementById('editButton').style.display = 'block';
}
```

### مثال 3: بررسی دسترسی به مدرک
```csharp
var canAccess = await _permissionService.CanAccessDocumentAsync(userId, documentId);
if (!canAccess)
{
    return Forbid("شما دسترسی به این مدرک را ندارید");
}
```

### مثال 4: بررسی قفل/باز بودن مدرک
```csharp
var canEdit = await _permissionService.CanEditDocumentAsync(userId, documentId);
if (!canEdit)
{
    // قفل کردن تمام فیلدهای فرم
    ViewBag.IsReadOnly = true;
}
```

### مثال 5: ارجاع با Action Type
```csharp
[HttpPost]
[Route("api/documents/{documentId}/refer")]
public async Task<IActionResult> ReferDocument(Guid documentId, [FromBody] ReferDocumentDto dto)
{
    // بررسی مجوز ارجاع
    if (!await _permissionService.HasPermissionAsync(userId, "WORKFLOW_ASSIGN"))
    {
        return Forbid("شما مجوز ارجاع ندارید");
    }
    
    // بررسی Action Type
    var actionType = await _permissionService.GetActionTypeByCodeAsync(dto.ActionTypeCode);
    if (actionType == null)
    {
        return BadRequest("نوع عملگر نامعتبر است");
    }
    
    // ارجاع مدرک
    await _documentService.ReferDocumentAsync(
        documentId, 
        actionType.Id, 
        dto.ReferredToUserId, 
        dto.ReferredToRoleId, 
        dto.ReferredToDepartmentId, 
        dto.Notes
    );
    
    return Ok(new { success = true });
}
```

### مثال 6: پیوست کردن مدرک داخلی
```csharp
[HttpPost]
[Route("api/documents/{documentId}/attach-internal")]
public async Task<IActionResult> AttachInternalDocument(Guid documentId, [FromBody] AttachInternalDto dto)
{
    // بررسی مجوز
    if (!await _permissionService.HasPermissionAsync(userId, "FORM_ATTACHMENT_ATTACH_DOCUMENT"))
    {
        return Forbid("شما مجوز پیوست کردن ندارید");
    }
    
    // بررسی قفل بودن
    var canEdit = await _permissionService.CanEditDocumentAsync(userId, documentId);
    if (!canEdit)
    {
        return BadRequest("مدرک قفل است و قابل ویرایش نیست");
    }
    
    await _documentService.AttachInternalDocumentAsync(documentId, dto.ReferencedDocumentId, "ATTACHMENT");
    return Ok(new { success = true });
}
```

---

## 🔄 به‌روزرسانی‌های آینده

1. **مجوزهای زمان‌بندی شده**: مجوزهایی که در زمان خاصی فعال/غیرفعال می‌شوند
2. **مجوزهای شرطی**: مجوزهایی که بر اساس شرایط خاص (مثل مقدار فیلد) فعال می‌شوند
3. **لاگ دسترسی‌ها**: ثبت تمام دسترسی‌ها برای Audit
4. **مجوزهای موقت**: مجوزهایی که برای مدت محدود فعال هستند

---

---

## 📌 خلاصه تغییرات نسخه 1.1

### اضافه شده:
1. ✅ بخش عملگرها (Action Types) با 7 نوع مختلف
2. ✅ منطق قفل/باز بودن بر اساس Action Type
3. ✅ قفل خودکار بعد از امضاء
4. ✅ بخش پیوست و عطف (فایل خارجی و مدرک داخلی)
5. ✅ دکمه‌های استاندارد فرم با بررسی مجوز
6. ✅ جداول جدید: DocumentAttachments, DocumentReferrals
7. ✅ سرویس DocumentService
8. ✅ منطق بررسی قفل/باز بودن در PermissionService

### به‌روزرسانی شده:
1. ✅ فهرست مطالب
2. ✅ ساختار جداول دیتابیس
3. ✅ چک‌لیست پیاده‌سازی
4. ✅ مثال‌های استفاده

---

**تاریخ ایجاد:** 2025-01-27  
**آخرین به‌روزرسانی:** 2025-01-27  
**نسخه:** 1.1  
**نویسنده:** سیستم اتوماسیون اداری


using Automation.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Automation.Infrastructure.Data;

/// <summary>
/// Seed data for the database
/// </summary>
public static class SeedData
{
    public static async Task SeedFieldTypesAsync(AutomationDbContext context)
    {
        if (await context.FieldTypes.AnyAsync())
            return;

        var fieldTypes = new List<FieldType>
        {
            // Input Controls
            new() {
                Name = "TextInput",
                DisplayNameFa = "فیلد متنی",
                DisplayNameEn = "Text Input",
                Category = FieldTypeCategory.Input,
                Icon = "bi-input-cursor-text",
                Description = "فیلد ورودی متن تک خطی",
                CanCreateColumn = true,
                DisplayOrder = 1,
                DefaultProperties = """{"maxLength": 255, "inputType": "text"}""",
                HtmlTemplate = """<input type="text" id="{{id}}" name="{{name}}" class="form-control {{cssClasses}}" placeholder="{{placeholder}}" {{#if required}}required{{/if}} {{#if readonly}}readonly{{/if}} {{#if disabled}}disabled{{/if}} />""",
                RazorTemplate = """<input type="text" asp-for="{{name}}" class="form-control @Model.CssClasses" placeholder="@Model.Placeholder" />"""
            },
            new() {
                Name = "Password",
                DisplayNameFa = "رمز عبور",
                DisplayNameEn = "Password",
                Category = FieldTypeCategory.Input,
                Icon = "bi-key",
                Description = "فیلد ورودی رمز عبور",
                CanCreateColumn = true,
                DisplayOrder = 2,
                DefaultProperties = """{"maxLength": 100}""",
                HtmlTemplate = """<input type="password" id="{{id}}" name="{{name}}" class="form-control {{cssClasses}}" placeholder="{{placeholder}}" {{#if required}}required{{/if}} />"""
            },
            new() {
                Name = "Email",
                DisplayNameFa = "ایمیل",
                DisplayNameEn = "Email",
                Category = FieldTypeCategory.Input,
                Icon = "bi-envelope",
                Description = "فیلد ورودی ایمیل",
                CanCreateColumn = true,
                DisplayOrder = 3,
                DefaultProperties = """{"maxLength": 255}""",
                HtmlTemplate = """<input type="email" id="{{id}}" name="{{name}}" class="form-control {{cssClasses}}" placeholder="{{placeholder}}" {{#if required}}required{{/if}} />"""
            },
            new() {
                Name = "Number",
                DisplayNameFa = "عدد",
                DisplayNameEn = "Number",
                Category = FieldTypeCategory.Input,
                Icon = "bi-123",
                Description = "فیلد ورودی عدد",
                CanCreateColumn = true,
                DisplayOrder = 4,
                DefaultProperties = """{"min": null, "max": null, "step": 1}""",
                HtmlTemplate = """<input type="number" id="{{id}}" name="{{name}}" class="form-control {{cssClasses}}" {{#if min}}min="{{min}}"{{/if}} {{#if max}}max="{{max}}"{{/if}} step="{{step}}" {{#if required}}required{{/if}} />"""
            },
            new() {
                Name = "Phone",
                DisplayNameFa = "تلفن",
                DisplayNameEn = "Phone",
                Category = FieldTypeCategory.Input,
                Icon = "bi-telephone",
                Description = "فیلد ورودی شماره تلفن",
                CanCreateColumn = true,
                DisplayOrder = 5,
                DefaultProperties = """{"maxLength": 20, "pattern": "^[0-9+\\-\\s]+$"}""",
                HtmlTemplate = """<input type="tel" id="{{id}}" name="{{name}}" class="form-control {{cssClasses}}" placeholder="{{placeholder}}" {{#if required}}required{{/if}} />"""
            },
            new() {
                Name = "Textarea",
                DisplayNameFa = "متن چند خطی",
                DisplayNameEn = "Textarea",
                Category = FieldTypeCategory.Input,
                Icon = "bi-text-paragraph",
                Description = "فیلد ورودی متن چند خطی",
                CanCreateColumn = true,
                DisplayOrder = 6,
                DefaultProperties = """{"rows": 4, "maxLength": 2000}""",
                HtmlTemplate = """<textarea id="{{id}}" name="{{name}}" class="form-control {{cssClasses}}" rows="{{rows}}" placeholder="{{placeholder}}" {{#if required}}required{{/if}}>{{defaultValue}}</textarea>"""
            },
            new() {
                Name = "RichTextEditor",
                DisplayNameFa = "ویرایشگر متن غنی",
                DisplayNameEn = "Rich Text Editor",
                Category = FieldTypeCategory.Input,
                Icon = "bi-text-indent-left",
                Description = "ویرایشگر متن با قابلیت فرمت‌بندی",
                CanCreateColumn = true,
                DisplayOrder = 7,
                DefaultProperties = """{"toolbar": ["bold", "italic", "underline", "link", "list"]}""",
                HtmlTemplate = """<div id="{{id}}" class="rich-text-editor {{cssClasses}}" data-name="{{name}}"></div>"""
            },
            new() {
                Name = "Select",
                DisplayNameFa = "لیست انتخابی",
                DisplayNameEn = "Select",
                Category = FieldTypeCategory.Input,
                Icon = "bi-list-ul",
                Description = "لیست کشویی انتخابی",
                CanCreateColumn = true,
                DisplayOrder = 8,
                DefaultProperties = """{"options": [], "placeholder": "انتخاب کنید..."}""",
                HtmlTemplate = """<select id="{{id}}" name="{{name}}" class="form-select {{cssClasses}}" {{#if required}}required{{/if}}><option value="">{{placeholder}}</option>{{#each options}}<option value="{{value}}">{{label}}</option>{{/each}}</select>"""
            },
            new() {
                Name = "MultiSelect",
                DisplayNameFa = "انتخاب چندگانه",
                DisplayNameEn = "Multi Select",
                Category = FieldTypeCategory.Input,
                Icon = "bi-list-check",
                Description = "لیست انتخاب چندگانه",
                CanCreateColumn = true,
                DisplayOrder = 9,
                DefaultProperties = """{"options": [], "maxSelections": null}""",
                HtmlTemplate = """<select id="{{id}}" name="{{name}}" class="form-select {{cssClasses}}" multiple {{#if required}}required{{/if}}>{{#each options}}<option value="{{value}}">{{label}}</option>{{/each}}</select>"""
            },
            new() {
                Name = "Checkbox",
                DisplayNameFa = "چک باکس",
                DisplayNameEn = "Checkbox",
                Category = FieldTypeCategory.Input,
                Icon = "bi-check-square",
                Description = "چک باکس تکی",
                CanCreateColumn = true,
                DisplayOrder = 10,
                DefaultProperties = """{"checked": false}""",
                HtmlTemplate = """<div class="form-check"><input type="checkbox" id="{{id}}" name="{{name}}" class="form-check-input {{cssClasses}}" {{#if checked}}checked{{/if}} /><label for="{{id}}" class="form-check-label">{{label}}</label></div>"""
            },
            new() {
                Name = "CheckboxGroup",
                DisplayNameFa = "گروه چک باکس",
                DisplayNameEn = "Checkbox Group",
                Category = FieldTypeCategory.Input,
                Icon = "bi-ui-checks",
                Description = "گروه چند چک باکس",
                CanCreateColumn = true,
                DisplayOrder = 11,
                DefaultProperties = """{"options": [], "inline": false}"""
            },
            new() {
                Name = "RadioButton",
                DisplayNameFa = "دکمه رادیویی",
                DisplayNameEn = "Radio Button",
                Category = FieldTypeCategory.Input,
                Icon = "bi-record-circle",
                Description = "گروه دکمه‌های رادیویی",
                CanCreateColumn = true,
                DisplayOrder = 12,
                DefaultProperties = """{"options": [], "inline": false}""",
                HtmlTemplate = """{{#each options}}<div class="form-check {{#if ../inline}}form-check-inline{{/if}}"><input type="radio" id="{{../id}}_{{@index}}" name="{{../name}}" value="{{value}}" class="form-check-input" {{#if selected}}checked{{/if}} /><label for="{{../id}}_{{@index}}" class="form-check-label">{{label}}</label></div>{{/each}}"""
            },
            new() {
                Name = "Switch",
                DisplayNameFa = "سوئیچ",
                DisplayNameEn = "Switch",
                Category = FieldTypeCategory.Input,
                Icon = "bi-toggle-on",
                Description = "سوئیچ روشن/خاموش",
                CanCreateColumn = true,
                DisplayOrder = 13,
                DefaultProperties = """{"checked": false}""",
                HtmlTemplate = """<div class="form-check form-switch"><input type="checkbox" id="{{id}}" name="{{name}}" class="form-check-input {{cssClasses}}" role="switch" {{#if checked}}checked{{/if}} /><label for="{{id}}" class="form-check-label">{{label}}</label></div>"""
            },
            new() {
                Name = "DatePicker",
                DisplayNameFa = "تاریخ میلادی",
                DisplayNameEn = "Date Picker",
                Category = FieldTypeCategory.Input,
                Icon = "bi-calendar",
                Description = "انتخابگر تاریخ میلادی",
                CanCreateColumn = true,
                DisplayOrder = 14,
                DefaultProperties = """{"format": "yyyy-MM-dd", "minDate": null, "maxDate": null}""",
                HtmlTemplate = """<input type="date" id="{{id}}" name="{{name}}" class="form-control {{cssClasses}}" {{#if required}}required{{/if}} />"""
            },
            new() {
                Name = "PersianDatePicker",
                DisplayNameFa = "تاریخ شمسی",
                DisplayNameEn = "Persian Date Picker",
                Category = FieldTypeCategory.Input,
                Icon = "bi-calendar-event",
                Description = "انتخابگر تاریخ شمسی",
                CanCreateColumn = true,
                DisplayOrder = 15,
                DefaultProperties = """{"format": "yyyy/MM/dd", "minDate": null, "maxDate": null}""",
                HtmlTemplate = """<input type="text" id="{{id}}" name="{{name}}" class="form-control persian-datepicker {{cssClasses}}" data-jdp {{#if required}}required{{/if}} />"""
            },
            new() {
                Name = "TimePicker",
                DisplayNameFa = "زمان",
                DisplayNameEn = "Time Picker",
                Category = FieldTypeCategory.Input,
                Icon = "bi-clock",
                Description = "انتخابگر زمان",
                CanCreateColumn = true,
                DisplayOrder = 16,
                DefaultProperties = """{"format": "HH:mm", "step": 15}""",
                HtmlTemplate = """<input type="time" id="{{id}}" name="{{name}}" class="form-control {{cssClasses}}" {{#if required}}required{{/if}} />"""
            },
            new() {
                Name = "DateTimePicker",
                DisplayNameFa = "تاریخ و زمان",
                DisplayNameEn = "Date Time Picker",
                Category = FieldTypeCategory.Input,
                Icon = "bi-calendar-date",
                Description = "انتخابگر تاریخ و زمان",
                CanCreateColumn = true,
                DisplayOrder = 17,
                DefaultProperties = """{"format": "yyyy-MM-dd HH:mm"}""",
                HtmlTemplate = """<input type="datetime-local" id="{{id}}" name="{{name}}" class="form-control {{cssClasses}}" {{#if required}}required{{/if}} />"""
            },
            new() {
                Name = "FileUpload",
                DisplayNameFa = "آپلود فایل",
                DisplayNameEn = "File Upload",
                Category = FieldTypeCategory.Input,
                Icon = "bi-cloud-upload",
                Description = "آپلود فایل",
                CanCreateColumn = true,
                DisplayOrder = 18,
                DefaultProperties = """{"accept": "*/*", "maxSize": 10485760, "multiple": false}""",
                HtmlTemplate = """<input type="file" id="{{id}}" name="{{name}}" class="form-control {{cssClasses}}" accept="{{accept}}" {{#if multiple}}multiple{{/if}} {{#if required}}required{{/if}} />"""
            },
            new() {
                Name = "ImageUpload",
                DisplayNameFa = "آپلود تصویر",
                DisplayNameEn = "Image Upload",
                Category = FieldTypeCategory.Input,
                Icon = "bi-image",
                Description = "آپلود تصویر با پیش‌نمایش",
                CanCreateColumn = true,
                DisplayOrder = 19,
                DefaultProperties = """{"accept": "image/*", "maxSize": 5242880, "preview": true}"""
            },
            new() {
                Name = "Signature",
                DisplayNameFa = "امضا",
                DisplayNameEn = "Signature",
                Category = FieldTypeCategory.Input,
                Icon = "bi-pen",
                Description = "پد امضای دیجیتال",
                CanCreateColumn = true,
                DisplayOrder = 20,
                DefaultProperties = """{"width": 400, "height": 200, "penColor": "#000000"}"""
            },
            new() {
                Name = "Rating",
                DisplayNameFa = "امتیازدهی",
                DisplayNameEn = "Rating",
                Category = FieldTypeCategory.Input,
                Icon = "bi-star",
                Description = "امتیازدهی ستاره‌ای",
                CanCreateColumn = true,
                DisplayOrder = 21,
                DefaultProperties = """{"maxStars": 5, "allowHalf": false}"""
            },
            new() {
                Name = "RangeSlider",
                DisplayNameFa = "اسلایدر",
                DisplayNameEn = "Range Slider",
                Category = FieldTypeCategory.Input,
                Icon = "bi-sliders",
                Description = "اسلایدر محدوده",
                CanCreateColumn = true,
                DisplayOrder = 22,
                DefaultProperties = """{"min": 0, "max": 100, "step": 1}""",
                HtmlTemplate = """<input type="range" id="{{id}}" name="{{name}}" class="form-range {{cssClasses}}" min="{{min}}" max="{{max}}" step="{{step}}" />"""
            },
            new() {
                Name = "ColorPicker",
                DisplayNameFa = "انتخاب رنگ",
                DisplayNameEn = "Color Picker",
                Category = FieldTypeCategory.Input,
                Icon = "bi-palette",
                Description = "انتخابگر رنگ",
                CanCreateColumn = true,
                DisplayOrder = 23,
                DefaultProperties = """{"defaultColor": "#000000"}""",
                HtmlTemplate = """<input type="color" id="{{id}}" name="{{name}}" class="form-control form-control-color {{cssClasses}}" value="{{defaultValue}}" />"""
            },

            // Layout Controls
            new() {
                Name = "Container",
                DisplayNameFa = "کانتینر",
                DisplayNameEn = "Container",
                Category = FieldTypeCategory.Layout,
                Icon = "bi-bounding-box",
                Description = "کانتینر برای گروه‌بندی عناصر",
                IsContainer = true,
                CanCreateColumn = false,
                DisplayOrder = 50,
                DefaultProperties = """{"fluid": false}""",
                HtmlTemplate = """<div id="{{id}}" class="container{{#if fluid}}-fluid{{/if}} {{cssClasses}}">{{children}}</div>"""
            },
            new() {
                Name = "Row",
                DisplayNameFa = "سطر",
                DisplayNameEn = "Row",
                Category = FieldTypeCategory.Layout,
                Icon = "bi-layout-three-columns",
                Description = "سطر برای چیدمان ستون‌ها",
                IsContainer = true,
                CanCreateColumn = false,
                DisplayOrder = 51,
                DefaultProperties = """{"gutter": "g-3"}""",
                HtmlTemplate = """<div id="{{id}}" class="row {{gutter}} {{cssClasses}}">{{children}}</div>"""
            },
            new() {
                Name = "Column",
                DisplayNameFa = "ستون",
                DisplayNameEn = "Column",
                Category = FieldTypeCategory.Layout,
                Icon = "bi-layout-sidebar",
                Description = "ستون با عرض قابل تنظیم",
                IsContainer = true,
                CanCreateColumn = false,
                DisplayOrder = 52,
                DefaultProperties = """{"col": "col-md-6", "colSm": "", "colLg": "", "colXl": ""}""",
                HtmlTemplate = """<div id="{{id}}" class="{{col}} {{colSm}} {{colLg}} {{colXl}} {{cssClasses}}">{{children}}</div>"""
            },
            new() {
                Name = "FlexContainer",
                DisplayNameFa = "کانتینر Flex",
                DisplayNameEn = "Flex Container",
                Category = FieldTypeCategory.Layout,
                Icon = "bi-distribute-horizontal",
                Description = "کانتینر با چیدمان Flexbox",
                IsContainer = true,
                CanCreateColumn = false,
                DisplayOrder = 53,
                DefaultProperties = """{"direction": "row", "justify": "start", "align": "stretch", "wrap": "wrap", "gap": "1rem"}""",
                HtmlTemplate = """<div id="{{id}}" class="d-flex flex-{{direction}} justify-content-{{justify}} align-items-{{align}} flex-{{wrap}} {{cssClasses}}" style="gap: {{gap}}">{{children}}</div>"""
            },
            new() {
                Name = "GridContainer",
                DisplayNameFa = "کانتینر Grid",
                DisplayNameEn = "Grid Container",
                Category = FieldTypeCategory.Layout,
                Icon = "bi-grid",
                Description = "کانتینر با چیدمان CSS Grid",
                IsContainer = true,
                CanCreateColumn = false,
                DisplayOrder = 54,
                DefaultProperties = """{"columns": 3, "gap": "1rem"}""",
                HtmlTemplate = """<div id="{{id}}" class="{{cssClasses}}" style="display: grid; grid-template-columns: repeat({{columns}}, 1fr); gap: {{gap}}">{{children}}</div>"""
            },
            new() {
                Name = "Table",
                DisplayNameFa = "جدول",
                DisplayNameEn = "Table",
                Category = FieldTypeCategory.Layout,
                Icon = "bi-table",
                Description = "جدول با سطر و ستون",
                IsContainer = true,
                CanCreateColumn = false,
                DisplayOrder = 55,
                DefaultProperties = """{"bordered": true, "striped": true, "hover": true, "responsive": true}""",
                HtmlTemplate = """<div class="{{#if responsive}}table-responsive{{/if}}"><table id="{{id}}" class="table {{#if bordered}}table-bordered{{/if}} {{#if striped}}table-striped{{/if}} {{#if hover}}table-hover{{/if}} {{cssClasses}}">{{children}}</table></div>"""
            },
            new() {
                Name = "Card",
                DisplayNameFa = "کارت",
                DisplayNameEn = "Card",
                Category = FieldTypeCategory.Layout,
                Icon = "bi-card-text",
                Description = "کارت با هدر و بدنه",
                IsContainer = true,
                CanCreateColumn = false,
                DisplayOrder = 56,
                DefaultProperties = """{"showHeader": true, "showFooter": false, "headerTitle": ""}""",
                HtmlTemplate = """<div id="{{id}}" class="card {{cssClasses}}">{{#if showHeader}}<div class="card-header">{{headerTitle}}</div>{{/if}}<div class="card-body">{{children}}</div>{{#if showFooter}}<div class="card-footer"></div>{{/if}}</div>"""
            },
            new() {
                Name = "Panel",
                DisplayNameFa = "پنل",
                DisplayNameEn = "Panel",
                Category = FieldTypeCategory.Layout,
                Icon = "bi-window",
                Description = "پنل با عنوان",
                IsContainer = true,
                CanCreateColumn = false,
                DisplayOrder = 57,
                DefaultProperties = """{"title": "", "collapsible": false, "collapsed": false}"""
            },
            new() {
                Name = "Accordion",
                DisplayNameFa = "آکاردئون",
                DisplayNameEn = "Accordion",
                Category = FieldTypeCategory.Layout,
                Icon = "bi-chevron-bar-expand",
                Description = "گروه پنل‌های قابل بسته شدن",
                IsContainer = true,
                CanCreateColumn = false,
                DisplayOrder = 58,
                DefaultProperties = """{"alwaysOpen": false, "flush": false}""",
                HtmlTemplate = """<div id="{{id}}" class="accordion {{#if flush}}accordion-flush{{/if}} {{cssClasses}}">{{children}}</div>"""
            },
            new() {
                Name = "Tabs",
                DisplayNameFa = "تب‌ها",
                DisplayNameEn = "Tabs",
                Category = FieldTypeCategory.Layout,
                Icon = "bi-folder",
                Description = "تب‌های قابل انتخاب",
                IsContainer = true,
                CanCreateColumn = false,
                DisplayOrder = 59,
                DefaultProperties = """{"type": "tabs", "justified": false}""",
                HtmlTemplate = """<div id="{{id}}"><ul class="nav nav-{{type}} {{#if justified}}nav-justified{{/if}} {{cssClasses}}" role="tablist">{{tabHeaders}}</ul><div class="tab-content">{{children}}</div></div>"""
            },
            new() {
                Name = "Modal",
                DisplayNameFa = "پنجره مودال",
                DisplayNameEn = "Modal",
                Category = FieldTypeCategory.Layout,
                Icon = "bi-window-dock",
                Description = "پنجره بازشو",
                IsContainer = true,
                CanCreateColumn = false,
                DisplayOrder = 60,
                DefaultProperties = """{"title": "", "size": "modal-lg", "staticBackdrop": false, "scrollable": true}""",
                HtmlTemplate = """<div id="{{id}}" class="modal fade" tabindex="-1" {{#if staticBackdrop}}data-bs-backdrop="static"{{/if}}><div class="modal-dialog {{size}} {{#if scrollable}}modal-dialog-scrollable{{/if}}"><div class="modal-content"><div class="modal-header"><h5 class="modal-title">{{title}}</h5><button type="button" class="btn-close" data-bs-dismiss="modal"></button></div><div class="modal-body">{{children}}</div></div></div></div>"""
            },
            new() {
                Name = "Divider",
                DisplayNameFa = "خط جداکننده",
                DisplayNameEn = "Divider",
                Category = FieldTypeCategory.Layout,
                Icon = "bi-hr",
                Description = "خط افقی جداکننده",
                IsContainer = false,
                CanCreateColumn = false,
                DisplayOrder = 61,
                DefaultProperties = """{}""",
                HtmlTemplate = """<hr id="{{id}}" class="{{cssClasses}}" />"""
            },
            new() {
                Name = "Spacer",
                DisplayNameFa = "فاصله",
                DisplayNameEn = "Spacer",
                Category = FieldTypeCategory.Layout,
                Icon = "bi-arrows-expand",
                Description = "فاصله عمودی",
                IsContainer = false,
                CanCreateColumn = false,
                DisplayOrder = 62,
                DefaultProperties = """{"height": "1rem"}""",
                HtmlTemplate = """<div id="{{id}}" class="{{cssClasses}}" style="height: {{height}}"></div>"""
            },

            // Static Elements
            new() {
                Name = "Label",
                DisplayNameFa = "برچسب",
                DisplayNameEn = "Label",
                Category = FieldTypeCategory.Static,
                Icon = "bi-tag",
                Description = "برچسب متنی",
                CanCreateColumn = false,
                DisplayOrder = 70,
                DefaultProperties = """{"text": "", "for": ""}""",
                HtmlTemplate = """<label id="{{id}}" for="{{for}}" class="form-label {{cssClasses}}">{{text}}</label>"""
            },
            new() {
                Name = "Heading",
                DisplayNameFa = "تیتر",
                DisplayNameEn = "Heading",
                Category = FieldTypeCategory.Static,
                Icon = "bi-type-h1",
                Description = "تیتر H1 تا H6",
                CanCreateColumn = false,
                DisplayOrder = 71,
                DefaultProperties = """{"level": 3, "text": ""}""",
                HtmlTemplate = """<h{{level}} id="{{id}}" class="{{cssClasses}}">{{text}}</h{{level}}>"""
            },
            new() {
                Name = "Paragraph",
                DisplayNameFa = "پاراگراف",
                DisplayNameEn = "Paragraph",
                Category = FieldTypeCategory.Static,
                Icon = "bi-text-left",
                Description = "متن پاراگراف",
                CanCreateColumn = false,
                DisplayOrder = 72,
                DefaultProperties = """{"text": ""}""",
                HtmlTemplate = """<p id="{{id}}" class="{{cssClasses}}">{{text}}</p>"""
            },
            new() {
                Name = "HtmlContent",
                DisplayNameFa = "محتوای HTML",
                DisplayNameEn = "HTML Content",
                Category = FieldTypeCategory.Static,
                Icon = "bi-code-slash",
                Description = "محتوای HTML سفارشی",
                CanCreateColumn = false,
                DisplayOrder = 73,
                DefaultProperties = """{"content": ""}""",
                HtmlTemplate = """<div id="{{id}}" class="{{cssClasses}}">{{{content}}}</div>"""
            },
            new() {
                Name = "Image",
                DisplayNameFa = "تصویر",
                DisplayNameEn = "Image",
                Category = FieldTypeCategory.Static,
                Icon = "bi-image",
                Description = "تصویر",
                CanCreateColumn = false,
                DisplayOrder = 74,
                DefaultProperties = """{"src": "", "alt": "", "fluid": true}""",
                HtmlTemplate = """<img id="{{id}}" src="{{src}}" alt="{{alt}}" class="{{#if fluid}}img-fluid{{/if}} {{cssClasses}}" />"""
            },
            new() {
                Name = "Icon",
                DisplayNameFa = "آیکون",
                DisplayNameEn = "Icon",
                Category = FieldTypeCategory.Static,
                Icon = "bi-emoji-smile",
                Description = "آیکون Bootstrap",
                CanCreateColumn = false,
                DisplayOrder = 75,
                DefaultProperties = """{"iconClass": "bi-star", "size": "1rem"}""",
                HtmlTemplate = """<i id="{{id}}" class="{{iconClass}} {{cssClasses}}" style="font-size: {{size}}"></i>"""
            },
            new() {
                Name = "Alert",
                DisplayNameFa = "پیام هشدار",
                DisplayNameEn = "Alert",
                Category = FieldTypeCategory.Static,
                Icon = "bi-exclamation-triangle",
                Description = "پیام هشدار/اطلاع‌رسانی",
                CanCreateColumn = false,
                DisplayOrder = 76,
                DefaultProperties = """{"type": "info", "dismissible": true, "text": ""}""",
                HtmlTemplate = """<div id="{{id}}" class="alert alert-{{type}} {{#if dismissible}}alert-dismissible fade show{{/if}} {{cssClasses}}" role="alert">{{text}}{{#if dismissible}}<button type="button" class="btn-close" data-bs-dismiss="alert"></button>{{/if}}</div>"""
            },

            // Action Elements
            new() {
                Name = "Button",
                DisplayNameFa = "دکمه",
                DisplayNameEn = "Button",
                Category = FieldTypeCategory.Action,
                Icon = "bi-hand-index",
                Description = "دکمه کلیک",
                CanCreateColumn = false,
                DisplayOrder = 80,
                DefaultProperties = """{"type": "button", "variant": "primary", "size": "", "text": "دکمه"}""",
                HtmlTemplate = """<button id="{{id}}" type="{{type}}" class="btn btn-{{variant}} {{size}} {{cssClasses}}">{{text}}</button>"""
            },
            new() {
                Name = "SubmitButton",
                DisplayNameFa = "دکمه ثبت",
                DisplayNameEn = "Submit Button",
                Category = FieldTypeCategory.Action,
                Icon = "bi-check-circle",
                Description = "دکمه ارسال فرم",
                CanCreateColumn = false,
                DisplayOrder = 81,
                DefaultProperties = """{"variant": "success", "size": "", "text": "ثبت"}""",
                HtmlTemplate = """<button id="{{id}}" type="submit" class="btn btn-{{variant}} {{size}} {{cssClasses}}">{{text}}</button>"""
            },
            new() {
                Name = "ResetButton",
                DisplayNameFa = "دکمه بازنشانی",
                DisplayNameEn = "Reset Button",
                Category = FieldTypeCategory.Action,
                Icon = "bi-arrow-counterclockwise",
                Description = "دکمه بازنشانی فرم",
                CanCreateColumn = false,
                DisplayOrder = 82,
                DefaultProperties = """{"variant": "secondary", "text": "بازنشانی"}""",
                HtmlTemplate = """<button id="{{id}}" type="reset" class="btn btn-{{variant}} {{cssClasses}}">{{text}}</button>"""
            },
            new() {
                Name = "LinkButton",
                DisplayNameFa = "دکمه لینک",
                DisplayNameEn = "Link Button",
                Category = FieldTypeCategory.Action,
                Icon = "bi-link",
                Description = "دکمه با لینک",
                CanCreateColumn = false,
                DisplayOrder = 83,
                DefaultProperties = """{"href": "#", "variant": "link", "target": "_self", "text": "لینک"}""",
                HtmlTemplate = """<a id="{{id}}" href="{{href}}" class="btn btn-{{variant}} {{cssClasses}}" target="{{target}}">{{text}}</a>"""
            }
        };

        await context.FieldTypes.AddRangeAsync(fieldTypes);
        await context.SaveChangesAsync();
    }

    public static async Task SeedFormCategoriesAsync(AutomationDbContext context)
    {
        if (await context.FormCategories.AnyAsync())
            return;

        var categories = new List<FormCategory>
        {
            new() { Code = "ADMIN", NameFa = "اداری", NameEn = "Administrative", Icon = "bi-building", DisplayOrder = 1 },
            new() { Code = "HR", NameFa = "منابع انسانی", NameEn = "Human Resources", Icon = "bi-people", DisplayOrder = 2 },
            new() { Code = "FIN", NameFa = "مالی", NameEn = "Financial", Icon = "bi-currency-dollar", DisplayOrder = 3 },
            new() { Code = "IT", NameFa = "فناوری اطلاعات", NameEn = "IT", Icon = "bi-laptop", DisplayOrder = 4 },
            new() { Code = "WEB", NameFa = "وب‌سایت", NameEn = "Website", Icon = "bi-globe", DisplayOrder = 5 },
            new() { Code = "CRM", NameFa = "ارتباط با مشتری", NameEn = "CRM", Icon = "bi-person-badge", DisplayOrder = 6 },
            new() { Code = "OTHER", NameFa = "سایر", NameEn = "Other", Icon = "bi-folder", DisplayOrder = 99 }
        };

        await context.FormCategories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }

    public static async Task SeedScalarFunctionsAsync(AutomationDbContext context)
    {
        if (await context.ScalarFunctions.AnyAsync())
            return;

        var functions = new List<ScalarFunction>
        {
            // DateTime Functions
            new() {
                Name = "GETDATE",
                DisplayName = "تاریخ و زمان جاری",
                DisplayNameEn = "Current DateTime",
                ReturnType = SqlDataType.DateTime,
                Category = FunctionCategory.DateTime,
                FunctionBody = "GETDATE()",
                IsSystem = true,
                Description = "تاریخ و زمان سرور را برمیگرداند",
                DisplayOrder = 1
            },
            new() {
                Name = "GETUTCDATE",
                DisplayName = "تاریخ و زمان UTC",
                DisplayNameEn = "UTC DateTime",
                ReturnType = SqlDataType.DateTime,
                Category = FunctionCategory.DateTime,
                FunctionBody = "GETUTCDATE()",
                IsSystem = true,
                Description = "تاریخ و زمان UTC سرور را برمیگرداند",
                DisplayOrder = 2
            },
            new() {
                Name = "SYSDATETIME",
                DisplayName = "تاریخ سیستم",
                DisplayNameEn = "System DateTime",
                ReturnType = SqlDataType.DateTime,
                Category = FunctionCategory.DateTime,
                FunctionBody = "SYSDATETIME()",
                IsSystem = true,
                Description = "تاریخ و زمان دقیق سیستم",
                DisplayOrder = 3
            },

            // Identifier Functions
            new() {
                Name = "NEWID",
                DisplayName = "شناسه یکتا (int)",
                DisplayNameEn = "New int",
                ReturnType = SqlDataType.UniqueIdentifier,
                Category = FunctionCategory.System,
                FunctionBody = "NEWID()",
                IsSystem = true,
                Description = "یک int جدید تولید میکند",
                DisplayOrder = 10
            },
            new() {
                Name = "NEWSEQUENTIALID",
                DisplayName = "شناسه یکتای متوالی",
                DisplayNameEn = "Sequential int",
                ReturnType = SqlDataType.UniqueIdentifier,
                Category = FunctionCategory.System,
                FunctionBody = "NEWSEQUENTIALID()",
                IsSystem = true,
                Description = "int متوالی برای کارایی بهتر در index",
                DisplayOrder = 11
            },

            // User Functions
            new() {
                Name = "CURRENT_USER",
                DisplayName = "کاربر جاری",
                DisplayNameEn = "Current User",
                ReturnType = SqlDataType.NVarChar,
                Category = FunctionCategory.System,
                FunctionBody = "CURRENT_USER",
                IsSystem = true,
                Description = "نام کاربر دیتابیس جاری",
                DisplayOrder = 20
            },
            new() {
                Name = "SYSTEM_USER",
                DisplayName = "کاربر سیستم",
                DisplayNameEn = "System User",
                ReturnType = SqlDataType.NVarChar,
                Category = FunctionCategory.System,
                FunctionBody = "SYSTEM_USER",
                IsSystem = true,
                Description = "نام کاربر سیستم",
                DisplayOrder = 21
            },
            new() {
                Name = "HOST_NAME",
                DisplayName = "نام سیستم کلاینت",
                DisplayNameEn = "Host Name",
                ReturnType = SqlDataType.NVarChar,
                Category = FunctionCategory.System,
                FunctionBody = "HOST_NAME()",
                IsSystem = true,
                Description = "نام کامپیوتر کلاینت",
                DisplayOrder = 22
            },

            // Numeric Functions
            new() {
                Name = "ZERO_INT",
                DisplayName = "عدد صفر",
                DisplayNameEn = "Zero Integer",
                ReturnType = SqlDataType.Int,
                Category = FunctionCategory.Math,
                FunctionBody = "0",
                IsSystem = true,
                Description = "مقدار پیشفرض صفر",
                DisplayOrder = 30
            },
            new() {
                Name = "ONE_INT",
                DisplayName = "عدد یک",
                DisplayNameEn = "One Integer",
                ReturnType = SqlDataType.Int,
                Category = FunctionCategory.Math,
                FunctionBody = "1",
                IsSystem = true,
                Description = "مقدار پیشفرض یک",
                DisplayOrder = 31
            },

            // Boolean Functions
            new() {
                Name = "DEFAULT_FALSE",
                DisplayName = "مقدار خیر",
                DisplayNameEn = "Default False",
                ReturnType = SqlDataType.Bit,
                Category = FunctionCategory.General,
                FunctionBody = "0",
                IsSystem = true,
                Description = "مقدار پیشفرض false",
                DisplayOrder = 40
            },
            new() {
                Name = "DEFAULT_TRUE",
                DisplayName = "مقدار بله",
                DisplayNameEn = "Default True",
                ReturnType = SqlDataType.Bit,
                Category = FunctionCategory.General,
                FunctionBody = "1",
                IsSystem = true,
                Description = "مقدار پیشفرض true",
                DisplayOrder = 41
            },

            // Text Functions
            new() {
                Name = "EMPTY_STRING",
                DisplayName = "رشته خالی",
                DisplayNameEn = "Empty String",
                ReturnType = SqlDataType.NVarChar,
                Category = FunctionCategory.Text,
                FunctionBody = "''",
                IsSystem = true,
                Description = "رشته خالی بعنوان مقدار پیشفرض",
                DisplayOrder = 50
            }
        };

        await context.ScalarFunctions.AddRangeAsync(functions);
        await context.SaveChangesAsync();
    }

    public static async Task SeedActionTypesAsync(CoreDbContext context)
    {
        if (await context.ActionTypes.AnyAsync())
            return;

        var actionTypes = new List<ActionType>
        {
            new() {
                ActionCode = "ACTION_FOR_SIGNATURE",
                ActionNameFa = "جهت امضاء",
                ActionNameEn = "For Signature",
                IsEditable = true,
                DisplayOrder = 1,
                Description = "نامه برای امضاء ارجاع می‌شود و باید قابل ویرایش باشد"
            },
            new() {
                ActionCode = "ACTION_FOR_REVIEW",
                ActionNameFa = "جهت بررسی",
                ActionNameEn = "For Review",
                IsEditable = false,
                DisplayOrder = 2,
                Description = "نامه فقط برای بررسی و مشاهده ارجاع می‌شود"
            },
            new() {
                ActionCode = "ACTION_FOR_ORDER",
                ActionNameFa = "جهت دستور",
                ActionNameEn = "For Order",
                IsEditable = false,
                DisplayOrder = 3,
                Description = "نامه برای دریافت دستور ارجاع می‌شود"
            },
            new() {
                ActionCode = "ACTION_FOR_OUTGOING",
                ActionNameFa = "جهت ثبت صادره",
                ActionNameEn = "For Outgoing Registration",
                IsEditable = true,
                DisplayOrder = 4,
                Description = "نامه برای ثبت به عنوان صادره ارجاع می‌شود"
            },
            new() {
                ActionCode = "ACTION_FOR_ARCHIVE",
                ActionNameFa = "جهت بایگانی",
                ActionNameEn = "For Archive",
                IsEditable = false,
                DisplayOrder = 5,
                Description = "نامه برای بایگانی ارجاع می‌شود"
            },
            new() {
                ActionCode = "ACTION_FOR_EDIT",
                ActionNameFa = "جهت اصلاح و ویرایش",
                ActionNameEn = "For Edit",
                IsEditable = true,
                DisplayOrder = 6,
                Description = "نامه برای اصلاح و ویرایش ارجاع می‌شود"
            },
            new() {
                ActionCode = "ACTION_FOR_VIEW_COPY",
                ActionNameFa = "جهت مشاهده رونوشت",
                ActionNameEn = "For View Copy",
                IsEditable = false,
                DisplayOrder = 7,
                Description = "نامه فقط برای مشاهده رونوشت ارجاع می‌شود"
            }
        };

        await context.ActionTypes.AddRangeAsync(actionTypes);
        await context.SaveChangesAsync();
    }

    public static async Task SeedPermissionGroupsAsync(IdentityDbContext context)
    {
        if (await context.PermissionGroups.AnyAsync())
            return;

        // ایجاد دسته‌های اصلی
        var formGroup = new PermissionGroup
        {
            GroupCode = "FORM",
            GroupName = "فرم‌ها",
            Icon = "bi-file-earmark-text",
            DisplayOrder = 1,
            Description = "مجوزهای مربوط به فرم‌ها"
        };

        var cabinetGroup = new PermissionGroup
        {
            GroupCode = "CABINET",
            GroupName = "کارتابل‌ها",
            Icon = "bi-inbox",
            DisplayOrder = 2,
            Description = "مجوزهای مربوط به کارتابل‌ها"
        };

        var archiveGroup = new PermissionGroup
        {
            GroupCode = "ARCHIVE",
            GroupName = "دبیرخانه",
            Icon = "bi-archive",
            DisplayOrder = 4,
            Description = "مجوزهای مربوط به دبیرخانه"
        };

        var searchGroup = new PermissionGroup
        {
            GroupCode = "SEARCH",
            GroupName = "جستجو",
            Icon = "bi-search",
            DisplayOrder = 5,
            Description = "مجوزهای مربوط به جستجو"
        };

        var reportGroup = new PermissionGroup
        {
            GroupCode = "REPORT",
            GroupName = "گزارش‌ها",
            Icon = "bi-graph-up",
            DisplayOrder = 6,
            Description = "مجوزهای مربوط به گزارش‌ها"
        };

        var settingsGroup = new PermissionGroup
        {
            GroupCode = "SETTINGS",
            GroupName = "تنظیمات",
            Icon = "bi-gear",
            DisplayOrder = 7,
            Description = "مجوزهای مربوط به تنظیمات"
        };

        var dashboardGroup = new PermissionGroup
        {
            GroupCode = "DASHBOARD",
            GroupName = "داشبورد",
            Icon = "bi-speedometer2",
            DisplayOrder = 8,
            Description = "مجوزهای مربوط به داشبورد"
        };

        await context.PermissionGroups.AddRangeAsync(formGroup, cabinetGroup, archiveGroup, searchGroup, reportGroup, settingsGroup, dashboardGroup);
        await context.SaveChangesAsync();

        // ایجاد زیردسته‌ها برای فرم
        var formDesignGroup = new PermissionGroup
        {
            GroupCode = "FORM_DESIGN",
            GroupName = "طراحی فرم",
            Icon = "bi-pencil-square",
            ParentGroupId = formGroup.Id,
            DisplayOrder = 1,
            Description = "مجوزهای مربوط به طراحی و ساختار فرم"
        };

        var formDataGroup = new PermissionGroup
        {
            GroupCode = "FORM_DATA",
            GroupName = "اطلاعات فرم",
            Icon = "bi-database",
            ParentGroupId = formGroup.Id,
            DisplayOrder = 2,
            Description = "مجوزهای مربوط به ثبت و ویرایش اطلاعات فرم"
        };

        var formAttachmentGroup = new PermissionGroup
        {
            GroupCode = "FORM_ATTACHMENT",
            GroupName = "پیوست‌ها",
            Icon = "bi-paperclip",
            ParentGroupId = formGroup.Id,
            DisplayOrder = 3,
            Description = "مجوزهای مربوط به پیوست‌ها"
        };

        var formSignGroup = new PermissionGroup
        {
            GroupCode = "FORM_SIGN",
            GroupName = "امضاء و قفل",
            Icon = "bi-lock",
            ParentGroupId = formGroup.Id,
            DisplayOrder = 4,
            Description = "مجوزهای مربوط به امضاء و قفل اطلاعات"
        };

        await context.PermissionGroups.AddRangeAsync(formDesignGroup, formDataGroup, formAttachmentGroup, formSignGroup);
        await context.SaveChangesAsync();
    }

    public static async Task SeedSystemPermissionsAsync(IdentityDbContext context)
    {
        if (await context.Permissions.AnyAsync())
            return;

        // دریافت دسته‌ها
        var formDesignGroup = await context.PermissionGroups.FirstOrDefaultAsync(pg => pg.GroupCode == "FORM_DESIGN");
        var formDataGroup = await context.PermissionGroups.FirstOrDefaultAsync(pg => pg.GroupCode == "FORM_DATA");
        var formAttachmentGroup = await context.PermissionGroups.FirstOrDefaultAsync(pg => pg.GroupCode == "FORM_ATTACHMENT");
        var formSignGroup = await context.PermissionGroups.FirstOrDefaultAsync(pg => pg.GroupCode == "FORM_SIGN");
        var cabinetGroup = await context.PermissionGroups.FirstOrDefaultAsync(pg => pg.GroupCode == "CABINET");
        var archiveGroup = await context.PermissionGroups.FirstOrDefaultAsync(pg => pg.GroupCode == "ARCHIVE");
        var searchGroup = await context.PermissionGroups.FirstOrDefaultAsync(pg => pg.GroupCode == "SEARCH");
        var reportGroup = await context.PermissionGroups.FirstOrDefaultAsync(pg => pg.GroupCode == "REPORT");
        var settingsGroup = await context.PermissionGroups.FirstOrDefaultAsync(pg => pg.GroupCode == "SETTINGS");
        var dashboardGroup = await context.PermissionGroups.FirstOrDefaultAsync(pg => pg.GroupCode == "DASHBOARD");

        var permissions = new List<Permission>
        {
            // فرم - طراحی
            new() { PermissionCode = "FORM_DESIGN_CREATE", PermissionName = "طراحی فرم جدید", Category = "FORM", PermissionGroupId = formDesignGroup?.Id, DisplayOrder = 1, Description = "امکان ایجاد فرم جدید در فرم‌ساز" },
            new() { PermissionCode = "FORM_DESIGN_EDIT", PermissionName = "ویرایش ساختار فرم", Category = "FORM", PermissionGroupId = formDesignGroup?.Id, DisplayOrder = 2, Description = "امکان ویرایش ساختار و کنترل‌های یک فرم" },
            new() { PermissionCode = "FORM_DESIGN_DELETE", PermissionName = "حذف فرم", Category = "FORM", PermissionGroupId = formDesignGroup?.Id, DisplayOrder = 3, Description = "امکان حذف فرم از سیستم" },
            new() { PermissionCode = "FORM_DESIGN_VIEW", PermissionName = "مشاهده فرم‌ها", Category = "FORM", PermissionGroupId = formDesignGroup?.Id, DisplayOrder = 4, Description = "امکان مشاهده لیست فرم‌های طراحی شده" },
            new() { PermissionCode = "FORM_DESIGN_PUBLISH", PermissionName = "انتشار فرم", Category = "FORM", PermissionGroupId = formDesignGroup?.Id, DisplayOrder = 5, Description = "امکان انتشار فرم برای استفاده" },
            
            // فرم - اطلاعات
            new() { PermissionCode = "FORM_DATA_CREATE", PermissionName = "ثبت اطلاعات فرم", Category = "FORM", PermissionGroupId = formDataGroup?.Id, DisplayOrder = 1, Description = "امکان ثبت اطلاعات جدید در یک فرم" },
            new() { PermissionCode = "FORM_DATA_VIEW", PermissionName = "مشاهده اطلاعات فرم", Category = "FORM", PermissionGroupId = formDataGroup?.Id, DisplayOrder = 2, Description = "امکان مشاهده اطلاعات ثبت شده در فرم" },
            new() { PermissionCode = "FORM_DATA_EDIT", PermissionName = "ویرایش اطلاعات فرم", Category = "FORM", PermissionGroupId = formDataGroup?.Id, DisplayOrder = 3, Description = "امکان ویرایش اطلاعات ثبت شده (قبل از امضاء)" },
            new() { PermissionCode = "FORM_DATA_DELETE", PermissionName = "حذف اطلاعات فرم", Category = "FORM", PermissionGroupId = formDataGroup?.Id, DisplayOrder = 4, Description = "امکان حذف اطلاعات ثبت شده (قبل از امضاء)" },
            new() { PermissionCode = "FORM_DATA_SAVE", PermissionName = "ذخیره اطلاعات", Category = "FORM", PermissionGroupId = formDataGroup?.Id, DisplayOrder = 5, Description = "امکان ذخیره اطلاعات فرم" },
            new() { PermissionCode = "FORM_DATA_SUBMIT", PermissionName = "ثبت نهایی", Category = "FORM", PermissionGroupId = formDataGroup?.Id, DisplayOrder = 6, Description = "امکان ثبت نهایی اطلاعات فرم" },
            new() { PermissionCode = "FORM_DATA_SAVE_CHANGES", PermissionName = "ثبت تغییرات", Category = "FORM", PermissionGroupId = formDataGroup?.Id, DisplayOrder = 7, Description = "امکان ذخیره تغییرات اعمال شده" },
            new() { PermissionCode = "FORM_DATA_PRINT_TEMPLATE", PermissionName = "استفاده از قالب چاپ", Category = "FORM", PermissionGroupId = formDataGroup?.Id, DisplayOrder = 8, Description = "امکان چاپ فرم با استفاده از قالب‌های تعریف شده" },
            new() { PermissionCode = "FORM_DATA_EXPORT", PermissionName = "خروجی اطلاعات", Category = "FORM", PermissionGroupId = formDataGroup?.Id, DisplayOrder = 9, Description = "امکان خروجی گرفتن از اطلاعات فرم" },
            
            // فرم - پیوست
            new() { PermissionCode = "FORM_ATTACHMENT_UPLOAD", PermissionName = "پیوست کردن مدرک خارجی", Category = "FORM", PermissionGroupId = formAttachmentGroup?.Id, DisplayOrder = 1, Description = "امکان آپلود فایل خارج از سیستم به عنوان پیوست" },
            new() { PermissionCode = "FORM_ATTACHMENT_ATTACH_DOCUMENT", PermissionName = "پیوست کردن مدرک داخلی", Category = "FORM", PermissionGroupId = formAttachmentGroup?.Id, DisplayOrder = 2, Description = "امکان پیوست کردن یک مدرک/نامه از سیستم به فرم" },
            new() { PermissionCode = "FORM_ATTACHMENT_VIEW", PermissionName = "مشاهده پیوست", Category = "FORM", PermissionGroupId = formAttachmentGroup?.Id, DisplayOrder = 3, Description = "امکان مشاهده و دانلود پیوست‌های فرم" },
            new() { PermissionCode = "FORM_ATTACHMENT_DELETE", PermissionName = "حذف پیوست", Category = "FORM", PermissionGroupId = formAttachmentGroup?.Id, DisplayOrder = 4, Description = "امکان حذف پیوست‌های فرم (قبل از امضاء)" },
            new() { PermissionCode = "FORM_REFERENCE_ATTACH_DOCUMENT", PermissionName = "عطف کردن مدرک داخلی", Category = "FORM", PermissionGroupId = formAttachmentGroup?.Id, DisplayOrder = 5, Description = "امکان عطف کردن یک مدرک/نامه از سیستم به فرم" },
            new() { PermissionCode = "FORM_REFERENCE_ATTACH_EXTERNAL", PermissionName = "عطف کردن مدرک خارجی", Category = "FORM", PermissionGroupId = formAttachmentGroup?.Id, DisplayOrder = 6, Description = "امکان عطف کردن فایل خارج از سیستم" },
            new() { PermissionCode = "FORM_REFERENCE_VIEW", PermissionName = "مشاهده عطف", Category = "FORM", PermissionGroupId = formAttachmentGroup?.Id, DisplayOrder = 7, Description = "امکان مشاهده و دانلود عطف‌های فرم" },
            new() { PermissionCode = "FORM_REFERENCE_DELETE", PermissionName = "حذف عطف", Category = "FORM", PermissionGroupId = formAttachmentGroup?.Id, DisplayOrder = 8, Description = "امکان حذف عطف‌های فرم (قبل از امضاء)" },
            
            // فرم - امضاء
            new() { PermissionCode = "FORM_SIGN", PermissionName = "امضاء فرم", Category = "FORM", PermissionGroupId = formSignGroup?.Id, DisplayOrder = 1, Description = "امکان امضاء کردن فرم (قفل شدن اطلاعات)" },
            new() { PermissionCode = "FORM_SIGNED_VIEW", PermissionName = "مشاهده فرم امضاء شده", Category = "FORM", PermissionGroupId = formSignGroup?.Id, DisplayOrder = 2, Description = "امکان مشاهده فرم‌های امضاء شده (بدون امکان ویرایش)" },
            new() { PermissionCode = "FORM_SIGNED_EDIT", PermissionName = "ویرایش فرم امضاء شده", Category = "FORM", PermissionGroupId = formSignGroup?.Id, DisplayOrder = 3, Description = "امکان ویرایش فرم‌های امضاء شده (فقط برای نقش‌های خاص)" },
            new() { PermissionCode = "FORM_SIGN_CANCEL", PermissionName = "لغو امضاء", Category = "FORM", PermissionGroupId = formSignGroup?.Id, DisplayOrder = 4, Description = "امکان لغو امضاء فرم (فقط برای نقش‌های خاص)" },
            
            // کارتابل
            new() { PermissionCode = "CABINET_INBOX_VIEW", PermissionName = "مشاهده کارتابل ورودی", Category = "CABINET", PermissionGroupId = cabinetGroup?.Id, DisplayOrder = 1, Description = "امکان مشاهده نامه‌ها و فرم‌های دریافتی" },
            new() { PermissionCode = "CABINET_INBOX_DETAIL", PermissionName = "مشاهده جزئیات ورودی", Category = "CABINET", PermissionGroupId = cabinetGroup?.Id, DisplayOrder = 2, Description = "امکان مشاهده جزئیات کامل نامه/فرم دریافتی" },
            new() { PermissionCode = "CABINET_INBOX_ACTION", PermissionName = "اقدام روی ورودی", Category = "CABINET", PermissionGroupId = cabinetGroup?.Id, DisplayOrder = 3, Description = "امکان انجام اقدام (ارجاع، پاسخ، ...) روی ورودی" },
            new() { PermissionCode = "CABINET_REFERRED_VIEW", PermissionName = "مشاهده کارتابل ارجاعی", Category = "CABINET", PermissionGroupId = cabinetGroup?.Id, DisplayOrder = 4, Description = "امکان مشاهده نامه‌ها و فرم‌های ارجاع داده شده" },
            new() { PermissionCode = "CABINET_OUTBOX_VIEW", PermissionName = "مشاهده کارتابل صادره", Category = "CABINET", PermissionGroupId = cabinetGroup?.Id, DisplayOrder = 5, Description = "امکان مشاهده نامه‌ها و فرم‌های صادره" },
            new() { PermissionCode = "CABINET_INTERNAL_VIEW", PermissionName = "مشاهده کارتابل داخلی", Category = "CABINET", PermissionGroupId = cabinetGroup?.Id, DisplayOrder = 6, Description = "امکان مشاهده نامه‌ها و فرم‌های داخلی" },
            
            // گردش کار
            
            // دبیرخانه
            new() { PermissionCode = "ARCHIVE_VIEW", PermissionName = "مشاهده دبیرخانه", Category = "ARCHIVE", PermissionGroupId = archiveGroup?.Id, DisplayOrder = 1, Description = "امکان مشاهده مدارک بایگانی شده" },
            new() { PermissionCode = "ARCHIVE_CREATE", PermissionName = "بایگانی مدرک", Category = "ARCHIVE", PermissionGroupId = archiveGroup?.Id, DisplayOrder = 2, Description = "امکان بایگانی کردن مدرک" },
            
            // جستجو
            new() { PermissionCode = "SEARCH_GLOBAL", PermissionName = "جستجوی سراسری", Category = "SEARCH", PermissionGroupId = searchGroup?.Id, DisplayOrder = 1, Description = "امکان جستجو در تمام سیستم" },
            new() { PermissionCode = "SEARCH_FORMS", PermissionName = "جستجو در فرم‌ها", Category = "SEARCH", PermissionGroupId = searchGroup?.Id, DisplayOrder = 2, Description = "امکان جستجو در فرم‌ها" },
            
            // گزارش
            new() { PermissionCode = "REPORT_VIEW", PermissionName = "مشاهده گزارش‌ها", Category = "REPORT", PermissionGroupId = reportGroup?.Id, DisplayOrder = 1, Description = "امکان مشاهده گزارش‌های تعریف شده" },
            new() { PermissionCode = "REPORT_EXECUTE", PermissionName = "اجرای گزارش", Category = "REPORT", PermissionGroupId = reportGroup?.Id, DisplayOrder = 2, Description = "امکان اجرا و مشاهده نتایج گزارش" },
            
            // تنظیمات
            new() { PermissionCode = "SETTINGS_USERS_VIEW", PermissionName = "مشاهده کاربران", Category = "SETTINGS", PermissionGroupId = settingsGroup?.Id, DisplayOrder = 1, Description = "امکان مشاهده لیست کاربران" },
            new() { PermissionCode = "SETTINGS_USERS_MANAGE", PermissionName = "مدیریت کاربران", Category = "SETTINGS", PermissionGroupId = settingsGroup?.Id, DisplayOrder = 2, Description = "امکان ایجاد/ویرایش/حذف کاربر" },
            new() { PermissionCode = "SETTINGS_ROLES_VIEW", PermissionName = "مشاهده نقش‌ها", Category = "SETTINGS", PermissionGroupId = settingsGroup?.Id, DisplayOrder = 3, Description = "امکان مشاهده لیست نقش‌ها" },
            new() { PermissionCode = "SETTINGS_ROLES_MANAGE", PermissionName = "مدیریت نقش‌ها", Category = "SETTINGS", PermissionGroupId = settingsGroup?.Id, DisplayOrder = 4, Description = "امکان ایجاد/ویرایش/حذف نقش" },
            new() { PermissionCode = "SETTINGS_GROUPS_VIEW", PermissionName = "مشاهده گروه‌ها", Category = "SETTINGS", PermissionGroupId = settingsGroup?.Id, DisplayOrder = 5, Description = "امکان مشاهده لیست گروه‌ها" },
            new() { PermissionCode = "SETTINGS_GROUPS_MANAGE", PermissionName = "مدیریت گروه‌ها", Category = "SETTINGS", PermissionGroupId = settingsGroup?.Id, DisplayOrder = 6, Description = "امکان ایجاد/ویرایش/حذف گروه" },
            new() { PermissionCode = "SETTINGS_PERMISSIONS_VIEW", PermissionName = "مشاهده مجوزها", Category = "SETTINGS", PermissionGroupId = settingsGroup?.Id, DisplayOrder = 7, Description = "امکان مشاهده لیست مجوزها" },
            new() { PermissionCode = "SETTINGS_PERMISSIONS_MANAGE", PermissionName = "مدیریت مجوزها", Category = "SETTINGS", PermissionGroupId = settingsGroup?.Id, DisplayOrder = 8, Description = "امکان ایجاد/ویرایش/حذف مجوز" },
            
            // داشبورد
            new() { PermissionCode = "DASHBOARD_VIEW", PermissionName = "مشاهده داشبورد", Category = "DASHBOARD", PermissionGroupId = dashboardGroup?.Id, DisplayOrder = 1, Description = "امکان مشاهده داشبورد اصلی" },
            new() { PermissionCode = "DASHBOARD_INBOX_CARD", PermissionName = "کارت کارتابل ورودی", Category = "DASHBOARD", PermissionGroupId = dashboardGroup?.Id, DisplayOrder = 2, Description = "نمایش کارت کارتابل ورودی در داشبورد" },
            new() { PermissionCode = "DASHBOARD_REFERRED_CARD", PermissionName = "کارت کارتابل ارجاعی", Category = "DASHBOARD", PermissionGroupId = dashboardGroup?.Id, DisplayOrder = 3, Description = "نمایش کارت کارتابل ارجاعی در داشبورد" }
        };

        await context.Permissions.AddRangeAsync(permissions);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// مجوزهای عمومی پیش‌فرض سیستم
    /// </summary>
    public static async Task SeedGeneralPermissionsAsync(IdentityDbContext context)
    {
        // Check if general permissions already exist
        var existingGeneralCount = await context.Permissions
            .IgnoreQueryFilters()
            .CountAsync(p => p.Category == "general" && !p.IsDeleted);

        if (existingGeneralCount > 0)
            return;

        var permissions = new List<Permission>
        {
            // === دسترسی و ورود ===
            new() { PermissionCode = "GEN_LOGIN", PermissionName = "مجوز ورود به سیستم", Category = "general", DisplayOrder = 1, Description = "امکان ورود و احراز هویت در سیستم" },
            new() { PermissionCode = "GEN_VIEW_DASHBOARD", PermissionName = "مجوز مشاهده داشبورد", Category = "general", DisplayOrder = 2, Description = "امکان مشاهده صفحه اصلی و داشبورد سیستم" },

            // === سازمان و ساختار ===
            new() { PermissionCode = "GEN_ORG_DEFINE", PermissionName = "مجوز معرفی سازمان", Category = "general", DisplayOrder = 10, Description = "امکان تعریف و ثبت سازمان جدید در سیستم" },
            new() { PermissionCode = "GEN_ORG_VIEW_ALL", PermissionName = "مجوز مشاهده لیست کلیه سازمان‌ها", Category = "general", DisplayOrder = 11, Description = "امکان مشاهده لیست تمام سازمان‌های ثبت شده" },
            new() { PermissionCode = "GEN_DEPT_MANAGE", PermissionName = "مجوز مدیریت واحدهای سازمانی", Category = "general", DisplayOrder = 12, Description = "امکان ایجاد، ویرایش و حذف واحدهای سازمانی" },
            new() { PermissionCode = "GEN_ROLE_DEFINE", PermissionName = "مجوز تعریف سمت‌های سازمانی", Category = "general", DisplayOrder = 13, Description = "امکان تعریف و مدیریت سمت‌های سازمانی" },

            // === مدیریت کاربران و گروه‌ها ===
            new() { PermissionCode = "GEN_USER_MANAGE", PermissionName = "مجوز مدیریت کاربران", Category = "general", DisplayOrder = 20, Description = "امکان ایجاد، ویرایش و حذف حساب‌های کاربری" },
            new() { PermissionCode = "GEN_GROUP_MANAGE", PermissionName = "مجوز مدیریت گروه‌ها", Category = "general", DisplayOrder = 21, Description = "امکان ایجاد و مدیریت گروه‌های کاربری" },
            new() { PermissionCode = "GEN_REFERRAL_MANAGE", PermissionName = "مجوز مدیریت ارجاعات افراد", Category = "general", DisplayOrder = 22, Description = "امکان مدیریت و پیگیری ارجاعات کاربران" },
            new() { PermissionCode = "GEN_PERSONNEL_MONITOR", PermissionName = "مجوز نظارت بر کارتابل پرسنل", Category = "general", DisplayOrder = 23, Description = "امکان مشاهده و نظارت بر کارتابل سایر کاربران" },

            // === دبیرخانه ===
            new() { PermissionCode = "GEN_SECRETARIAT_USE", PermissionName = "مجوز استفاده از دبیرخانه", Category = "general", DisplayOrder = 30, Description = "امکان دسترسی و استفاده از بخش دبیرخانه" },
            new() { PermissionCode = "GEN_SECRETARIAT_MANAGE", PermissionName = "مجوز مدیریت دبیرخانه‌ها", Category = "general", DisplayOrder = 31, Description = "امکان تعریف و مدیریت دبیرخانه‌ها" },

            // === اسناد و مدارک ===
            new() { PermissionCode = "GEN_DOC_CREATE", PermissionName = "مجوز استفاده از ایجاد مدرک", Category = "general", DisplayOrder = 40, Description = "امکان ایجاد مدرک جدید در سیستم" },
            new() { PermissionCode = "GEN_DOC_ENTER_DATA", PermissionName = "مجوز ورود اطلاعات در مدرک", Category = "general", DisplayOrder = 41, Description = "امکان وارد کردن اطلاعات در مدارک" },
            new() { PermissionCode = "GEN_DOC_SEARCH", PermissionName = "مجوز جستجوی اسناد", Category = "general", DisplayOrder = 42, Description = "امکان جستجو در اسناد و مدارک سیستم" },
            new() { PermissionCode = "GEN_DOC_PARAPH", PermissionName = "مجوز پاراف عمومی سازمان", Category = "general", DisplayOrder = 43, Description = "امکان پاراف زدن بر روی مدارک سازمانی" },
            new() { PermissionCode = "GEN_TOPIC_MANAGE", PermissionName = "مجوز مدیریت موضوعات", Category = "general", DisplayOrder = 44, Description = "امکان تعریف و مدیریت موضوعات مدارک" },
            new() { PermissionCode = "GEN_NUMBERING_MANAGE", PermissionName = "مجوز مدیریت الگوهای شماره‌گذاری", Category = "general", DisplayOrder = 45, Description = "امکان تعریف و مدیریت الگوهای شماره‌گذاری مدارک" },

            // === فرم‌ساز ===
            new() { PermissionCode = "GEN_FORM_DEFINE", PermissionName = "مجوز تعریف فرم‌های سازمانی", Category = "general", DisplayOrder = 50, Description = "امکان تعریف فرم‌های جدید در فرم‌ساز" },
            new() { PermissionCode = "GEN_FORM_EDIT_STRUCTURE", PermissionName = "مجوز ویرایش ساختار فرم‌های سازمانی", Category = "general", DisplayOrder = 51, Description = "امکان ویرایش ساختار و فیلدهای فرم‌ها" },
            new() { PermissionCode = "GEN_FORM_CLASSIFY", PermissionName = "مجوز ساختار طبقه‌بندی فرم‌های سازمانی", Category = "general", DisplayOrder = 52, Description = "امکان تعریف و مدیریت دسته‌بندی فرم‌ها" },

            // === گزارش‌ها ===
            new() { PermissionCode = "GEN_REPORT_CREATE", PermissionName = "مجوز ایجاد گزارش‌های کاربردی", Category = "general", DisplayOrder = 60, Description = "امکان ایجاد و طراحی گزارش‌های سفارشی" },

            // === امنیت ===
            new() { PermissionCode = "GEN_FIELD_SECURITY", PermissionName = "مجوز اعمال قانون امنیت فیلدها", Category = "general", DisplayOrder = 70, Description = "امکان تعریف سطوح دسترسی روی فیلدهای فرم" },

            // === ایمیل و فکس ===
            new() { PermissionCode = "GEN_EMAIL_CONFIG", PermissionName = "مجوز پیکربندی پست الکترونیکی", Category = "general", DisplayOrder = 80, Description = "امکان تنظیم سرویس پست الکترونیکی" },
            new() { PermissionCode = "GEN_EMAIL_MANAGE", PermissionName = "مجوز مدیریت ارسال‌های پست الکترونیکی", Category = "general", DisplayOrder = 81, Description = "امکان مدیریت و پیگیری ایمیل‌های ارسال شده" },
            new() { PermissionCode = "GEN_EMAIL_SEND_PLAIN", PermissionName = "مجوز ارسال پست الکترونیکی رمزنشده", Category = "general", DisplayOrder = 82, Description = "امکان ارسال ایمیل بدون رمزگذاری" },
            new() { PermissionCode = "GEN_FAX_CONFIG", PermissionName = "مجوز پیکربندی فکس", Category = "general", DisplayOrder = 83, Description = "امکان تنظیم سرویس فکس" },

            // === پیام‌رسانی ===
            new() { PermissionCode = "GEN_MESSAGE_RECEIVE", PermissionName = "مجوز دریافت از سیستم پیام", Category = "general", DisplayOrder = 90, Description = "امکان دریافت پیام‌های سیستمی و اعلان‌ها" },
            new() { PermissionCode = "GEN_MESSAGE_SEND", PermissionName = "مجوز ارسال پیام داخلی", Category = "general", DisplayOrder = 91, Description = "امکان ارسال پیام به سایر کاربران سیستم" },

            // === تنظیمات عمومی ===
            new() { PermissionCode = "GEN_SETTINGS_MANAGE", PermissionName = "مجوز مدیریت تنظیمات عمومی", Category = "general", DisplayOrder = 100, Description = "امکان تغییر تنظیمات عمومی سیستم" },
            new() { PermissionCode = "GEN_SOFTWARE_INFO", PermissionName = "مجوز مشاهده اطلاعات نرم‌افزار", Category = "general", DisplayOrder = 101, Description = "امکان مشاهده اطلاعات نسخه و لایسنس نرم‌افزار" },
        };

        await context.Permissions.AddRangeAsync(permissions);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// ایجاد مجوزهای فرمساز برای یک فرم خاص
    /// </summary>
    public static async Task SeedFormPermissionsAsync(IdentityDbContext context, int formId, string formNameFa, string formCode)
    {
        // Check if permissions already exist for this form
        var groupCode = $"FRMB_{formCode}";
        var existingGroup = await context.PermissionGroups
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(pg => pg.GroupCode == groupCode && !pg.IsDeleted);

        if (existingGroup != null)
            return;

        // Create permission group for this form
        var formPermGroup = new PermissionGroup
        {
            GroupCode = groupCode,
            GroupName = formNameFa,
            Icon = "bi-file-earmark-text",
            DisplayOrder = 100,
            Description = $"مجوزهای مربوط به فرم {formNameFa}"
        };
        context.PermissionGroups.Add(formPermGroup);
        await context.SaveChangesAsync();

        // Create form-specific permissions
        var prefix = $"FRMB_{formCode}";
        var permissions = new List<Permission>
        {
            new() { PermissionCode = $"{prefix}_DATA_ENTRY", PermissionName = $"مجوز ورود اطلاعات برای اسناد عادی از نوع فرم {formNameFa}", Category = "formbuilder", PermissionGroupId = formPermGroup.Id, DisplayOrder = 1 },
            new() { PermissionCode = $"{prefix}_VIEW", PermissionName = $"مجوز مشاهده اسناد عادی از نوع فرم {formNameFa}", Category = "formbuilder", PermissionGroupId = formPermGroup.Id, DisplayOrder = 2 },
            new() { PermissionCode = $"{prefix}_EDIT_DATA", PermissionName = $"مجوز ویرایش اطلاعات فرم {formNameFa}", Category = "formbuilder", PermissionGroupId = formPermGroup.Id, DisplayOrder = 3 },
            new() { PermissionCode = $"{prefix}_EDIT_LOCKED", PermissionName = $"مجوز ویرایش اطلاعات قفل شده در تمام اسناد عادی از نوع فرم {formNameFa}", Category = "formbuilder", PermissionGroupId = formPermGroup.Id, DisplayOrder = 4 },
            new() { PermissionCode = $"{prefix}_DELETE_DATA", PermissionName = $"مجوز حذف اطلاعات اسناد عادی از نوع فرم {formNameFa}", Category = "formbuilder", PermissionGroupId = formPermGroup.Id, DisplayOrder = 5 },
            new() { PermissionCode = $"{prefix}_DELETE", PermissionName = $"مجوز حذف فرم {formNameFa}", Category = "formbuilder", PermissionGroupId = formPermGroup.Id, DisplayOrder = 6 },
            new() { PermissionCode = $"{prefix}_DESIGN", PermissionName = $"مجوز طراحی فرم {formNameFa}", Category = "formbuilder", PermissionGroupId = formPermGroup.Id, DisplayOrder = 7 },
            new() { PermissionCode = $"{prefix}_EDIT_STRUCT", PermissionName = $"مجوز ویرایش ساختار فرم {formNameFa}", Category = "formbuilder", PermissionGroupId = formPermGroup.Id, DisplayOrder = 8 },
            new() { PermissionCode = $"{prefix}_PRINT", PermissionName = $"مجوز چاپ محلی فرم {formNameFa}", Category = "formbuilder", PermissionGroupId = formPermGroup.Id, DisplayOrder = 9 },
            new() { PermissionCode = $"{prefix}_SEARCH_PUBLIC", PermissionName = $"مجوز جستجو در اسناد غیرخصوصی عادی از نوع فرم {formNameFa}", Category = "formbuilder", PermissionGroupId = formPermGroup.Id, DisplayOrder = 10 },
            new() { PermissionCode = $"{prefix}_SEARCH_ALL", PermissionName = $"مجوز جستجوی کلیه اسناد عادی از نوع فرم {formNameFa}", Category = "formbuilder", PermissionGroupId = formPermGroup.Id, DisplayOrder = 11 },
            new() { PermissionCode = $"{prefix}_PRIVATE_MANAGE", PermissionName = $"مجوز مدیریت اسناد خصوصی عادی از نوع فرم {formNameFa}", Category = "formbuilder", PermissionGroupId = formPermGroup.Id, DisplayOrder = 12 },
            new() { PermissionCode = $"{prefix}_VIEW_FLOW", PermissionName = $"مجوز مشاهده گردش اسناد از نوع فرم {formNameFa}", Category = "formbuilder", PermissionGroupId = formPermGroup.Id, DisplayOrder = 13 },
            new() { PermissionCode = $"{prefix}_VIEW_HIDDEN_FLOW", PermissionName = $"مجوز مشاهده گردش مخفی اسناد از نوع فرم {formNameFa}", Category = "formbuilder", PermissionGroupId = formPermGroup.Id, DisplayOrder = 14 },
            new() { PermissionCode = $"{prefix}_VIEW_CHAIN", PermissionName = $"مجوز مشاهده زنجیره مدارک مرتبط با مدرک فرم {formNameFa}", Category = "formbuilder", PermissionGroupId = formPermGroup.Id, DisplayOrder = 15 },
            new() { PermissionCode = $"{prefix}_ADD_FOLLOW", PermissionName = $"مجوز افزودن پیرو به فرم {formNameFa}", Category = "formbuilder", PermissionGroupId = formPermGroup.Id, DisplayOrder = 16 },
            new() { PermissionCode = $"{prefix}_DEL_FOLLOW", PermissionName = $"مجوز حذف پیرو از فرم {formNameFa}", Category = "formbuilder", PermissionGroupId = formPermGroup.Id, DisplayOrder = 17 },
            new() { PermissionCode = $"{prefix}_ADD_ATTACH", PermissionName = $"مجوز افزودن پیوست به فرم {formNameFa}", Category = "formbuilder", PermissionGroupId = formPermGroup.Id, DisplayOrder = 18 },
            new() { PermissionCode = $"{prefix}_DEL_ATTACH", PermissionName = $"مجوز حذف پیوست از فرم {formNameFa}", Category = "formbuilder", PermissionGroupId = formPermGroup.Id, DisplayOrder = 19 },
            new() { PermissionCode = $"{prefix}_ADD_REF", PermissionName = $"مجوز افزودن عطف به فرم {formNameFa}", Category = "formbuilder", PermissionGroupId = formPermGroup.Id, DisplayOrder = 20 },
            new() { PermissionCode = $"{prefix}_DEL_REF", PermissionName = $"مجوز حذف عطف از فرم {formNameFa}", Category = "formbuilder", PermissionGroupId = formPermGroup.Id, DisplayOrder = 21 },
            new() { PermissionCode = $"{prefix}_ADD_RELATED", PermissionName = $"مجوز افزودن مدارک در ارتباط با فرم {formNameFa}", Category = "formbuilder", PermissionGroupId = formPermGroup.Id, DisplayOrder = 22 },
            new() { PermissionCode = $"{prefix}_DEL_RELATED", PermissionName = $"مجوز حذف مدارک در ارتباط با فرم {formNameFa}", Category = "formbuilder", PermissionGroupId = formPermGroup.Id, DisplayOrder = 23 },
            new() { PermissionCode = $"{prefix}_ADD_PARAPH", PermissionName = $"مجوز افزودن پاراف به فرم {formNameFa}", Category = "formbuilder", PermissionGroupId = formPermGroup.Id, DisplayOrder = 24 },
            new() { PermissionCode = $"{prefix}_ADD_HIDDEN_PARAPH", PermissionName = $"مجوز افزودن پاراف مخفی به فرم {formNameFa}", Category = "formbuilder", PermissionGroupId = formPermGroup.Id, DisplayOrder = 25 },
            new() { PermissionCode = $"{prefix}_VIEW_PARAPH", PermissionName = $"مجوز مشاهده لیست پاراف‌های مدرک فرم {formNameFa}", Category = "formbuilder", PermissionGroupId = formPermGroup.Id, DisplayOrder = 26 },
            new() { PermissionCode = $"{prefix}_VIEW_HIDDEN_PARAPH", PermissionName = $"مجوز مشاهده لیست پاراف‌های مخفی مدرک فرم {formNameFa}", Category = "formbuilder", PermissionGroupId = formPermGroup.Id, DisplayOrder = 27 },
            new() { PermissionCode = $"{prefix}_FAX", PermissionName = $"مجوز ارسال فاکس مدرک فرم {formNameFa}", Category = "formbuilder", PermissionGroupId = formPermGroup.Id, DisplayOrder = 28 },
            new() { PermissionCode = $"{prefix}_ADV_SEARCH", PermissionName = $"مجوز تنظیمات جستجوی پیشرفته برای فرم {formNameFa}", Category = "formbuilder", PermissionGroupId = formPermGroup.Id, DisplayOrder = 29 },
        };

        await context.Permissions.AddRangeAsync(permissions);
        await context.SaveChangesAsync();
    }

    public static async Task SeedAllAsync(IdentityDbContext identityContext, CoreDbContext coreContext, AutomationDbContext context)
    {
        await SeedFormCategoriesAsync(context);
        await SeedFieldTypesAsync(context);
        await SeedScalarFunctionsAsync(context);
        await SeedActionTypesAsync(coreContext);
        await SeedPermissionGroupsAsync(identityContext);
        await SeedSystemPermissionsAsync(identityContext);
        await SeedGeneralPermissionsAsync(identityContext);
        await SeedFormButtonTypesAsync(context);
        await SeedButtonStylePresetsAsync(context);
    }

    // ==========================================
    //     FORM BUTTON TYPES (18 predefined)
    // ==========================================

    public static async Task SeedFormButtonTypesAsync(AutomationDbContext context)
    {
        if (await context.FormButtonTypes.IgnoreQueryFilters().AnyAsync(t => !t.IsDeleted))
            return;

        var types = new List<FormButtonType>
        {
            new() { ButtonCode = "SAVE_CHANGES", NameFa = "ثبت تغییرات", NameEn = "Save Changes", DefaultIcon = "fa-solid fa-floppy-disk", DefaultColor = "btn-success", Category = "document", ActionHandler = "handleSaveChanges", DisplayOrder = 3, Description = "ذخیره تغییرات فرم" },
            new() { ButtonCode = "PRINT", NameFa = "چاپ", NameEn = "Print", DefaultIcon = "fa-solid fa-print", DefaultColor = "btn-outline-secondary", Category = "utility", ActionHandler = "handlePrint", OpensModal = true, ModalId = "printModal", DisplayOrder = 4, Description = "پیش‌نمایش و چاپ فرم" },
            new() { ButtonCode = "ATTACHMENT", NameFa = "پیوست", NameEn = "Attachment", DefaultIcon = "fa-solid fa-paperclip", DefaultColor = "btn-secondary", Category = "document", ActionHandler = "handleAttachment", OpensModal = true, ModalId = "enhancedAttachModal", DisplayOrder = 5, Description = "افزودن پیوست از فایل یا اسناد اتوماسیون" },
            new() { ButtonCode = "REFERENCE", NameFa = "عطف", NameEn = "Reference", DefaultIcon = "fa-solid fa-link", DefaultColor = "btn-secondary", Category = "document", ActionHandler = "handleReference", OpensModal = true, ModalId = "enhancedAttachModal", DisplayOrder = 6, Description = "عطف به سند دیگر" },
            new() { ButtonCode = "RELATED", NameFa = "در ارتباط", NameEn = "Related", DefaultIcon = "fa-solid fa-diagram-project", DefaultColor = "btn-info", Category = "document", ActionHandler = "handleRelated", OpensModal = true, ModalId = "enhancedAttachModal", DisplayOrder = 7, Description = "ارتباط با سند دیگر" },
            new() { ButtonCode = "FOLLOW_UP", NameFa = "پیرو", NameEn = "Follow-up", DefaultIcon = "fa-solid fa-reply", DefaultColor = "btn-info", Category = "document", ActionHandler = "handleFollowUp", OpensModal = true, ModalId = "enhancedAttachModal", DisplayOrder = 8, Description = "پیرو سند دیگر" },
            new() { ButtonCode = "DRAFT", NameFa = "پیش نویس", NameEn = "Draft", DefaultIcon = "fa-solid fa-file-pen", DefaultColor = "btn-outline-primary", Category = "document", ActionHandler = "handleDraft", DisplayOrder = 9, Description = "ذخیره بعنوان پیش نویس" },
            new() { ButtonCode = "PERSONAL_ARCHIVE", NameFa = "بایگانی شخصی", NameEn = "Personal Archive", DefaultIcon = "fa-solid fa-box-archive", DefaultColor = "btn-outline-dark", Category = "utility", ActionHandler = "handlePersonalArchive", DisplayOrder = 10, Description = "انتقال به بایگانی شخصی" },
            new() { ButtonCode = "DELETE_DOCUMENT", NameFa = "حذف مدرک", NameEn = "Delete Document", DefaultIcon = "fa-solid fa-trash", DefaultColor = "btn-danger", Category = "document", ActionHandler = "handleDeleteDocument", DisplayOrder = 11, Description = "حذف مدرک فعلی" },
            new() { ButtonCode = "SAVE_AS_NEW", NameFa = "ذخیره بعنوان مدرک جدید", NameEn = "Save as New Document", DefaultIcon = "fa-solid fa-file-circle-plus", DefaultColor = "btn-outline-success", Category = "document", ActionHandler = "handleSaveAsNew", DisplayOrder = 15, Description = "ایجاد مدرک جدید از فرم فعلی" },
            new() { ButtonCode = "CLOSE_WINDOW", NameFa = "بستن پنجره", NameEn = "Close Window", DefaultIcon = "fa-solid fa-xmark", DefaultColor = "btn-outline-danger", Category = "utility", ActionHandler = "handleCloseWindow", DisplayOrder = 16, Description = "بستن پنجره فرم" },
            new() { ButtonCode = "REGISTER_OUTGOING", NameFa = "ثبت صادره", NameEn = "Register Outgoing", DefaultIcon = "fa-solid fa-arrow-right-from-bracket", DefaultColor = "btn-primary", Category = "document", ActionHandler = "handleRegisterOutgoing", OpensModal = true, ModalId = "registerOutgoingModal", DisplayOrder = 17, Description = "ثبت نامه صادره" },
            new() { ButtonCode = "COPY_LIST", NameFa = "لیست رونوشت ها", NameEn = "Copy List", DefaultIcon = "fa-solid fa-copy", DefaultColor = "btn-outline-info", Category = "document", ActionHandler = "handleCopyList", OpensModal = true, ModalId = "copyListModal", DisplayOrder = 18, Description = "مدیریت رونوشت‌ها" },
        };

        await context.FormButtonTypes.AddRangeAsync(types);
        await context.SaveChangesAsync();
    }

    // ==========================================
    //     BUTTON STYLE PRESETS (4 system)
    // ==========================================

    public static async Task SeedButtonStylePresetsAsync(AutomationDbContext context)
    {
        if (await context.ButtonStylePresets.IgnoreQueryFilters().AnyAsync(p => !p.IsDeleted))
            return;

        var presets = new List<ButtonStylePreset>
        {
            new() { PresetName = "کلاسیک", Description = "استایل کلاسیک با حاشیه مشخص", IsSystem = true, DisplayOrder = 1,
                StyleDefinition = "{\"fontFamily\":\"Vazirmatn\",\"fontSize\":\"13px\",\"borderRadius\":\"4px\",\"padding\":\"6px 16px\",\"gap\":\"6px\"}" },
            new() { PresetName = "مدرن", Description = "استایل مدرن با سایه ملایم", IsSystem = true, DisplayOrder = 2,
                StyleDefinition = "{\"fontFamily\":\"Vazirmatn\",\"fontSize\":\"13px\",\"borderRadius\":\"8px\",\"padding\":\"8px 20px\",\"gap\":\"8px\",\"boxShadow\":\"0 2px 4px rgba(0,0,0,0.1)\"}" },
            new() { PresetName = "فلت", Description = "استایل تخت و ساده بدون حاشیه", IsSystem = true, DisplayOrder = 3,
                StyleDefinition = "{\"fontFamily\":\"Vazirmatn\",\"fontSize\":\"12px\",\"borderRadius\":\"0\",\"padding\":\"6px 14px\",\"gap\":\"4px\"}" },
            new() { PresetName = "گرد", Description = "استایل با گوشه‌های کاملا گرد", IsSystem = true, DisplayOrder = 4,
                StyleDefinition = "{\"fontFamily\":\"Vazirmatn\",\"fontSize\":\"13px\",\"borderRadius\":\"20px\",\"padding\":\"6px 20px\",\"gap\":\"8px\"}" },
        };

        await context.ButtonStylePresets.AddRangeAsync(presets);
        await context.SaveChangesAsync();
    }
}






using Automation.Core.Entities;
using System.Text;
using System.Xml.Linq;

namespace Automation.Infrastructure.Services;

public interface IFormXmlService
{
    string GenerateFormXml(Form form, List<FormField> fields);
}

public class FormXmlService : IFormXmlService
{
    public string GenerateFormXml(Form form, List<FormField> fields)
    {
        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", "yes"),
            new XElement("FormDefinition",
                new XAttribute("Code", form.Code),
                new XAttribute("NameEn", form.NameEn),
                new XAttribute("NameFa", form.NameFa),
                new XAttribute("Version", form.Version),
                new XElement("Description", form.Description),
                new XElement("Settings",
                     new XElement("LayoutType", form.LayoutType),
                     new XElement("CanvasSize", form.CanvasSize),
                     new XElement("Background", form.BackgroundSettings)
                ),
                new XElement("Fields",
                    fields.OrderBy(f => f.DisplayOrder).Select(f => GenerateFieldXml(f))
                ),
                new XElement("Scripts", form.CustomScripts),
                new XElement("Styles", form.CustomStyles)
            )
        );

        return doc.ToString();
    }

    private XElement GenerateFieldXml(FormField field)
    {
        return new XElement("Field",
            new XAttribute("Key", field.FieldKey),
            new XAttribute("Type", field.FieldType?.Name ?? "Unknown"),
            new XElement("Name", field.Name),
            new XElement("Label",
                new XElement("Fa", field.LabelFa),
                new XElement("En", field.LabelEn)
            ),
            new XElement("Database",
                new XAttribute("Column", field.DatabaseColumnName ?? ""),
                new XAttribute("Type", field.DatabaseColumnType ?? ""),
                new XAttribute("Nullable", field.IsNullable),
                new XAttribute("Index", field.HasIndex)
            ),
            new XElement("UI",
                new XAttribute("Hidden", field.IsHidden),
                new XAttribute("ReadOnly", field.IsReadOnly),
                new XAttribute("Disabled", field.IsDisabled),
                new XAttribute("Required", field.IsRequired),
                new XElement("Placeholder", field.Placeholder),
                new XElement("HelpText", field.HelpText),
                new XElement("Styles", field.Styles),
                new XElement("Properties", field.Properties)
            ),
            // Recursive children
            field.ChildFields != null && field.ChildFields.Any() 
                ? new XElement("Children", field.ChildFields.OrderBy(c => c.DisplayOrder).Select(c => GenerateFieldXml(c))) 
                : null
        );
    }
}




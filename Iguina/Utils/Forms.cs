using System.Collections.Generic;
using Iguina.Entities;

namespace Iguina.Utils
{
    /// <summary>Field type for a <see cref="FormFieldData"/>.</summary>
    public enum FormFieldType
    {
        /// <summary>Single-line text input.</summary>
        Text,
        /// <summary>Numeric spinner (Iguina's <see cref="NumericInput"/>).</summary>
        Numeric,
        /// <summary>Boolean checkbox.</summary>
        Checkbox,
        /// <summary>Choose one option from a list (DropDown).</summary>
        DropDown,
        /// <summary>Read-only paragraph block — useful for section dividers / hints.</summary>
        Paragraph,
    }

    /// <summary>Descriptor for one field in a <see cref="Form"/>.</summary>
    public class FormFieldData
    {
        /// <summary>Stable key used to read the value back via <see cref="Form.GetValue"/>.</summary>
        public string Key { get; set; } = string.Empty;
        /// <summary>Label shown above the input.</summary>
        public string Label { get; set; } = string.Empty;
        public FormFieldType Type { get; set; } = FormFieldType.Text;
        /// <summary>Initial value; for Checkbox use "true" / "false".</summary>
        public string DefaultValue { get; set; } = string.Empty;
        /// <summary>For <see cref="FormFieldType.DropDown"/>: the list of options.</summary>
        public IList<string>? DropDownChoices { get; set; }
    }

    /// <summary>
    /// Descriptor-driven form builder. Pass a list of <see cref="FormFieldData"/>
    /// and get back a <see cref="Panel"/> containing one label+input pair per
    /// field, plus an API to read the current values keyed by
    /// <see cref="FormFieldData.Key"/>. Minimal port of GeonBit.UI's Forms util.
    /// </summary>
    public class Form
    {
        readonly Dictionary<string, Entity> _entitiesByKey = new();

        /// <summary>Root panel containing all field rows. Caller adds it to the UI.</summary>
        public Panel Panel { get; }

        public Form(UISystem ui, IEnumerable<FormFieldData> fields)
        {
            Panel = new Panel(ui, ui.DefaultStylesheets.Panels) { AutoHeight = true };
            Panel.Size.X.SetPercents(100f);
            foreach (var f in fields) AddField(ui, f);
        }

        void AddField(UISystem ui, FormFieldData field)
        {
            // label
            Panel.AddChild(new Paragraph(ui, field.Label));

            switch (field.Type)
            {
                case FormFieldType.Text:
                {
                    var input = new TextInput(ui) { Value = field.DefaultValue };
                    Panel.AddChild(input);
                    _entitiesByKey[field.Key] = input;
                    break;
                }
                case FormFieldType.Numeric:
                {
                    var input = new NumericInput(ui) { Value = field.DefaultValue };
                    Panel.AddChild(input);
                    _entitiesByKey[field.Key] = input;
                    break;
                }
                case FormFieldType.Checkbox:
                {
                    var cb = new Checkbox(ui, field.Label) { Checked = field.DefaultValue == "true" };
                    Panel.AddChild(cb);
                    _entitiesByKey[field.Key] = cb;
                    break;
                }
                case FormFieldType.DropDown:
                {
                    var dd = new DropDown(ui);
                    if (field.DropDownChoices != null)
                    {
                        foreach (var c in field.DropDownChoices) dd.AddItem(c);
                    }
                    if (!string.IsNullOrEmpty(field.DefaultValue)) dd.SelectedValue = field.DefaultValue;
                    Panel.AddChild(dd);
                    _entitiesByKey[field.Key] = dd;
                    break;
                }
                case FormFieldType.Paragraph:
                {
                    var p = new Paragraph(ui, field.DefaultValue);
                    Panel.AddChild(p);
                    _entitiesByKey[field.Key] = p;
                    break;
                }
            }
        }

        /// <summary>Read the current text value of the field with this key.
        /// For Checkbox returns "true" / "false"; for DropDown returns the
        /// selected option (or empty if none); for Paragraph returns the
        /// rendered text.</summary>
        public string GetValue(string key)
        {
            if (!_entitiesByKey.TryGetValue(key, out var entity)) return string.Empty;
            return entity switch
            {
                TextInput t => t.Value,
                Checkbox c => c.Checked ? "true" : "false",
                DropDown d => d.SelectedValue ?? string.Empty,
                Paragraph p => p.Text,
                _ => string.Empty,
            };
        }

        /// <summary>True iff the field is a checked Checkbox.</summary>
        public bool GetBool(string key)
            => _entitiesByKey.TryGetValue(key, out var e) && e is Checkbox c && c.Checked;

        /// <summary>Snapshot every field's current value as a flat string map.</summary>
        public Dictionary<string, string> GetAllValues()
        {
            var ret = new Dictionary<string, string>();
            foreach (var key in _entitiesByKey.Keys) ret[key] = GetValue(key);
            return ret;
        }
    }
}

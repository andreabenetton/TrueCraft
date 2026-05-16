using System.Collections.Generic;
using System.Globalization;
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
        /// <summary>Integer slider with Min/Max range from the descriptor.</summary>
        Slider,
        /// <summary>Mutually-exclusive radio-button group from <see cref="FormFieldData.DropDownChoices"/>.</summary>
        RadioButtons,
        /// <summary>Visual section header (bold Title); no input value.</summary>
        Section,
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
        /// <summary>For <see cref="FormFieldType.DropDown"/> / <see cref="FormFieldType.RadioButtons"/>: the list of options.</summary>
        public IList<string>? DropDownChoices { get; set; }
        /// <summary>For <see cref="FormFieldType.Slider"/> / <see cref="FormFieldType.Numeric"/>: minimum allowed value.</summary>
        public int Min { get; set; } = 0;
        /// <summary>For <see cref="FormFieldType.Slider"/> / <see cref="FormFieldType.Numeric"/>: maximum allowed value.</summary>
        public int Max { get; set; } = 100;
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
        // For RadioButtons we store the list of radio entities + the original choices.
        readonly Dictionary<string, (List<RadioButton> radios, IList<string> choices)> _radioGroups = new();

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
            // For Section, render the label as a bold Title and skip the input.
            if (field.Type == FormFieldType.Section)
            {
                Panel.AddChild(new Title(ui, field.Label));
                return;
            }

            // For all other field types, show a label paragraph above the input.
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
                    input.MinValue = field.Min;
                    input.MaxValue = field.Max;
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
                    if (field.DropDownChoices is not null)
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
                case FormFieldType.Slider:
                {
                    var slider = new Slider(ui)
                    {
                        MinValue = field.Min,
                        MaxValue = field.Max,
                    };
                    if (int.TryParse(field.DefaultValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
                        slider.Value = System.Math.Clamp(v, field.Min, field.Max);
                    Panel.AddChild(slider);
                    _entitiesByKey[field.Key] = slider;
                    break;
                }
                case FormFieldType.RadioButtons:
                {
                    var radios = new List<RadioButton>();
                    if (field.DropDownChoices is not null)
                    {
                        foreach (var choice in field.DropDownChoices)
                        {
                            var rb = new RadioButton(ui, choice)
                            {
                                ExclusiveSelection = true,
                                Checked = choice == field.DefaultValue,
                            };
                            Panel.AddChild(rb);
                            radios.Add(rb);
                        }
                    }
                    _radioGroups[field.Key] = (radios, field.DropDownChoices ?? new List<string>());
                    break;
                }
            }
        }

        /// <summary>Read the current text value of the field with this key.
        /// For Checkbox returns "true" / "false"; for DropDown / RadioButtons
        /// returns the selected option (or empty if none); for Slider /
        /// NumericInput returns the integer as a string; for Paragraph returns
        /// the rendered text.</summary>
        public string GetValue(string key)
        {
            if (_radioGroups.TryGetValue(key, out var group))
            {
                for (var i = 0; i < group.radios.Count; i++)
                    if (group.radios[i].Checked) return group.choices[i];
                return string.Empty;
            }
            if (!_entitiesByKey.TryGetValue(key, out var entity)) return string.Empty;
            return entity switch
            {
                TextInput t => t.Value,
                Checkbox c => c.Checked ? "true" : "false",
                DropDown d => d.SelectedValue ?? string.Empty,
                Slider s => s.Value.ToString(CultureInfo.InvariantCulture),
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
            foreach (var key in _radioGroups.Keys) ret[key] = GetValue(key);
            return ret;
        }
    }
}

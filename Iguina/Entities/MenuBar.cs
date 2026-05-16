using System;
using System.Collections.Generic;
using Iguina.Defs;

namespace Iguina.Entities
{
    /// <summary>
    /// Single entry in a <see cref="MenuBar"/> dropdown. Clicking it closes the
    /// parent dropdown and invokes <see cref="Action"/>.
    /// </summary>
    public class MenuItem
    {
        public string Label { get; }
        public Action? Action { get; }
        public bool Enabled { get; set; } = true;

        public MenuItem(string label, Action? action = null)
        {
            Label = label;
            Action = action;
        }
    }

    /// <summary>
    /// Horizontal menu bar with one-level dropdowns. Each top-level entry is a
    /// button that opens a dropdown panel beneath it; clicking a sub-item closes
    /// the dropdown and runs the item's action; clicking outside any dropdown
    /// also closes it. A minimal port of GeonBit.UI's MenuBar — covers the
    /// common "File / Edit / Help" pattern; deeper nesting is intentionally not
    /// supported (callers can compose with <see cref="DropDown"/> for that).
    /// </summary>
    public class MenuBar : Panel
    {
        Panel? _openDropdown;
        readonly List<(Button button, Panel dropdown)> _topLevel = new();

        public MenuBar(UISystem system) : base(system, system.DefaultStylesheets.Panels)
        {
            Size.X.SetPercents(100f);
            AutoHeight = true;

            // close any open dropdown when the user clicks anywhere not on it
            system.Events.OnLeftMousePressed += MaybeCloseOnOutsideClick;
        }

        /// <summary>Add a top-level menu and its dropdown contents.</summary>
        public void AddItem(string label, IEnumerable<MenuItem> items)
        {
            var topButton = new Button(UISystem, label) { Anchor = Anchor.AutoInlineLTR };
            var dropdown = new Panel(UISystem, UISystem.DefaultStylesheets.Panels)
            {
                Anchor = Anchor.TopLeft,
                Visible = false,
                Identifier = "MenuBar-Dropdown",
            };
            dropdown.Size.X.SetPixels(220);
            dropdown.AutoHeight = true;

            foreach (var item in items)
            {
                var entryButton = new Button(UISystem, item.Label)
                {
                    Anchor = Anchor.AutoLTR,
                    Enabled = item.Enabled,
                };
                entryButton.Events.OnClick = _ =>
                {
                    item.Action?.Invoke();
                    CloseOpenDropdown();
                };
                dropdown.AddChild(entryButton);
            }

            topButton.Events.OnClick = _ =>
            {
                var willOpen = _openDropdown != dropdown;
                CloseOpenDropdown();
                if (willOpen)
                {
                    dropdown.Offset.X.SetPixels(topButton.LastBoundingRect.X);
                    dropdown.Offset.Y.SetPixels(topButton.LastBoundingRect.Bottom);
                    dropdown.Visible = true;
                    dropdown.BringToFront();
                    _openDropdown = dropdown;
                }
            };

            AddChild(topButton);
            UISystem.Root.AddChild(dropdown);
            _topLevel.Add((topButton, dropdown));
        }

        void CloseOpenDropdown()
        {
            if (_openDropdown == null) return;
            _openDropdown.Visible = false;
            _openDropdown = null;
        }

        void MaybeCloseOnOutsideClick(Entity clicked)
        {
            if (_openDropdown == null) return;
            // walk up from the clicked entity; if we hit the open dropdown we
            // were clicked inside, leave it alone
            for (var e = clicked; e != null; e = e.Parent)
            {
                if (e == _openDropdown) return;
            }
            // also leave alone if the click was on one of our top-level buttons —
            // that button will handle the toggle in its own OnClick
            foreach (var (button, _) in _topLevel)
            {
                if (clicked == button) return;
            }
            CloseOpenDropdown();
        }
    }
}

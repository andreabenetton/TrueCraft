using System;
using System.Collections.Generic;
using Iguina.Defs;

namespace Iguina.Entities;

/// <summary>
/// Entry in a <see cref="MenuBar"/>. A leaf has an <see cref="Action"/>; a
/// parent has <see cref="SubItems"/> and opens a cascade dropdown when
/// clicked. Mixing is fine — a parent can still carry an Action (invoked
/// alongside opening the submenu) though that's unusual.
/// </summary>
public class MenuItem
{
    public string Label { get; }
    public Action? Action { get; }
    public IList<MenuItem>? SubItems { get; }
    public bool Enabled { get; set; } = true;

    /// <summary>Leaf item with an action.</summary>
    public MenuItem(string label, Action? action = null)
    {
        Label = label;
        Action = action;
    }

    /// <summary>Parent item that opens a cascade submenu.</summary>
    public MenuItem(string label, IEnumerable<MenuItem> subItems)
    {
        Label = label;
        SubItems = new List<MenuItem>(subItems);
    }
}

/// <summary>
/// Horizontal menu bar with arbitrary-depth cascade dropdowns. Each top-level
/// entry is a button that opens a dropdown beneath it; clicking a parent
/// item inside any dropdown opens a side-positioned cascade to its right;
/// clicking a leaf item runs the item's action and closes the entire chain.
/// Clicking anywhere outside the open chain also closes the chain.
///
/// A port of GeonBit.UI's MenuBar covering the common menu-stack patterns:
/// File → New → World/Project, etc. Open-on-hover (vs the click-to-open
/// model used here) is intentionally not implemented — predictability beats
/// MacOS menu-bar mimicry.
/// </summary>
public class MenuBar : Panel
{
    // The currently-open chain of dropdown panels, from outermost (a
    // top-level dropdown) to innermost (deepest cascade). Empty when closed.
    readonly List<Panel> _openChain = new();

    // Maps each open dropdown to the button that anchors it (so outside-click
    // can leave the chain alone if the click landed on one of those buttons).
    readonly Dictionary<Panel, Button> _anchorButton = new();

    public MenuBar(UISystem system) : base(system, system.DefaultStylesheets.Panels)
    {
        Size.X.SetPercents(100f);
        AutoHeight = true;
        system.Events.OnLeftMousePressed += MaybeCloseOnOutsideClick;
    }

    /// <summary>Add a top-level menu (label + dropdown contents).</summary>
    public void AddItem(string label, IEnumerable<MenuItem> items)
        => AddItem(new MenuItem(label, items));

    /// <summary>Add a top-level menu from a MenuItem with SubItems.</summary>
    public void AddItem(MenuItem rootItem)
    {
        if (rootItem.SubItems is null)
            throw new ArgumentException("Top-level MenuBar item must have SubItems", nameof(rootItem));

        var topButton = new Button(UISystem, rootItem.Label) { Anchor = Anchor.AutoInlineLTR };
        var dropdown = BuildDropdown(rootItem.SubItems, level: 0);
        _anchorButton[dropdown] = topButton;

        topButton.Events.OnClick = _ =>
        {
            var alreadyOpen = _openChain.Count > 0 && _openChain[0] == dropdown;
            CloseChainFromLevel(0);
            if (!alreadyOpen)
            {
                dropdown.Offset.X.SetPixels(topButton.LastBoundingRect.X);
                dropdown.Offset.Y.SetPixels(topButton.LastBoundingRect.Bottom);
                OpenAtLevel(dropdown, 0);
            }
        };

        AddChild(topButton);
        UISystem.Root.AddChild(dropdown);
    }

    Panel BuildDropdown(IList<MenuItem> items, int level)
    {
        var dropdown = new Panel(UISystem, UISystem.DefaultStylesheets.Panels)
        {
            Anchor = Anchor.TopLeft,
            Visible = false,
            Identifier = $"MenuBar-Dropdown-L{level}",
        };
        dropdown.Size.X.SetPixels(220);
        dropdown.AutoHeight = true;

        foreach (var item in items)
        {
            // Label suffix '▶' makes cascade parents visually distinct from leaves.
            var displayLabel = item.SubItems is not null ? $"{item.Label}  ▶" : item.Label;
            var entryButton = new Button(UISystem, displayLabel)
            {
                Anchor = Anchor.AutoLTR,
                Enabled = item.Enabled,
            };

            if (item.SubItems is null)
            {
                // Leaf: run action + close entire chain
                entryButton.Events.OnClick = _ =>
                {
                    item.Action?.Invoke();
                    CloseChainFromLevel(0);
                };
            }
            else
            {
                // Parent: open cascade to the right of this button
                var childLevel = level + 1;
                var childDropdown = BuildDropdown(item.SubItems, childLevel);
                _anchorButton[childDropdown] = entryButton;
                UISystem.Root.AddChild(childDropdown);

                entryButton.Events.OnClick = _ =>
                {
                    item.Action?.Invoke();
                    var alreadyOpen = _openChain.Count > childLevel && _openChain[childLevel] == childDropdown;
                    CloseChainFromLevel(childLevel);
                    if (!alreadyOpen)
                    {
                        childDropdown.Offset.X.SetPixels(entryButton.LastBoundingRect.Right);
                        childDropdown.Offset.Y.SetPixels(entryButton.LastBoundingRect.Y);
                        OpenAtLevel(childDropdown, childLevel);
                    }
                };
            }

            dropdown.AddChild(entryButton);
        }
        return dropdown;
    }

    void OpenAtLevel(Panel dropdown, int level)
    {
        while (_openChain.Count > level) _openChain.RemoveAt(_openChain.Count - 1);
        dropdown.Visible = true;
        dropdown.BringToFront();
        _openChain.Add(dropdown);
    }

    void CloseChainFromLevel(int level)
    {
        while (_openChain.Count > level)
        {
            _openChain[_openChain.Count - 1].Visible = false;
            _openChain.RemoveAt(_openChain.Count - 1);
        }
    }

    void MaybeCloseOnOutsideClick(Entity clicked)
    {
        if (_openChain.Count == 0) return;

        // If the click landed inside any open dropdown in the chain, leave it.
        for (var e = clicked; e is not null; e = e.Parent)
        {
            if (_openChain.Contains((e as Panel)!)) return;
        }
        // Anchor buttons (top-level and cascade-parent buttons) handle their
        // own toggle/open behaviour — don't close out from under them.
        foreach (var anchor in _anchorButton.Values)
        {
            if (clicked == anchor) return;
        }
        CloseChainFromLevel(0);
    }
}

using System;
using System.Collections.Generic;
using Iguina.Defs;


namespace Iguina.Entities;

/// <summary>
/// Multi-tab container: a horizontal strip of tab-buttons at the top, plus
/// a body panel that switches between one body-panel per tab when the
/// corresponding tab-button is clicked.
///
/// API mirrors GeonBit.UI's PanelTabs. Reuses Iguina's existing
/// <see cref="Button"/> for tab heads and <see cref="Panel"/> for bodies —
/// no new primitives, just composition. The currently-active tab is
/// marked Checked so a theme can style it distinctly.
/// </summary>
public class PanelTabs : Panel
{
    /// <summary>The header strip containing the tab buttons.</summary>
    public readonly Panel TabsStrip;

    /// <summary>The container that holds all body panels (one shown at a time).</summary>
    public readonly Panel BodyContainer;

    readonly List<TabData> _tabs = new();
    int _activeIndex = -1;

    /// <summary>One tab's elements.</summary>
    public readonly struct TabData
    {
        public readonly Button TabButton;
        public readonly Panel BodyPanel;
        public TabData(Button b, Panel p) { TabButton = b; BodyPanel = p; }
    }

    public PanelTabs(UISystem system) : base(system, system.DefaultStylesheets.Panels)
    {
        Size.X.SetPercents(100f);
        AutoHeight = true;

        TabsStrip = new Panel(system, system.DefaultStylesheets.Panels);
        TabsStrip.Size.X.SetPercents(100f);
        TabsStrip.AutoHeight = true;
        AddChild(TabsStrip);

        BodyContainer = new Panel(system, system.DefaultStylesheets.Panels);
        BodyContainer.Size.X.SetPercents(100f);
        BodyContainer.AutoHeight = true;
        AddChild(BodyContainer);
    }

    /// <summary>
    /// Add a new tab with the given label. Returns the freshly-created body
    /// panel for the caller to populate. The first tab added becomes the
    /// active one automatically.
    /// </summary>
    public Panel AddTab(string label)
    {
        var tabButton = new Button(UISystem, label) { Anchor = Anchor.AutoInlineLTR };
        var bodyPanel = new Panel(UISystem, UISystem.DefaultStylesheets.Panels)
        {
            Anchor = Anchor.AutoLTR,
            Visible = false,
        };
        bodyPanel.Size.X.SetPercents(100f);
        bodyPanel.AutoHeight = true;

        var index = _tabs.Count;
        tabButton.Events.OnClick = _ => SetActive(index);
        TabsStrip.AddChild(tabButton);
        BodyContainer.AddChild(bodyPanel);

        _tabs.Add(new TabData(tabButton, bodyPanel));
        if (_activeIndex == -1) SetActive(0);
        return bodyPanel;
    }

    /// <summary>Switch to the tab at the given index. No-op if out of range.</summary>
    public void SetActive(int index)
    {
        if (index < 0 || index >= _tabs.Count) return;
        for (var i = 0; i < _tabs.Count; i++)
        {
            var (btn, body) = (_tabs[i].TabButton, _tabs[i].BodyPanel);
            body.Visible = (i == index);
            btn.Checked = (i == index);
        }
        _activeIndex = index;
    }

    /// <summary>Index of the currently active tab, or -1 if none.</summary>
    public int ActiveIndex => _activeIndex;

    /// <summary>All tabs in insertion order.</summary>
    public IReadOnlyList<TabData> Tabs => _tabs;
}

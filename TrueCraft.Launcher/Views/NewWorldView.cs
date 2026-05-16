using System;
using Iguina.Defs;
using Iguina.Entities;
using Iguina.Utils;
using TrueCraft.Core.World;
using TrueCraft.Launcher.Singleplayer;

namespace TrueCraft.Launcher.Views;

/// <summary>
///     Dedicated screen for creating a new world. Replaces the inline create-world
///     panel that previously lived inside <see cref="SingleplayerView"/>.
/// </summary>
public sealed class NewWorldView : ILauncherView
{
    private readonly LauncherGame _game;
    private readonly Action<World> _onCreated;
    private TextInput _name;
    private TextInput _seed;
    private Button _createButton;

    /// <param name="game"> Shared LauncherGame instance. </param>
    /// <param name="onCreated">
    ///     Invoked after a world is created successfully. Typically navigates back to
    ///     <see cref="SingleplayerView"/>.
    /// </param>
    public NewWorldView(LauncherGame game, Action<World> onCreated)
    {
        _game = game;
        _onCreated = onCreated;
    }

    public void Mount(Panel parent)
    {
        parent.AddChild(new Title(_game.UI, "New world"));
        parent.AddChild(new HorizontalLine(_game.UI));

        parent.AddChild(new Paragraph(_game.UI, "Name"));
        _name = new TextInput(_game.UI) { PlaceholderText = "World name" };
        _name.Validators.Add(TextInputValidators.EnglishCharactersOnly(allowSpaces: true));
        _name.Validators.Add(TextInputValidators.OnlySingleSpaces());
        parent.AddChild(_name);

        parent.AddChild(new Paragraph(_game.UI, "Seed (optional)"));
        _seed = new TextInput(_game.UI) { PlaceholderText = "Seed" };
        parent.AddChild(_seed);

        parent.AddChild(new RowsSpacer(_game.UI));

        _createButton = new Button(_game.UI, "Create")
        {
            Anchor = Anchor.AutoCenter,
            Enabled = false,
        };
        _createButton.Events.OnClick = _ => CommitCreate();
        parent.AddChild(_createButton);

        var cancel = new Button(_game.UI, "Cancel") { Anchor = Anchor.AutoCenter };
        cancel.Events.OnClick = _ => _game.ShowView(new SingleplayerView(_game));
        parent.AddChild(cancel);

        // Enable Create only when a non-empty name is present.
        _name.Events.OnValueChanged = _ =>
            _createButton.Enabled = !string.IsNullOrWhiteSpace(_name.Value);
    }

    private void CommitCreate()
    {
        var trimmed = _name.Value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return;
        var world = Worlds.Local.CreateNewWorld(trimmed, _seed.Value);
        _onCreated?.Invoke(world);
    }

    public void Dispose() { }
}

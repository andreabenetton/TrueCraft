using System;
using GeonBit.UI.Entities;
using GeonBit.UI.Entities.TextValidators;
using Microsoft.Xna.Framework;
using TrueCraft.Core.World;
using TrueCraft.Launcher.Entities;
using TrueCraft.Launcher.Singleplayer;

namespace TrueCraft.Launcher.Views
{
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
        private MenuButton _createButton;

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
            parent.AddChild(new Header("New world"));
            parent.AddChild(new HorizontalLine());

            parent.AddChild(new Label("Name"));
            _name = new TextInput(false) { PlaceholderText = "World name" };
            _name.Validators.Add(new EnglishCharactersOnly(true));
            _name.Validators.Add(new OnlySingleSpaces());
            parent.AddChild(_name);

            parent.AddChild(new Label("Seed (optional)"));
            _seed = new TextInput(false) { PlaceholderText = "Seed" };
            parent.AddChild(_seed);

            parent.AddChild(new LineSpace());

            _createButton = new MenuButton("Create", anchor: Anchor.Auto) { Enabled = false };
            _createButton.OnClick = _ => CommitCreate();
            parent.AddChild(_createButton);

            var cancel = new MenuButton("Cancel", Anchor.Auto);
            cancel.OnClick = _ => _game.ShowView(new SingleplayerView(_game));
            parent.AddChild(cancel);

            // Enable Create only when a non-empty name is present.
            _name.OnValueChange = _ =>
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
}

using GeonBit.UI;
using GeonBit.UI.Entities;
using GeonBit.UI.Entities.TextValidators;
using TrueCraft.Core.World;
using TrueCraft.Launcher.Singleplayer;

namespace TrueCraft.Launcher.Panels
{
    public class NewWorldPanel: BackgroundManagingPanel
    {
        private TextInput _newWorldName;
        private TextInput _newWorldSeed;
        private Button _newWorldCommit;
        private Button _newWorldCancel;

        public EventCallback OnCancel = null;
        public EventCallback OnCommit = null;

        private World _world;
        public NewWorldPanel(LauncherGame game) : base(game)
        {
            _newWorldName = new TextInput { PlaceholderText = "Name" };
            _newWorldName.Validators.Add(new EnglishCharactersOnly(true));
            _newWorldName.Validators.Add(new OnlySingleSpaces());

            _newWorldSeed = new TextInput { PlaceholderText = "Seed (optional)" };
            _newWorldCommit = new Button("Create");
            _newWorldCancel = new Button("Cancel", ButtonSkin.Default, Anchor.AutoInline);

            _newWorldCancel.OnClick += (sender) => { OnCancel?.Invoke(this); };
            _newWorldCommit.OnClick += (sender) =>
            {
                _world = Worlds.Local.CreateNewWorld(_newWorldName.Value.Trim(), _newWorldSeed.Value.Trim());
                OnCommit?.Invoke(this);
            };
            _newWorldName.OnValueChange += (sender) =>
            {
                _newWorldCommit.Enabled = !string.IsNullOrEmpty(_newWorldName.Value.Trim());
            };

            AddChild(new Header("New World"));
            AddChild(new HorizontalLine());
            AddChild(_newWorldName);
            AddChild(_newWorldSeed);
            AddChild(_newWorldCommit);
            AddChild(_newWorldCancel);
        }
        public World World => _world;
    }
}

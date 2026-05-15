using System;
using System.Collections.Generic;
using System.Net.Http;
using GeonBit.UI;
using GeonBit.UI.Entities;
using GeonBit.UI.Entities.TextValidators;
using Microsoft.Xna.Framework;
using TrueCraft.Core;

// using GeonBit UI elements

namespace TrueCraft.Launcher.Panels
{
    public class LoginPanel : BackgroundManagingPanel
    {
        private readonly LauncherGame _game;
        private readonly TextInput _usernameText;
        private readonly TextInput _passwordText;
        private readonly CheckBox _rememberCheckBox;
        private readonly Button _logInButton;
        private readonly Button _registerButton;
        private readonly Button _offlineButton;
        private readonly Button _backButton;
        private readonly Label _errorLabel;
        private readonly int panelWidth = 730;

        public EventCallback OnBack = null;
        public EventCallback OnLogin = null;

        public LoginPanel(LauncherGame game) : base(game)
        {
            _game = game;
            Size = new Vector2(panelWidth, -1);

            // Control creations
            _usernameText = new TextInput(false, new Vector2(0.4f, -1), anchor: Anchor.Auto);
            _passwordText = new TextInput(false, new Vector2(0.4f, -1), anchor: Anchor.AutoInline);
            _rememberCheckBox = new CheckBox("Remember Me", isChecked: true);
            _logInButton = new Button("Log In");
            _registerButton = new Button("Register");
            _offlineButton = new Button("Play Offline");
            _errorLabel = new Label("Username or password incorrect");
            _backButton = new Button("Back", ButtonSkin.Default, Anchor.AutoInline);

            // Controls Initialization
            _errorLabel.OutlineColor = new Color(255, 0, 0);
            _errorLabel.AlignToCenter = true;
            _errorLabel.Visible = false;
            
            _usernameText.Value = UserSettings.Local.Username;
            _usernameText.PlaceholderText = "Enter username..";
            _usernameText.Validators.Add(new EnglishCharactersOnly(true));
            _usernameText.Validators.Add(new OnlySingleSpaces());
            _usernameText.Validators.Add(new MakeTitleCase());
            
            _passwordText.PlaceholderText = "Enter password..";
            if (UserSettings.Local.AutoLogin)
            {
                _passwordText.Value = UserSettings.Local.Password;
                _rememberCheckBox.Checked = true;
            }
            _passwordText.HideInputWithChar = '*';


            _offlineButton.OnClick += _ =>
            {
                _game.User.Username = _usernameText.Value;
                _game.User.SessionId = "-";
                OnLogin?.Invoke(this);
            };

            _backButton.OnClick += _ =>
            {
                OnBack?.Invoke(this);
            };

            // Events
            _logInButton.OnClick += LogInButton_Clicked;

            // Add controls
            AddChild(new Header("Login"));
            AddChild(new HorizontalLine());
            AddChild(_usernameText);
            AddChild(_passwordText);
            AddChild(_rememberCheckBox);
            AddChild(_logInButton);
            AddChild(_offlineButton);
            AddChild(_registerButton);
            AddChild(_backButton);
        }

        private void DisableForm()
        {
            _usernameText.Enabled = _passwordText.Enabled = _logInButton.Enabled = _rememberCheckBox.Enabled = _backButton.Enabled =
                _registerButton.Enabled = _offlineButton.Enabled = false;
        }

        private void EnableForm()
        {
            _usernameText.Enabled = _passwordText.Enabled = _logInButton.Enabled = _rememberCheckBox.Enabled = _backButton.Enabled =
                _registerButton.Enabled = _offlineButton.Enabled = true;
        }

		private async void LogInButton_Clicked(Entity e)
		{
			if (string.IsNullOrEmpty(_usernameText.Value) || string.IsNullOrEmpty(_passwordText.Value))
			{
				_errorLabel.Text = "Username and password are required";
				_errorLabel.Visible = true;
				return;
			}

			_errorLabel.Visible = false;
			DisableForm();

			_game.User.Username = _usernameText.Value;

			try
			{
				using (HttpClient client = new HttpClient())
				{
					var requestUri = TrueCraftUser.AuthServer + "/api/login";
					var content = new FormUrlEncodedContent(new[]
					{
				new KeyValuePair<string, string>("user", _usernameText.Value),
				new KeyValuePair<string, string>("password", _passwordText.Value),
				new KeyValuePair<string, string>("version", "12")
			});

					HttpResponseMessage response = await client.PostAsync(requestUri, content);

					if (response.IsSuccessStatusCode)
					{
						string session = await response.Content.ReadAsStringAsync();
						HandleLoginResponse(session);
					}
					else
					{
						EnableForm();
						_errorLabel.Text = "Unable to log in";
						_errorLabel.Visible = true;
						_registerButton.ButtonParagraph = UserInterface.DefaultParagraph("Offline Mode", Anchor.Center);
					}
				}
			}
			catch
			{
				EnableForm();
				_errorLabel.Text = "Unable to log in";
				_errorLabel.Visible = true;
				_registerButton.ButtonParagraph = UserInterface.DefaultParagraph("Offline Mode", Anchor.Center);
			}
		}

		private void HandleLoginResponse(string session)
		{
			if (session.Contains(":"))
			{
				var parts = session.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

				_game.User.Username = parts[2];
				_game.User.SessionId = parts[3];
				EnableForm();

				UserSettings.Local.AutoLogin = _rememberCheckBox.Checked;
				UserSettings.Local.Username = _game.User.Username;
				UserSettings.Local.Password =
					UserSettings.Local.AutoLogin ? _passwordText.Value : string.Empty;
				UserSettings.Local.Save();
				OnLogin?.Invoke(this);
			}
			else
			{
				EnableForm();
				_errorLabel.Text = session;
				_errorLabel.Visible = true;
				_registerButton.ButtonParagraph = UserInterface.DefaultParagraph("Offline Mode", Anchor.Center);
			}
		}
	}
}

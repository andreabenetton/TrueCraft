using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using GeonBit.UI.Entities;
using GeonBit.UI.Entities.TextValidators;
using Microsoft.Xna.Framework;
using TrueCraft.Core;
using TrueCraft.Launcher.Entities;

namespace TrueCraft.Launcher.Views
{
    public sealed class LoginView : ILauncherView
    {
        private static readonly HttpClient HttpClient = new();

        private readonly LauncherGame _game;
        private Label _errorLabel;
        private TextInput _usernameInput;
        private TextInput _passwordInput;
        private CheckBox _rememberCheckbox;
        private MenuButton _loginButton;
        private MenuButton _registerButton;
        private MenuButton _offlineButton;

        public LoginView(LauncherGame game)
        {
            _game = game;
        }

        public void Mount(Panel parent)
        {
            parent.AddChild(new Header("Sign in"));
            parent.AddChild(new HorizontalLine());

            _errorLabel = new Label("", Anchor.Auto)
            {
                FillColor = Color.IndianRed,
                Visible = false,
            };
            parent.AddChild(_errorLabel);

            parent.AddChild(new Label("Username"));
            _usernameInput = new TextInput(false)
            {
                Value = UserSettings.Local?.Username ?? string.Empty,
            };
            // Match Mojang conventions: ASCII letters/digits, no double spaces.
            _usernameInput.Validators.Add(new EnglishCharactersOnly(true));
            _usernameInput.Validators.Add(new OnlySingleSpaces());
            parent.AddChild(_usernameInput);

            parent.AddChild(new Label("Password"));
            _passwordInput = new TextInput(false)
            {
                HideInputWithChar = '*',
                Value = UserSettings.Local?.AutoLogin == true ? UserSettings.Local.Password ?? string.Empty : string.Empty,
            };
            parent.AddChild(_passwordInput);

            _rememberCheckbox = new CheckBox("Remember me", Anchor.Auto)
            {
                Checked = UserSettings.Local?.AutoLogin == true,
            };
            parent.AddChild(_rememberCheckbox);

            parent.AddChild(new LineSpace());

            _loginButton = new MenuButton("Log in", anchor: Anchor.Auto);
            _loginButton.OnClick = _ => BeginLogin();
            parent.AddChild(_loginButton);

            _offlineButton = new MenuButton("Play offline", Anchor.Auto);
            _offlineButton.OnClick = _ => LoginOffline();
            parent.AddChild(_offlineButton);

            _registerButton = new MenuButton("Register account", Anchor.Auto);
            _registerButton.OnClick = _ => OpenBrowser("https://truecraft.io/register");
            parent.AddChild(_registerButton);
        }

        private void BeginLogin()
        {
            var username = _usernameInput.Value;
            var password = _passwordInput.Value;
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Enter both a username and a password.");
                return;
            }

            SetInteractive(false);
            ShowError(null);

            Task.Run(async () =>
            {
                try
                {
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new System.Collections.Generic.KeyValuePair<string, string>("user", username),
                        new System.Collections.Generic.KeyValuePair<string, string>("password", password),
                        new System.Collections.Generic.KeyValuePair<string, string>("version", "12"),
                    });
                    var response = await HttpClient.PostAsync(
                        TrueCraftUser.AuthServer + "/api/login", content);
                    var body = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(body))
                    {
                        _game.Invoke(() =>
                        {
                            ShowError("Username or password incorrect.");
                            SetInteractive(true);
                        });
                        return;
                    }

                    // Response format historically: part0:part1:UUID:SessionId
                    var parts = body.Split(':');
                    var sessionId = parts.Length >= 4 ? parts[3] : body.Trim();

                    _game.Invoke(() =>
                    {
                        _game.User.Username = username;
                        _game.User.SessionId = sessionId;
                        PersistSettings(username, password, _rememberCheckbox.Checked);
                        _game.ShowView(new MainMenuView(_game));
                    });
                }
                catch (Exception ex)
                {
                    _game.Invoke(() =>
                    {
                        ShowError($"Login failed: {ex.Message}");
                        SetInteractive(true);
                    });
                }
            });
        }

        private void LoginOffline()
        {
            var username = string.IsNullOrWhiteSpace(_usernameInput.Value) ? "Player" : _usernameInput.Value;
            _game.User.Username = username;
            _game.User.SessionId = "-";
            PersistSettings(username, password: null, remember: false);
            _game.ShowView(new MainMenuView(_game));
        }

        private static void PersistSettings(string username, string password, bool remember)
        {
            if (UserSettings.Local is null)
                return;
            UserSettings.Local.Username = username;
            UserSettings.Local.AutoLogin = remember && password is not null;
            UserSettings.Local.Password = remember && password is not null ? password : string.Empty;
            UserSettings.Local.Save();
        }

        private void SetInteractive(bool enabled)
        {
            if (_loginButton is not null) _loginButton.Enabled = enabled;
            if (_offlineButton is not null) _offlineButton.Enabled = enabled;
            if (_registerButton is not null) _registerButton.Enabled = enabled;
            if (_usernameInput is not null) _usernameInput.Enabled = enabled;
            if (_passwordInput is not null) _passwordInput.Enabled = enabled;
        }

        private void ShowError(string message)
        {
            if (_errorLabel is null)
                return;
            if (string.IsNullOrEmpty(message))
            {
                _errorLabel.Visible = false;
                _errorLabel.Text = string.Empty;
                return;
            }
            _errorLabel.Text = message;
            _errorLabel.Visible = true;
        }

        private static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch
            {
                // ignore — non-fatal
            }
        }

        public void Dispose() { }
    }
}

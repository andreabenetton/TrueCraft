using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Iguina.Defs;
using Iguina.Entities;
using TrueCraft.Core;

namespace TrueCraft.Launcher.Views;

public sealed class LoginView : ILauncherView
{
    private static readonly HttpClient HttpClient = new();

    private readonly LauncherGame _game;
    private Paragraph _errorLabel;
    private TextInput _usernameInput;
    private TextInput _passwordInput;
    private Checkbox _rememberCheckbox;
    private Button _loginButton;
    private Button _registerButton;
    private Button _offlineButton;

    public LoginView(LauncherGame game)
    {
        _game = game;
    }

    public void Mount(Panel parent)
    {
        parent.AddChild(new Title(_game.UI, "Sign in"));
        parent.AddChild(new HorizontalLine(_game.UI));

        _errorLabel = new Paragraph(_game.UI, string.Empty) { Visible = false };
        // Iguina has no "FillColor" on Paragraph; use OverrideStyles.TextFillColor.
        _errorLabel.OverrideStyles.TextFillColor = new Color(205, 92, 92, 255);
        parent.AddChild(_errorLabel);

        parent.AddChild(new Paragraph(_game.UI, "Username"));
        _usernameInput = new TextInput(_game.UI)
        {
            Value = UserSettings.Local?.Username ?? string.Empty,
        };
        parent.AddChild(_usernameInput);

        parent.AddChild(new Paragraph(_game.UI, "Password"));
        _passwordInput = new TextInput(_game.UI)
        {
            MaskingCharacter = '*',
            Value = UserSettings.Local?.AutoLogin == true
                ? UserSettings.Local.Password ?? string.Empty
                : string.Empty,
        };
        parent.AddChild(_passwordInput);

        _rememberCheckbox = new Checkbox(_game.UI, "Remember me")
        {
            Checked = UserSettings.Local?.AutoLogin == true,
        };
        parent.AddChild(_rememberCheckbox);

        parent.AddChild(new RowsSpacer(_game.UI));

        _loginButton = new Button(_game.UI, "Log in") { Anchor = Anchor.AutoCenter };
        _loginButton.Events.OnClick = _ => BeginLogin();
        parent.AddChild(_loginButton);

        _offlineButton = new Button(_game.UI, "Play offline") { Anchor = Anchor.AutoCenter };
        _offlineButton.Events.OnClick = _ => LoginOffline();
        parent.AddChild(_offlineButton);

        _registerButton = new Button(_game.UI, "Register account") { Anchor = Anchor.AutoCenter };
        _registerButton.Events.OnClick = _ => OpenBrowser("https://truecraft.io/register");
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
                    new KeyValuePair<string, string>("user", username),
                    new KeyValuePair<string, string>("password", password),
                    new KeyValuePair<string, string>("version", "12"),
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

                // Historical Mojang response: part0:part1:UUID:SessionId
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

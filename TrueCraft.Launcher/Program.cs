using System;
using System.Net.Http;
using System.Threading;
using TrueCraft.Core;
using Xwt;

namespace TrueCraft.Launcher
{
    internal class Program
    {
        private static readonly HttpClient SessionClient = new HttpClient();
        private static readonly CancellationTokenSource SessionCancel = new CancellationTokenSource();

        public static LauncherWindow Window { get; set; }

        [STAThread]
        public static void Main(string[] args)
        {
            if (RuntimeInfo.IsLinux)
                Application.Initialize(ToolkitType.Gtk);
            else if (RuntimeInfo.IsMacOSX)
                Application.Initialize(ToolkitType.Gtk); // TODO: Cocoa
            else if (RuntimeInfo.IsWindows)
                Application.Initialize(ToolkitType.Wpf);
            else
                // In this case they're probably using some flavor of Unix
                // which probably has some flavor of GTK availble
                Application.Initialize(ToolkitType.Gtk);
            UserSettings.Local = new UserSettings();
            UserSettings.Local.Load();
            var thread = new Thread(KeepSessionAlive) {IsBackground = true, Priority = ThreadPriority.Lowest};
            Window = new LauncherWindow();
            thread.Start();
            Window.Show();
            Window.Closed += (sender, e) => Application.Exit();
            Application.Run();
            Window.Dispose();
            SessionCancel.Cancel();
            thread.Join(TimeSpan.FromSeconds(5));
            SessionClient.Dispose();
        }

        private static void KeepSessionAlive()
        {
            var token = SessionCancel.Token;
            while (!token.IsCancellationRequested)
            {
                if (!string.IsNullOrEmpty(Window.User.SessionId))
                {
                    try
                    {
                        var url = string.Format(TrueCraftUser.AuthServer + "/session?name={0}&session={1}",
                            Window.User.Username, Window.User.SessionId);
                        SessionClient.GetStringAsync(url).GetAwaiter().GetResult();
                    }
                    catch
                    {
                        // Network errors are not fatal — try again next interval.
                    }
                }

                token.WaitHandle.WaitOne(TimeSpan.FromMinutes(5));
            }
        }
    }
}
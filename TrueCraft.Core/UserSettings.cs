using System;
using System.IO;
using Newtonsoft.Json;

namespace TrueCraft.Core
{
    public class UserSettings
    {
        public UserSettings()
        {
            AutoLogin = false;
            Username = "";
            Password = "";
            LastIP = "";
            SelectedTexturePack = TexturePack.Default.Name;
            FavoriteServers = new FavoriteServer[0];
            IsFullscreen = false;
            InvertedMouse = false;
            WindowResolution = new WindowResolution
            {
                Width = 1280,
                Height = 720
            };
        }

        public static UserSettings Local { get; set; }

        public bool AutoLogin { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string LastIP { get; set; }
        public string SelectedTexturePack { get; set; }
        public FavoriteServer[] FavoriteServers { get; set; }
        public bool IsFullscreen { get; set; }
        public bool InvertedMouse { get; set; }
        public WindowResolution WindowResolution { get; set; }

        public void Load()
        {
            if (File.Exists(Paths.Settings))
                JsonConvert.PopulateObject(File.ReadAllText(Paths.Settings), this);
        }

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Paths.Settings) ?? throw new InvalidOperationException());
            File.WriteAllText(Paths.Settings, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }

    public class FavoriteServer
    {
        public string Name { get; set; }
        public string Address { get; set; }
    }

    public class WindowResolution
    {
        public static readonly WindowResolution[] Defaults =
        {
            // (from Wikipedia/other)
            FromString("800 x 600"), // SVGA
            FromString("960 x 640"), // DVGA
            FromString("1024 x 600"), // WSVGA
            FromString("1024 x 768"), // XGA
            FromString("1280 x 1024"), // SXGA
            FromString("1600 x 1200"), // UXGA
            FromString("1920 x 1080"), // big
            FromString("1920 x 1200"), // really big
            FromString("4096 x 2160") // huge
        };

        public int Width { get; set; }
        public int Height { get; set; }

        public static WindowResolution FromString(string str)
        {
            var tmp = str.Split('x');
            return new WindowResolution
            {
                Width = int.Parse(tmp[0].Trim()),
                Height = int.Parse(tmp[1].Trim())
            };
        }

        public override string ToString()
        {
            return $"{Width} x {Height}";
        }
    }
}
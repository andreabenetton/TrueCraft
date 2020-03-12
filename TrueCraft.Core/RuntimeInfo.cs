using System;
using System.Diagnostics;
using System.IO;

namespace TrueCraft.Core
{
    public static class RuntimeInfo
    {
        static RuntimeInfo()
        {
            IsMono = Type.GetType("Mono.Runtime") != null;
            var p = (int) Environment.OSVersion.Platform;
            IsUnix = p == 4 || p == 6 || p == 128;
            IsWindows = Path.DirectorySeparatorChar == '\\';

            Is32Bit = IntPtr.Size == 4;
            Is64Bit = IntPtr.Size == 8;

            if (IsUnix)
            {
                var uname = new Process();
                uname.StartInfo.FileName = "uname";
                uname.StartInfo.UseShellExecute = false;
                uname.StartInfo.RedirectStandardOutput = true;
                uname.Start();
                var output = uname.StandardOutput.ReadToEnd();
                uname.WaitForExit();

                output = output.ToUpper().Replace("\n", "").Trim();

                IsMacOSX = output == "DARWIN";
                IsLinux = output == "LINUX";
            }
            else
            {
                IsMacOSX = false;
                IsLinux = false;
            }
        }

        public static bool Is32Bit { get; }
        public static bool Is64Bit { get; }
        public static bool IsMono { get; }
        public static bool IsWindows { get; }
        public static bool IsUnix { get; }
        public static bool IsLinux { get; }
        public static bool IsMacOSX { get; }
    }
}
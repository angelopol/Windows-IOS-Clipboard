using System;
using Microsoft.Win32;

namespace ClipboardAgent
{
    // Arranque con Windows mediante la clave Run del registro (sesión de usuario).
    // Simple y fiable: sin el quoting frágil de schtasks, sin permisos de admin.
    public static class Autostart
    {
        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "ClipboardAgent";

        private static string ExePath()
        {
            // Environment.ProcessPath devuelve el .exe también en publish single-file.
            return Environment.ProcessPath;
        }

        public static bool IsEnabled()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunKey, false);
                return key?.GetValue(ValueName) is string s && !string.IsNullOrEmpty(s);
            }
            catch { return false; }
        }

        public static bool Enable()
        {
            try
            {
                var exe = ExePath();
                if (string.IsNullOrEmpty(exe)) return false;
                using var key = Registry.CurrentUser.OpenSubKey(RunKey, true)
                                ?? Registry.CurrentUser.CreateSubKey(RunKey, true);
                key.SetValue(ValueName, "\"" + exe + "\"");
                return true;
            }
            catch { return false; }
        }

        public static bool Disable()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunKey, true);
                if (key?.GetValue(ValueName) != null) key.DeleteValue(ValueName, false);
                return true;
            }
            catch { return false; }
        }
    }
}

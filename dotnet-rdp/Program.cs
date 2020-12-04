using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32;

namespace rdp
{
    class Program
    {
        static void Main(string[] args)
        {
            CheckRuntime();

            // Default key
            using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Terminal Server Client\Default", true);

            if (key == null)
            {
                Error("The specified registry could not be found.");
                return;
            }

            var keyNames = key.GetValueNames();

            if (!keyNames.Any())
            {
                Success("No record found, no need to clear.");
                return;
            }

            foreach (var name in keyNames)
            {
                key.DeleteValue(name);
            }

            // Server key
            using var serverKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Terminal Server Client\Servers", true);

            if (serverKey != null)
            {
                var subKeyNames = serverKey.GetSubKeyNames();
                if (subKeyNames.Any())
                {
                    foreach (var name in subKeyNames.Where(a => a != "localhost"))
                    {
                        serverKey.DeleteSubKey(name);
                    }
                }
            }

            Success("Cleaned up successfully.");
        }

        static void CheckRuntime()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Error("This application can only run on windows.");
                Environment.Exit(-1);
            }

            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            var isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);

            if (!isElevated)
            {
                Error("Administrator permission is required to running.");
                Environment.Exit(-1);
            }
        }

        static void Error(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        static void Success(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(msg);
            Console.ResetColor();
        }
    }
}

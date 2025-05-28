using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Components;

namespace OpenAutoBench_ng.OpenAutoBench
{
    public class WebUILauncher
    {
        [Inject]
        private static NavigationManager Navigation { get; set; } = default!;

        public WebUILauncher()
        {
            //
        }

        public static void LaunchWebUI()
        {
            //TODO: fix this
            //string appUrl = Navigation.ToAbsoluteUri("/").ToString();
            string appUrl = "http://localhost:5000/";
            Console.Write(appUrl);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo(appUrl) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", appUrl);
            }
            else
            {
                Console.Write("Get rekt lol");
            }
        }

        public static async Task WaitAndLaunchWebUI()
        {
            // give the server time to spool up
            await Task.Delay(1000);
            LaunchWebUI();
        }
    }
}

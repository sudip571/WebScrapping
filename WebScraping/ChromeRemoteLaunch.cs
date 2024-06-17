using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using System.Diagnostics;

namespace WebScraping
{
    public class ChromeRemoteLaunch
    {
        public static async Task RemoteLaunchChrome()
        {
            // Start Chrome with remote debugging
            var chromePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe"; // Ensure this is the correct path
            var userDataDir = @"C:\chrome_dev"; // Path to the user data directory

            var chromeProcess = StartChrome(chromePath, userDataDir);

            // Allow some time for Chrome to start and open the remote debugging port
            await Task.Delay(5000);

            // Connect to the Chrome instance using PuppeteerSharp
            var browserWSEndpoint = await GetWebSocketDebuggerUrlAsync();
            var browser = await Puppeteer.ConnectAsync(new ConnectOptions
            {
                BrowserWSEndpoint = browserWSEndpoint
            });

            var page = await browser.NewPageAsync();
            await page.GoToAsync("https://www.danmurphys.com.au/red-wine/all");

            // Your code here...

            // Close the browser and kill the process
            await browser.CloseAsync();
            chromeProcess.Kill();
        }

        private static Process StartChrome(string chromePath, string userDataDir)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = chromePath,
                Arguments = $"--remote-debugging-port=9222 --user-data-dir=\"{userDataDir}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            return Process.Start(startInfo);
        }

        private static async Task<string> GetWebSocketDebuggerUrlAsync()
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync("http://localhost:9222/json");
            var endpoint = JArray.Parse(response)[0]["webSocketDebuggerUrl"].ToString();
            return endpoint;
        }
    }
}



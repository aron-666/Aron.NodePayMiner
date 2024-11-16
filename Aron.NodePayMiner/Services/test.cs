using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using SeleniumExtras.WaitHelpers;
using System.Diagnostics;

namespace Aron.NodePayMiner.Services
{
    public class test
    {

        static void Main(string[] args)
        {
            Run();
        }

        static async Task Run()
        {
            // 設定記錄器
            SetupLogging();

            string branch = "";
            string version = "1.0.9" + branch;
            int secUntilRestart = 60;
            Console.WriteLine($"Started the script {version}");

            ChromeDriver driver = null;

            try
            {
                // 取得作業系統資訊
                var osInfo = GetOSInfo();
                Console.WriteLine($"OS Info: {osInfo}");

                // 從環境變數讀取變數
                string cookie = Environment.GetEnvironmentVariable("NP_COOKIE");
                string extensionId = Environment.GetEnvironmentVariable("EXTENSION_ID");
                string extensionUrl = Environment.GetEnvironmentVariable("EXTENSION_URL");

                // 檢查是否提供憑證
                if (string.IsNullOrEmpty(cookie))
                {
                    Console.WriteLine("No cookie provided. Please set the NP_COOKIE environment variable.");
                    return; // 如果未提供憑證，則退出腳本
                }

                // Chrome 選項
                var chromeOptions = new ChromeOptions();
                chromeOptions.AddExtension($"./{extensionId}.crx");
                chromeOptions.AddArgument("--no-sandbox");
                chromeOptions.AddArgument("--headless=new");
                chromeOptions.AddArgument("--disable-dev-shm-usage");
                chromeOptions.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36 Edg/121.0.0.0");

                // 初始化 WebDriver
                string chromedriverVersion = GetChromedriverVersion();
                Console.WriteLine($"Using {chromedriverVersion}");
                using (driver = new ChromeDriver(chromeOptions))
                {
                    // NodePass 檢查寬度是否小於 1024p
                    driver.Manage().Window.Size = new System.Drawing.Size(1024, driver.Manage().Window.Size.Height);

                    // 導航到網頁
                    Console.WriteLine($"Navigating to {extensionUrl} website...");
                    driver.Navigate().GoToUrl(extensionUrl);
                    await Task.Delay(TimeSpan.FromSeconds(new Random().Next(3, 7)));

                    AddCookieToLocalStorage(driver, cookie);

                    // 檢查是否成功登錄
                    while (!WaitForElementExists(driver, By.XPath("//*[text()='Dashboard']")))
                    {
                        Console.WriteLine($"Refreshing in {secUntilRestart} seconds to check login (If stuck, verify your token)...");
                        driver.Navigate().Refresh();
                        await Task.Delay(TimeSpan.FromSeconds(secUntilRestart));
                    }

                    Console.WriteLine("Logged in successfully!");

                    await Task.Delay(TimeSpan.FromSeconds(new Random().Next(10, 50)));
                    Console.WriteLine("Accessing extension settings page...");
                    driver.Navigate().GoToUrl($"chrome-extension://{extensionId}/index.html");
                    await Task.Delay(TimeSpan.FromSeconds(new Random().Next(3, 7)));

                    // 刷新直到“登錄”按鈕消失
                    while (WaitForElementExists(driver, By.XPath("//*[text()='Login']")))
                    {
                        Console.WriteLine("Clicking the extension login button...");
                        var login = driver.FindElement(By.XPath("//*[text()='Login']"));
                        login.Click();
                        await Task.Delay(TimeSpan.FromSeconds(10));
                        // 刷新頁面
                        driver.Navigate().Refresh();
                    }

                    // 檢查“已激活”元素
                    CheckActiveElement(driver);

                    // 取得所有窗口的句柄
                    var allWindows = driver.WindowHandles;

                    // 取得活動窗口的句柄
                    var activeWindow = driver.CurrentWindowHandle;

                    // 關閉除活動窗口之外的所有窗口
                    foreach (var window in allWindows)
                    {
                        if (window != activeWindow)
                        {
                            driver.SwitchTo().Window(window);
                            driver.Close();
                        }
                    }

                    // 切換回活動窗口
                    driver.SwitchTo().Window(activeWindow);

                    ConnectionStatus(driver);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred: {e}");
                Console.WriteLine($"Restarting in {secUntilRestart} seconds...");
                await Task.Delay(TimeSpan.FromSeconds(secUntilRestart));
                await Run();
            }

            while (true)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromHours(1));
                    //driver.Navigate().Refresh();
                    ConnectionStatus(driver);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"An error occurred in loop: {e}");
                    Console.WriteLine($"Restarting in {secUntilRestart} seconds...");
                    await Task.Delay(TimeSpan.FromSeconds(secUntilRestart));
                    await Run();
                }
            }
        }

        static void SetupLogging()
        {
            // 設定記錄器（可以使用其他記錄器，例如 NLog、log4net 等）
            // 這裡使用 Console 作為輸出
        }

        static void ConnectionStatus(ChromeDriver driver)
        {
            if (WaitForElementExists(driver, By.XPath("//*[text()='Connected']")))
            {
                Console.WriteLine("Status: Connected!");
            }
            else if (WaitForElementExists(driver, By.XPath("//*[text()='Disconnected']")))
            {
                Console.WriteLine("Status: Disconnected!");
            }
            else
            {
                Console.WriteLine("Status: Unknown!");
            }
        }

        static void CheckActiveElement(ChromeDriver driver)
        {
            try
            {
                WaitForElement(driver, By.XPath("//*[text()='Activated']"));
                driver.FindElement(By.XPath("//*[text()='Activated']"));
                Console.WriteLine("Extension is activated!");
            }
            catch (NoSuchElementException)
            {
                Console.WriteLine("Failed to find 'Activated' element. Extension activation failed.");
            }
        }

        static bool WaitForElementExists(ChromeDriver driver, By by, int timeout = 10)
        {
            try
            {
                new WebDriverWait(driver, TimeSpan.FromSeconds(timeout)).Until(ExpectedConditions.ElementExists(by));
                return true;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        static IWebElement WaitForElement(IWebDriver driver, By by, int timeout = 10)
        {
            try
            {
                var element = new WebDriverWait(driver, TimeSpan.FromSeconds(timeout)).Until(ExpectedConditions.ElementExists(by));
                return element;
            }
            catch (WebDriverTimeoutException e)
            {
                Console.WriteLine($"Error waiting for element {by}: {e}");
                throw;
            }
        }

        static string SetLocalStorageItem(ChromeDriver driver, string key, string value)
        {
            driver.ExecuteScript($"localStorage.setItem('{key}', '{value}');");
            var result = driver.ExecuteScript($"return localStorage.getItem('{key}');") as string;
            return result;
        }

        static void AddCookieToLocalStorage(ChromeDriver driver, string cookieValue)
        {
            string[] keys = { "np_webapp_token", "np_token" };
            foreach (string key in keys)
            {
                string result = SetLocalStorageItem(driver, key, cookieValue);
                Console.WriteLine($"Added {key} with value {result.Substring(0, 8)}...{result.Substring(result.Length - 8)} to local storage.");
            }
            Console.WriteLine("!!!!! Your token can be used to login for 7 days !!!!!");
        }

        static string GetChromedriverVersion()
        {
            try
            {
                using (var process = new Process())
                {
                    process.StartInfo.FileName = "chromedriver";
                    process.StartInfo.Arguments = "--version";
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.UseShellExecute = false;

                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    return
 output.Trim();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not get ChromeDriver version: {e}");
                return "Unknown version";
            }
        }

        static string GetOSInfo()
        {
            try
            {
                // 在 C# 中，可以使用 Environment.OSVersion 取得作業系統資訊
                // 或者使用 RuntimeInformation.OSDescription 取得更詳細的資訊
                return Environment.OSVersion.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not get OS information: {e}");
                return "Unknown OS";
            }
        }
    }
}

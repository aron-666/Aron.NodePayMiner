using Microsoft.AspNetCore.Mvc;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using SeleniumExtras.WaitHelpers;
using System.Net;
using System.Drawing;
using Newtonsoft.Json;
using System.Text;
using Aron.NodePayMiner.Models;

namespace Aron.NodePayMiner.Services
{
    public class MinerService : IMinerService
    {
        public ChromeDriver driver { get; set; }
        private readonly AppConfig _appConfig;
        private readonly MinerRecord _minerRecord;
        private readonly string extensionPath = "./Nodepay.crx";
        private readonly string extensionId = "lgmpfmgeabnnlemejacfljbmonaomfmm";
        private bool Enabled { get; set; } = true;

        private Thread? thread;

        private DateTime BeforeRefresh = DateTime.MinValue;
        public MinerService(AppConfig appConfig, MinerRecord minerRecord)
        {
            _appConfig = appConfig;
            _minerRecord = minerRecord;
            // call https://ifconfig.me to get the public IP address
            try
            {
                _minerRecord.PublicIp = new WebClient().DownloadString("https://ifconfig.me");
            }
            catch (Exception ex)
            {
                _minerRecord.PublicIp = "Error to get your public ip.";
            }

            thread = new Thread(async () =>
            {
                while (true)
                {
                    try
                    {
                        if (Enabled)
                        {
                            await Run();
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _minerRecord.Exception = ex.ToString();
                        _minerRecord.ExceptionTime = DateTime.Now;
                        _minerRecord.Status = MinerStatus.Error;
                    }
                    finally
                    {
                        await Task.Delay(10000);
                    }
                }

            })
            { IsBackground = true };

            thread.Start();
        }

        public void Stop()
        {
            Enabled = false;
        }

        public void Start()
        {

            Enabled = true;

        }

        private async Task Run()
        {
            try
            {
                driver?.Close();
                driver?.Quit();
                driver?.Dispose();
                driver = null;
                _minerRecord.Status = MinerStatus.AppStart;
                _minerRecord.IsConnected = false;
                _minerRecord.LoginUserName = null;
                _minerRecord.ReconnectSeconds = 0;
                _minerRecord.ReconnectCounts = 0;
                _minerRecord.Exception = null;
                _minerRecord.ExceptionTime = null;
                _minerRecord.Points = "0";

                // get assembly version
                _minerRecord.AppVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

                string npToken = _appConfig.NpToken;

                // 設定 Chrome 擴充功能路徑
                string chromedriverPath = "./chromedriver";

                // 建立 Chrome 選項
                ChromeOptions options = new ChromeOptions();
                options.AddArgument("--chromedriver=" + chromedriverPath);
                if (!_appConfig.ShowChrome)
                    options.AddArgument("--headless=new");
                options.AddArgument("--no-sandbox");
                //options.AddArgument("--enable-javascript");
                options.AddArgument("--auto-close-quit-quit");
                options.AddArgument("disable-infobars");
                options.AddArgument("--window-size=1024,768");
                if ((_appConfig.ProxyEnable ?? "").ToLower() == "true"
                    && !string.IsNullOrEmpty(_appConfig.ProxyHost))
                {
                    options.AddArgument("--proxy-server=" + _appConfig.ProxyHost);
                    if (!string.IsNullOrEmpty(_appConfig.ProxyUser) && !string.IsNullOrEmpty(_appConfig.ProxyPass))
                    {
                        options.AddArgument($"--proxy-auth={_appConfig.ProxyUser}:{_appConfig.ProxyPass}");
                    }
                }
                options.AddExcludedArgument("enable-automation");
                options.AddUserProfilePreference("credentials_enable_service", false);
                options.AddUserProfilePreference("profile.password_manager_enabled", false);
                options.AddArgument("--disable-gpu"); // 禁用 GPU 加速，减少资源占用
                options.AddArgument("--disable-software-rasterizer"); // 禁用软件光栅化器
                options.AddArgument("--disable-dev-shm-usage"); // 禁用 /dev/shm 临时文件系统
                //options.AddArgument("--force-dark-mode");
                options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36 Edg/121.0.0.0");

                options.AddExtension(extensionPath);


                // 建立 Chrome 瀏覽器
                driver = new ChromeDriver(options);
                try
                {
                    
                    driver.Navigate().GoToUrl("https://app.nodepay.ai/");
                    WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(50));

                    Console.WriteLine("Go to app: " + driver.Url);

                    _ = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//span[text()='Access My Account']")));
                    AddCookieToLocalStorage(driver, npToken);



                    // 關閉其他頁面
                    var originalWindow = driver.CurrentWindowHandle;
                    foreach (var handle in driver.WindowHandles)
                    {
                        if (handle != originalWindow)
                        {
                            driver.SwitchTo().Window(handle);
                            driver.Close();
                        }
                    }
                    driver.SwitchTo().Window(originalWindow);

                    _minerRecord.Status = MinerStatus.LoginPage;
                    await Task.Delay(new Random().Next(2100, 5455));

                    driver.Navigate().GoToUrl("https://app.nodepay.ai/dashboard");
                    Console.WriteLine("Go to dashboard: " + driver.Url);
                    await Task.Delay(new Random().Next(2100, 5455));
                    AddCookieToLocalStorage(driver, npToken);

                    // 檢查是否成功登錄

                    int maxAttempts = 6;
                    int attemptCount = 0;
                    int secUntilRestart = 10;

                    while (attemptCount < maxAttempts && !WaitForElementExists(driver, By.XPath("//*[text()='Dashboard']"), 45))
                    {
                        Console.WriteLine($"Refreshing in {secUntilRestart} seconds to check login (If stuck, verify your token)...");

                        await Task.Delay(TimeSpan.FromSeconds(secUntilRestart) + TimeSpan.FromMilliseconds(new Random().Next(2200, 4050)));
                        driver.Navigate().GoToUrl("https://app.nodepay.ai/dashboard");
                        Console.WriteLine("Go to dashboard: " + driver.Url);



                        attemptCount++;
                    }

                    if (attemptCount >= maxAttempts)
                    {
                        throw new Exception("Failed to login after multiple attempts. Please verify your token.");
                    }

                    //_minerRecord.LoginUserName = userName;
                }
                catch (Exception ex)
                {
                    _minerRecord.Status = MinerStatus.LoginError;
                    _minerRecord.Exception = ex.ToString();
                    _minerRecord.ExceptionTime = DateTime.Now;
                    Console.WriteLine(ex);
                    return;
                }


                driver.Navigate().GoToUrl($"chrome-extension://{extensionId}/index.html");
                Console.WriteLine("Go to extension: " + driver.Url);

                driver.Manage().Window.Size = new Size(1024, 768);

                _minerRecord.Status = MinerStatus.Disconnected;
                while (Enabled)
                {
                    try
                    {
                        if (!driver.PageSource.Contains("Connected"))
                        {
                            
                            _minerRecord.Status = MinerStatus.Disconnected;
                            _minerRecord.IsConnected = false;
                            _minerRecord.ReconnectCounts++;
                        }
                        else
                        {
                            _minerRecord.Status = MinerStatus.Connected;
                            //$('img[alt="token"]')

                            IWebElement? imageElement = driver.FindElement(By.CssSelector("img[alt='ic-nodepay']"));
                            IWebElement? parentElement = imageElement?.FindElement(By.XPath(".."));
                            IWebElement? valueElement = parentElement?.FindElement(By.CssSelector("span.text-16px.font-bold"));

                            _minerRecord.Points = valueElement?.Text ?? "";
                            IWebElement element = driver.FindElement(By.XPath("//span[starts-with(., 'Network Quality:')]"));

                            _minerRecord.NetworkQuality = element.Text.Replace("Network Quality: ", "");
                            //IWebElement? userNameElement = driver.FindElement(By.CssSelector("span[title='Username']"));
                            _minerRecord.IsConnected = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        _minerRecord.Status = MinerStatus.Connected;
                    }
                    finally
                    {
                        int countdownSeconds = 30;

                        // 倒數計時
                        while (countdownSeconds > 0)
                        {
                            _minerRecord.ReconnectSeconds = countdownSeconds;

                            SpinWait.SpinUntil(() => false, 1000); // 等待 1 秒
                            if (driver.PageSource.Contains("Connected"))
                                break;
                            countdownSeconds--;
                            if (!Enabled)
                            {
                                break;
                            }
                        }
                        if (Enabled && BeforeRefresh.AddSeconds(60) <= DateTime.Now)
                        {
                            BeforeRefresh = DateTime.Now;
                            //refresh
                            driver.Navigate().GoToUrl($"chrome-extension://{extensionId}/index.html");
                            SpinWait.SpinUntil(() => !Enabled, 15000);
                        }
                        await Task.Delay(5000);
                    }
                }
                _minerRecord.Status = MinerStatus.Stop;
            }
            catch (Exception ex)
            {
                _minerRecord.Exception = ex.ToString();
                _minerRecord.ExceptionTime = DateTime.Now;
                _minerRecord.Status = MinerStatus.Error;
                Console.WriteLine(ex);
            }
            finally
            {
                driver?.Close();
                driver?.Quit();
                driver?.Dispose();
                driver = null;
            }
        }

        static void SetLocalStorageItem(IWebDriver driver, string key, string value)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript($"window.localStorage.setItem('{key}', '{value}');");
        }

        static void SetCookie(IWebDriver driver, string key, string value)
        {
            driver.Manage().Cookies.AddCookie(new OpenQA.Selenium.Cookie(key, value, "/", DateTime.UtcNow.AddYears(1)));
        }

        static string GetLocalStorageItem(IWebDriver driver, string key)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            return (string)js.ExecuteScript($"return window.localStorage.getItem('{key}');");
        }


        static string SetLocalStorageItem2(ChromeDriver driver, string key, string value)
        {
            driver.ExecuteScript($"localStorage.setItem('{key}', '{value}');");
            var result = driver.ExecuteScript($"return localStorage.getItem('{key}');") as string;
            return result;
        }

        static void AddCookieToLocalStorage(ChromeDriver driver, string npToken)
        {
            string[] keys = { "np_webapp_token", "np_token" };
            foreach (string key in keys)
            {
                SetLocalStorageItem2(driver, key, npToken);
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
                throw;
            }
        }

    }
}

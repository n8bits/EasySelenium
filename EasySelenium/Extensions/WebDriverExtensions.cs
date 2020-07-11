using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;

namespace StacksendAutomation.Extensions
{
    public static class WebDriverExtensions
    {
        /// <summary>
        /// Switches to the window at the index
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="windowIndex"></param>
        /// <returns>returns true if no exception occurs</returns>
        public static void SwitchToWindow(this IWebDriver driver, int windowIndex)
        {
               driver.SwitchTo().Window(driver.WindowHandles[windowIndex]);         
        }

        /// <summary>
        /// Recursively search for the target frame and switch to it if it exists.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="frameName">The name attribute of the frame to find</param>
        /// /// <returns>true if the frame was found</returns>
        public static bool SwitchToFrame(this IWebDriver driver, string frameName)
        {
            driver.SwitchTo().DefaultContent();
            bool frameFound = SearchForFrame(driver, frameName);

            return frameFound;

        }

        /// <summary>
        /// Recursively search for the specified frame and switch to it (if it exists)
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="frameName"></param>
        /// <returns>true if the frame was found</returns>
        private static bool SearchForFrame(IWebDriver driver, string frameName)
        {

            // Temporarily modify the driver's implicit wait avoid delay if no frame elements are found 
            var originalImplicitWait = driver.Manage().Timeouts().ImplicitWait;
            driver.SetImplicitWait(TimeSpan.FromMilliseconds(10));
            var frames = driver.FindElements(By.TagName("frame"));

            foreach(var frame in frames)
            {
                var name = frame.GetAttribute("name");
                if (frame.GetAttribute("name") == frameName)
                {
                    driver.SwitchTo().Frame(frameName);
                    driver.SetImplicitWait(originalImplicitWait);
                    return true;
                }

                driver.SwitchTo().Frame(name);

                if(SearchForFrame(driver, frameName))
                {
                    driver.SetImplicitWait(originalImplicitWait);
                    return true;
                }

                driver.SwitchTo().ParentFrame();
            }

            driver.SetImplicitWait(originalImplicitWait);
            return false;
        }

        /// <summary>
        /// Waits until an alert is visible and accepts it
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="secondsToWait"></param>
        public static void WaitForAlertAndAccept(this IWebDriver driver, int secondsToWait)
        {
            var wait = driver.Waiter();
            wait.Timeout = TimeSpan.FromSeconds(secondsToWait);
            wait.Until(d => d.IsAlertPresent());
            driver.SwitchTo().Alert().Accept();
        }

        /// <summary>
        /// Waits until an alert is present
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="secondsToWait"></param>
        /// <returns>The text of the alert.</returns>
        public static string WaitForAlert(this IWebDriver driver, int secondsToWait)
        {
            var wait = driver.Waiter();
            wait.Timeout = TimeSpan.FromSeconds(secondsToWait);
            wait.Until(d => d.IsAlertPresent());
            return driver.SwitchTo().Alert().Text;
        }

        /// <summary>
        /// Checks to see if there is currently an alert visible on the page by 
        /// attempting to switch to an alert 
        /// </summary>
        /// <param name="driver"></param>
        /// <returns></returns>
        public static bool IsAlertPresent(this IWebDriver driver)
        {
            try
            {
                driver.SwitchTo().Alert();
                return true;
            }
            catch(NoAlertPresentException e)
            {
                return false;
            }
        }

        /// <summary>
        /// Moves the mouse cursor to the specified element
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="element"></param>
        public static void HoverOverElement(this IWebDriver driver, IWebElement element)
        {
            Actions actions = new Actions(driver);
            actions.MoveToElement(element);

            driver.Waiter().Until(d =>
            {
                try
                {
                    actions.Build().Perform();
                }
                catch (Exception e)
                {
                    return false;
                }
                return true;
            });

        }

        /// <summary>
        /// Waits until the document ready state is "complete"
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="timeoutMilliseconds"></param>
        public static void WaitForPageLoaded(this IWebDriver driver)
        {
            driver.Waiter().Until(dri =>
            {
                string state = dri.ExecuteJavascript("return document.readyState").ToString();
                return state == "complete";
            });
        }


        /// <summary>
        /// Execute the specified javascript on the current page
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="script">A string containing javascript to be executed</param>
        /// <returns>The value directly returned by the execution of the script</returns>
        public static object ExecuteJavascript(this IWebDriver driver, string script)
        {
            return ((IJavaScriptExecutor)driver).ExecuteScript(script);
        }

        /// <summary>
        /// A more convenient way to create a WebDriverWait
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="timeoutInSeconds"></param>
        /// <returns></returns>
        public static WebDriverWait Waiter(this IWebDriver driver, int timeoutInSeconds = 50)
        {
            return new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
        }

        /// <summary>
        /// Shortcut method for setting this driver's implicit wait
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="seconds">the desired implicit wait in seconds</param>
        /// <returns></returns>
        public static IWebDriver SetImplicitWait(this IWebDriver driver, int seconds)
        {
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(seconds);
            return driver;
        }

        /// <summary>
        /// Shortcut method for setting this driver's implicit wait
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="timeSpan">the desired implicit wait in seconds</param>
        /// <returns></returns>
        public static IWebDriver SetImplicitWait(this IWebDriver driver, TimeSpan timeSpan)
        {
            driver.Manage().Timeouts().ImplicitWait = timeSpan;
            return driver;
        }

        /// <summary>
        /// Simulates disconnected network state. Only works with chrome driver.
        /// 
        /// Use in conjunction with GoOnline() method
        /// </summary>
        /// <param name="driver"></param>
        public static void GoOffline(this ChromeDriver driver)
        {
            ((ChromeDriver)driver).NetworkConditions = new ChromeNetworkConditions()
            {
                Latency = TimeSpan.FromMilliseconds(10),
                DownloadThroughput = 0,
                UploadThroughput = 0,
                IsOffline = true
            };
        }

        /// <summary>
        /// Returns the driver to the online state after a call to GoOffline has been made.
        /// 
        /// Only works with chrome driver
        /// </summary>
        /// <param name="driver"></param>
        public static void GoOnline(this ChromeDriver driver)
        {
            ((ChromeDriver)driver).NetworkConditions = new ChromeNetworkConditions()
            {
                Latency = TimeSpan.FromMilliseconds(0),
                DownloadThroughput = 1000000,
                UploadThroughput = 1000000,
                IsOffline = true
            };
        }

        /// <summary>
        /// Set the network speed, using the setnetworkspeed in util 
        /// (TODO: Add that body here and remove setnetwork function)
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="seconds"></param>
        /// <param name="disconnect"></param>

        public static void SetSpeed(this IWebDriver driver, int seconds, bool disconnect)
        {
            SetNetWorkSpeed(driver, TimeSpan.FromSeconds(seconds), disconnect);
        }

        public static void Disconnect(this IWebDriver driver)
        {
            driver.SetSpeed(1, true);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            driver.Waiter().Timeout = TimeSpan.FromSeconds(5);
        }

        /// <summary>
        /// Set chrome network slowdown with latency in sencond
        /// </summary>
        /// <param name="driver">Web Driver</param>
        /// <param name="latency">Latency in sencond(ex: TimeSpan.FromMilliseconds(1000))</param>
        /// <param name="onoff">True = network on, False = network off</param>
        public static void SetNetWorkSpeed(IWebDriver driver, TimeSpan latency, bool onoff = false)
        {
            //TimeSpan.FromMilliseconds(1000 * latency)
            ChromeDriver driverNetwork = (ChromeDriver)driver;
            driverNetwork.NetworkConditions = new ChromeNetworkConditions();
        }
    }

}

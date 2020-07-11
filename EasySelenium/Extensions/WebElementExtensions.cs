using OpenQA.Selenium;
using OpenQA.Selenium.Internal;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace StacksendAutomation.Extensions
{
    public static class WebElementExtensions
    {

        public static string GetValue(this IWebElement element)
        {
            return element.GetAttribute("value");
        }

        /// <summary>
        /// Shortcut to find an element usinging a css selector
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cssSelector"></param>
        /// <returns></returns>
        public static IList<IWebElement> FindElements(this ISearchContext context, string cssSelector)
        {
            return context.FindElements(By.CssSelector(cssSelector));
        }

        /// <summary>
        /// Alternative method to .Clear() that presses backspace until the field is cleared
        /// </summary>
        /// <param name="element"></param>
        public static void ClearWithBackspace(this IWebElement element)
        {
            element.SendKeys(Keys.End);
            IWebDriver driver = (element is IWebDriver ? (IWebDriver)element : ((IWrapsDriver)element).WrappedDriver);

            driver.Waiter().Until(d =>
            {
                element.SendKeys(Keys.Backspace);
                return element.Text != string.Empty || element.GetValue() != string.Empty;
            });
        }

        /// <summary>
        /// Accesses this Select's wrapped element and checks if it enabled
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        public static bool Enabled(this SelectElement select)
        {
            return select.WrappedElement.Enabled;
        }

        /// <summary>
        /// Repeatedly attempts to set the specified element's text value to the specified string.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="text"></param>
        /// <param name="maxRetries">The maximum amount of times this method will try to set the text</param>
        /// <returns>True if the text was succesfully set</returns>
        public static bool EnterTextTryHardMode(this IWebElement element, string text, int maxRetries = 10)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                element.Clear();
                element.SendKeys(text);
                if (element.GetValue() == text)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Tries to click an element. Returns false if the click is intercepted.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool AttemptClick(this IWebElement element)
        {
            try
            {
                element.Click();
            }
            catch (ElementClickInterceptedException e)
            {
                return false;
            }
            catch (ElementNotInteractableException e)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Will repeatedly attempt to click the element until either maxTries is reached or the click is successful.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="maxTries">maximum attempts</param>
        /// <param name="attemptInterval">How long to wait between attempts in milliseconds</param>
        /// <returns>true if the click was successful, otherwise false</returns>
        public static bool PatientClick(this IWebElement element, int maxTries = 5, int attemptInterval = 500)
        {
            for(int i=0; i< maxTries; i++)
            {
                try
                {
                    element.AttemptClick();
                    return true;
                }
                catch(WebDriverException e)
                {
                    Thread.Sleep(attemptInterval);
                }
            }

            return false;
        }

        /// <summary>
        /// Shortcut method for FindElement(By.CssSelector()) that returns null instead of thowing a WebElementNotFoundException
        /// if the specified element is not found
        /// </summary>
        /// <param name="context"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IWebElement FindElement(this ISearchContext context, string selector, TimeSpan timeout)
        {
            IWebDriver driver = (context is IWebDriver ? (IWebDriver)context : ((IWrapsDriver)context).WrappedDriver);
            var currentTimeout = driver.Manage().Timeouts().ImplicitWait;

            try
            {
                driver.Manage().Timeouts().ImplicitWait = timeout;
                var element = context.FindElements(By.CssSelector(selector));

                driver.Manage().Timeouts().ImplicitWait = currentTimeout;
                return element.FirstOrDefault();
            }
            catch (WebDriverException e)
            {
                driver.Manage().Timeouts().ImplicitWait = currentTimeout;
                return null;
            }
        }

        /// <summary>
        /// Clears the element using the backspace key, and then types the value passed.
        /// 
        /// If value is null, this method does nothing.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="value"></param>
        public static void SetValue(this IWebElement element, string value)
        {
            if (value == null)
                return;
            element.ClearWithBackspace();
            element.SendKeys(value);
        }
    }
}

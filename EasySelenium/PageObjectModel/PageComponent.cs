using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using StacksendAutomation.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
///
/// </summary>

namespace StacksendAutomation.Pages
{
    /// <summary>
    /// Base class for all web page components and web pages. 
    /// </summary>
    public class PageComponent
    {
        private ISearchContext context;

        private By locator;

        /// <summary>
        /// Creates a page component using a search context 
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="context"></param>
        internal PageComponent(IWebDriver driver, ISearchContext context)
        {
            this.Driver = driver;
            this.context = context;
        }

        /// <summary>
        /// Create a page component using a locator. This allows for the retrieval of the locator used to initialize the component.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="locator"></param>
        internal PageComponent(IWebDriver driver, By locator)
        {
            this.Driver = driver;
            this.context = driver.FindElement(locator);
            this.locator = locator;
        }

        /// <summary>
        /// If this component was constructed using a locator rather than
        /// an ISearch context, this method will return the locator used.
        /// 
        /// If this component was constructed using an ISearchContext, this will return null
        /// </summary>
        public By Locator => this.locator;

        /// <summary>
        /// Get this component's context cast as an IWebElement
        /// </summary>
        public IWebElement ContextAsWebElement
        {
            get
            {
                return ((IWebElement)this.Context);
            }
        }

        public IWebDriver Driver { get; }

        public ISearchContext Context
        {
            get
            {
                return locator != null ? this.Driver.FindElement(locator) : context;
            }
        }

        /// <summary>
        /// Provides easy access to a wait object from inside any web page or component
        /// 
        /// Example: Wait.Until(driver => driver.FindElement(someElementLocator).Displayed)
        /// 
        /// </summary>
        protected WebDriverWait Wait => Driver.Waiter();
    }
}

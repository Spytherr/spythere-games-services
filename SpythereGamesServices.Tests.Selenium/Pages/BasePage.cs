using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace SpythereGamesServices.Tests.Selenium.Pages;

public abstract class BasePage
{
    protected readonly IWebDriver Driver;

    protected readonly WebDriverWait Wait;

    protected readonly string BaseUrl;

    protected BasePage(IWebDriver driver, string baseUrl)
    {
        Driver = driver;
        BaseUrl = baseUrl;

        Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
    }

    public void NavigateTo(string path = "")
    {
        Driver.Navigate().GoToUrl($"{BaseUrl}/{path}".TrimEnd('/'));
    }

    protected void ScrollToElement(IWebElement element)
    {
        ((IJavaScriptExecutor)Driver)
            .ExecuteScript("arguments[0].scrollIntoView({ behavior: 'smooth', block: 'center' });", element);
        
        // short wait for CSS animation to finish
        Thread.Sleep(600);
    }
    protected void ScrollToBottom()
    {
        ((IJavaScriptExecutor)Driver)
            .ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
        Thread.Sleep(600);
    }

    protected IWebElement WaitForElement(By locator)
    {
        return Wait.Until(driver =>
        {
            var element = driver.FindElement(locator);
            return element.Displayed ? element : null;
        })!;
    }
    protected bool IsElementPresent(By locator)
    {
        try
        {
            Driver.FindElement(locator);
            return true;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }
}

using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace SpythereGamesServices.Tests.Selenium.Pages;

public class HeroPage : BasePage
{
    private static readonly By LogoLocator =
        By.CssSelector("img[alt='Spythere Games']");


    private static readonly By GitHubButtonLocator =
        By.XPath("//a[contains(text(),'GitHub')]");

    private static readonly By YouTubeButtonLocator =
        By.XPath("//span[contains(text(),'YouTube')]");


    private static readonly By HeroSectionLocator =
        By.CssSelector("section:first-of-type");


    public HeroPage(IWebDriver driver, string baseUrl) : base(driver, baseUrl){}

    public IWebElement Logo => WaitForElement(LogoLocator);
    public IWebElement GitHubButton => WaitForElement(GitHubButtonLocator);
    public IWebElement YouTubeButton => WaitForElement(YouTubeButtonLocator);
    public IWebElement HeroSection => WaitForElement(HeroSectionLocator);

    public HeroPage Open()
    {
        NavigateTo();
        WaitForElement(LogoLocator);
        return this;
    }

    public string ClickGitHubAndGetNewTabUrl()
    {
        var originalWindow = Driver.CurrentWindowHandle;
        var windowsBefore = Driver.WindowHandles.Count;

        GitHubButton.Click();

        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
        wait.Until(d => d.WindowHandles.Count > windowsBefore);

        var newWindow = Driver.WindowHandles
            .First(handle => handle != originalWindow);

        Driver.SwitchTo().Window(newWindow);
        var newUrl = Driver.Url;

        Driver.Close();
        Driver.SwitchTo().Window(originalWindow);

        return newUrl;
    }

    public bool IsLogoVisible()
    {
        try
        {
            var logo = WaitForElement(LogoLocator);
            var classes = logo.GetDomAttribute("class") ?? "";
            return classes.Contains("opacity-100");
        }
        catch (WebDriverTimeoutException)
        {
            return false;
        }
    }
    public bool IsYouTubeButtonDisabled()
    {
        var button = YouTubeButton;
        var classes = button.GetDomAttribute("class") ?? "";
        return classes.Contains("cursor-not-allowed") &&
               classes.Contains("opacity-40");
    }

    public HeroPage WaitForAnimations()
    {
        Thread.Sleep(1300);
        return this;
    }
}

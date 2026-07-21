using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace SpythereGamesServices.Tests.Selenium.Helpers;

public static class WebDriverFactory
{
    public static IWebDriver CreateChromeDriver()
    {
        var options = new ChromeOptions();

        options.AddArgument("--headless=new");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--window-size=1920,1080");

        return new ChromeDriver(options);
    }
    public static string GetBaseUrl()
    {
        return "https://spythere-games.vercel.app";
    }
}

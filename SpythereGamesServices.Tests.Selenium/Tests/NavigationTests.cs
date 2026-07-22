using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SpythereGamesServices.Tests.Selenium.Helpers;

namespace SpythereGamesServices.Tests.Selenium.Tests;

[TestFixture]
public class NavigationTests
{
    private IWebDriver _driver = null!;
    private string _baseUrl = null!;

    [SetUp]
    public void SetUp()
    {
        _driver = WebDriverFactory.CreateChromeDriver();
        _baseUrl = WebDriverFactory.GetBaseUrl();
        _driver.Navigate().GoToUrl(_baseUrl);
    }

    [TearDown]
    public void TearDown()
    {
        _driver.Quit();
        _driver.Dispose();
    }

    [Test]
    public void Page_LoadsSuccessfully_TitleIsNotEmpty()
    {
        var title = _driver.Title;

        Assert.That(title, Is.Not.Empty,
            "The page title should not be empty, the page should load correctly");
    }

    [Test]
    public void Page_CurrentUrl_StartsWithBaseUrl()
    {
        var currentUrl = _driver.Url;

        Assert.That(currentUrl, Does.StartWith(_baseUrl),
            $"The current URL should start with {_baseUrl}, actual URL: {currentUrl}");
    }

    [Test]
    public void Page_HasSections_AtLeastOnePresent()
    {
        var sections = _driver.FindElements(By.TagName("section"));

        Assert.That(sections, Is.Not.Empty,
            "The page should contain at least one <section> element");
    }

    [Test]
    public void Page_LeaderboardHeading_IsVisibleAfterScroll()
    {
        ((IJavaScriptExecutor)_driver)
            .ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");

        Thread.Sleep(800);

        var headings = _driver.FindElements(By.TagName("h2"));
        var leaderboardHeading = headings.FirstOrDefault(h =>
            h.Text.Contains("Leaderboard", StringComparison.OrdinalIgnoreCase));

        Assert.That(leaderboardHeading, Is.Not.Null,
            "The 'Leaderboards' heading should be visible after scrolling the page");
    }

    [Test]
    public void Page_Footer_IsPresentInDOM()
    {
        ((IJavaScriptExecutor)_driver)
            .ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
        Thread.Sleep(500);

        var footerElements = _driver.FindElements(By.TagName("footer"));

        Assert.That(footerElements, Is.Not.Empty,
            "The <footer> element should be present in the page DOM structure");
    }

    [Test]
    public void Page_GitHubLink_ExistsAndPointsToCorrectProfile()
    {
        var allLinks = _driver.FindElements(By.TagName("a"));

        var githubLink = allLinks.FirstOrDefault(link =>
            link.GetDomAttribute("href")?.Contains("github.com") == true);

        Assert.That(githubLink, Is.Not.Null,
            "The page should contain a GitHub link");

        Assert.That(githubLink!.GetDomAttribute("href"), Does.Contain("Spytherr"),
            "The GitHub link should lead to the 'Spytherr' profile");
    }

    [Test]
    public void Page_GitHubLink_OpensInNewTab()
    {
        var allLinks = _driver.FindElements(By.TagName("a"));

        var githubLink = allLinks.FirstOrDefault(link =>
            link.GetDomAttribute("href")?.Contains("github.com") == true);

        Assert.That(githubLink, Is.Not.Null, "The GitHub link must exist");


        var target = githubLink!.GetDomProperty("target");
        Assert.That(target, Is.EqualTo("_blank"),
            "The GitHub link should open in a new tab (target='_blank')");
    }


    [Test]
    public void Page_OnMobileViewport_HeroSectionIsStillPresent()
    {

        _driver.Manage().Window.Size = new System.Drawing.Size(375, 667);


        _driver.Navigate().Refresh();


        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        wait.Until(d => d.FindElements(By.TagName("section")).Count > 0);

        var sections = _driver.FindElements(By.TagName("section"));

        Assert.That(sections, Is.Not.Empty,
            "On mobile viewport (375px) the page should still display sections");
    }
}

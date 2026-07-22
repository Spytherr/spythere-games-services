using NUnit.Framework;
using OpenQA.Selenium;
using SpythereGamesServices.Tests.Selenium.Helpers;
using SpythereGamesServices.Tests.Selenium.Pages;

namespace SpythereGamesServices.Tests.Selenium.Tests;


[TestFixture]
public class HeroPageTests
{
    private IWebDriver _driver = null!;
    private HeroPage _heroPage = null!;

    [SetUp]
    public void SetUp()
    {
        _driver = WebDriverFactory.CreateChromeDriver();
        var baseUrl = WebDriverFactory.GetBaseUrl();

        _heroPage = new HeroPage(_driver, baseUrl);
        _heroPage.Open();
    }

    [TearDown]
    public void TearDown()
    {
        _driver.Quit();
        _driver.Dispose();
    }

    [Test]
    public void Logo_AfterPageLoad_IsVisible()
    {

        _heroPage.WaitForAnimations();

        Assert.That(_heroPage.IsLogoVisible(), Is.True,
            "Logo Spythere Games powinno być widoczne po załadowaniu strony");
    }

    [Test]
    public void Logo_HasCorrectAltText()
    {

        var altText = _heroPage.Logo.GetDomAttribute("alt");


        Assert.That(altText, Is.EqualTo("Spythere Games"),
            "Logo powinno mieć atrybut alt='Spythere Games' dla dostępności");
    }
    [Test]
    public void Logo_SrcAttribute_PointsToLogoFile()
    {
        var src = _heroPage.Logo.GetDomProperty("src");

        Assert.That(src, Does.Contain("logo.png"),
            $"Atrybut src logo powinien zawierać 'logo.png', aktualny src: {src}");
    }
    [Test]
    public void GitHubButton_IsDisplayedAndEnabled()
    {
        _heroPage.WaitForAnimations();

        var button = _heroPage.GitHubButton;

        Assert.Multiple(() =>
        {
            Assert.That(button.Displayed, Is.True,
                "Przycisk GitHub powinien być widoczny");
            Assert.That(button.Enabled, Is.True,
                "Przycisk GitHub powinien być klikalny");
        });
    }
    [Test]
    public void GitHubButton_OnClick_OpensGitHubProfileInNewTab()
    {
        _heroPage.WaitForAnimations();

        var newTabUrl = _heroPage.ClickGitHubAndGetNewTabUrl();

        Assert.That(newTabUrl, Does.Contain("github.com/Spytherr"),
            $"New tab should open the Spytherr GitHub profile, opened: {newTabUrl}");
    }

    [Test]
    public void GitHubButton_HasCorrectLabelText()
    {
        var buttonText = _heroPage.GitHubButton.Text;

        Assert.That(buttonText, Is.EqualTo("GitHub"),
            "Button should have the text 'GitHub'");
    }

    [Test]
    public void YouTubeButton_HasCorrectLabelText()
    {
        var text = _heroPage.YouTubeButton.Text;

        Assert.That(text, Is.EqualTo("YouTube"),
            "Element YouTube powinien wyświetlać tekst 'YouTube'");
    }
    [Test]
    public void HeroSection_Height_IsAtLeastFullViewport()
    {
        var sectionHeight = _heroPage.HeroSection.Size.Height;
        var viewportHeight = (long)((IJavaScriptExecutor)_driver)
            .ExecuteScript("return window.innerHeight;");

        Assert.That(sectionHeight, Is.GreaterThanOrEqualTo((int)viewportHeight),
            $"Sekcja Hero (min-h-screen) powinna być co najmniej {viewportHeight}px wysoka, " +
            $"aktualnie: {sectionHeight}px");
    }

    [Test]
    public void HeroSection_BothButtons_AreVisible()
    {
        _heroPage.WaitForAnimations();

        Assert.Multiple(() =>
        {
            Assert.That(_heroPage.GitHubButton.Displayed, Is.True,
                "Przycisk GitHub powinien być widoczny");

            Assert.That(_heroPage.YouTubeButton.Displayed, Is.True,
                "Przycisk YouTube powinien być widoczny");
        });
    }
}

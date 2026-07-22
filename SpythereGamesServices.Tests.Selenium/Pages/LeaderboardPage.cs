using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace SpythereGamesServices.Tests.Selenium.Pages;

public class LeaderboardPage : BasePage
{
    private static readonly By HeadingLocator =
        By.XPath("//h2[text()='Leaderboards']");

    private static readonly By GameTabsLocator =
        By.CssSelector("div.flex.justify-center button");

    private static readonly By ScoresTableLocator =
        By.TagName("table");

    private static readonly By ScoreRowsLocator =
        By.CssSelector("tbody tr");

    private static readonly By TableHeadersLocator =
        By.CssSelector("thead th");

    private static readonly By LoadingIndicatorLocator =
        By.XPath("//p[text()='Loading...']");


    public LeaderboardPage(IWebDriver driver, string baseUrl) : base(driver, baseUrl)
    {
    } 
       public LeaderboardPage OpenAndScrollToSection()
    {
        NavigateTo();
        ScrollToLeaderboard();
        return this;
    }

    public void ScrollToLeaderboard()
    {
        var heading = WaitForElement(HeadingLocator);
        ScrollToElement(heading);

        WaitForGameTabsToLoad();
    }

    private void WaitForGameTabsToLoad()
    {
        Wait.Until(driver =>
        {
            var tabs = driver.FindElements(GameTabsLocator);
            return tabs.Count > 0;
        });
    }
    private void WaitForLoadingToDisappear()
    {
        try
        {
            var shortWait = new WebDriverWait(Driver, TimeSpan.FromSeconds(2));
            shortWait.Until(d => d.FindElements(LoadingIndicatorLocator).Count > 0);

            Wait.Until(d => d.FindElements(LoadingIndicatorLocator).Count == 0);
        }
        catch (WebDriverTimeoutException)
        {
        }
    }

    private void WaitForScoreRowsToLoad()
    {
        Wait.Until(driver =>
        {
            var rows = driver.FindElements(ScoreRowsLocator);
            return rows.Count > 0;
        });
    }


    public IWebElement Heading => WaitForElement(HeadingLocator);

    public IReadOnlyCollection<IWebElement> GameTabs =>
        Driver.FindElements(GameTabsLocator);

    public IWebElement ScoresTable => WaitForElement(ScoresTableLocator);

    public IReadOnlyCollection<IWebElement> ScoreRows =>
        Driver.FindElements(ScoreRowsLocator);

    public IReadOnlyCollection<IWebElement> TableHeaders =>
        Driver.FindElements(TableHeadersLocator);


    public bool ClickGameTab(string tabName)
    {
        var tabs = GameTabs;
        var targetTab = tabs.FirstOrDefault(t =>
            t.Text.Equals(tabName, StringComparison.OrdinalIgnoreCase));

        if (targetTab is null)
            return false;

        targetTab.Click();

        WaitForLoadingToDisappear();

        return true;
    }

    public string ClickFirstGameTab()
    {
        WaitForGameTabsToLoad();
        var firstTab = GameTabs.First();
        var tabName = firstTab.Text;
        firstTab.Click();
        WaitForLoadingToDisappear();
        return tabName;
    }

    public string? ClickSecondGameTab()
    {
        var tabs = GameTabs.ToList();
        if (tabs.Count < 2)
            return null;

        var secondTab = tabs[1];
        var tabName = secondTab.Text;
        secondTab.Click();
        WaitForLoadingToDisappear();
        return tabName;
    }

    public IEnumerable<string> GetTableHeaderTexts()
    {
        return TableHeaders.Select(th => th.Text.Trim());
    }
    public string? GetActiveTabName()
    {
        var tabs = GameTabs;
        var activeTab = tabs.FirstOrDefault(t =>
        {
            var classes = t.GetDomAttribute("class") ?? "";
            return classes.Contains("bg-blue-500");
        });

        return activeTab?.Text;
    }

    public bool IsScoresTableVisible()
    {
        return Driver.FindElements(ScoresTableLocator).Count > 0 &&
               ScoresTable.Displayed;
    }

    public (string Rank, string PlayerName, string Score)? GetFirstRowData()
    {
        var rows = ScoreRows;
        if (!rows.Any())
            return null;

        var cells = rows.First().FindElements(By.TagName("td"));
        if (cells.Count < 3)
            return null;

        return (
            Rank: cells[0].Text.Trim(),
            PlayerName: cells[1].Text.Trim(),
            Score: cells[2].Text.Trim()
        );
    }
}

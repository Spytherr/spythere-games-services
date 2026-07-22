using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SpythereGamesServices.Tests.Selenium.Helpers;
using SpythereGamesServices.Tests.Selenium.Pages;

namespace SpythereGamesServices.Tests.Selenium.Tests;

[TestFixture]
public class LeaderboardTests
{
    private IWebDriver _driver = null!;
    private LeaderboardPage _leaderboardPage = null!;

    [SetUp]
    public void SetUp()
    {
        _driver = WebDriverFactory.CreateChromeDriver();
        var baseUrl = WebDriverFactory.GetBaseUrl();
        _leaderboardPage = new LeaderboardPage(_driver, baseUrl);

        _leaderboardPage.OpenAndScrollToSection();
    }

    [TearDown]
    public void TearDown()
    {
        _driver.Quit();
        _driver.Dispose();
    }
    [Test]
    public void LeaderboardHeading_IsVisible_AfterScrollingToSection()
    {
        var heading = _leaderboardPage.Heading;

        Assert.Multiple(() =>
        {
            Assert.That(heading.Displayed, Is.True,
                "The leaderboard section heading should be visible after scrolling");

            Assert.That(heading.Text, Is.EqualTo("Leaderboards"),
                "Heading should have the text 'Leaderboards'");
        });
    }
    [Test]
    [Category("RequiresBackend")]
    public void LeaderboardTable_HasCorrectColumnHeaders_InCorrectOrder()
    {
        var headers = _leaderboardPage.GetTableHeaderTexts().ToList();

        Assert.That(headers, Has.Count.GreaterThanOrEqualTo(4),
            "The table should have at least 4 columns");

        Assert.Multiple(() =>
        {
            Assert.That(headers[0], Is.EqualTo("#"),
                "The first column should be the ranking '#'");

            Assert.That(headers[1], Is.EqualTo("Player"),
                "The second column should be 'Player'");

            Assert.That(headers[2], Is.EqualTo("Score"),
                "The third column should be 'Score'");

            Assert.That(headers[3], Is.EqualTo("Platform"),
                "The fourth column should be 'Platform'");
        });
    }

    [Test]
    [Category("RequiresBackend")]
    public void GameTabs_AreRendered_AfterApiLoadsGames()
    {
        var tabs = _leaderboardPage.GameTabs;

        Assert.That(tabs, Is.Not.Empty,
            "At least one game tab should appear after loading from the API");

        foreach (var tab in tabs)
        {
            Assert.That(tab.Text, Is.Not.Empty,
                "Each tab should have a non-empty game name");
        }
    }

    [Test]
    [Category("RequiresBackend")]
    public void GameTabs_OnInitialLoad_FirstTabIsActiveByDefault()
    {
        var activeTabName = _leaderboardPage.GetActiveTabName();

        Assert.That(activeTabName, Is.Not.Null.And.Not.Empty,
            "After loading the page one tab should be active by default " +
            "(should have class 'bg-blue-500')");
    }

    [Test]
    [Category("RequiresBackend")]
    public void GameTab_WhenClicked_LoadsScoresTable()
    {
        _leaderboardPage.ClickFirstGameTab();

        Assert.That(_leaderboardPage.IsScoresTableVisible(), Is.True,
            "Clicking a tab should display the scores table");
    }
    [Test]
    [Category("RequiresBackend")]
    public void GameTab_WhenSwitched_ActiveTabChanges()
    {
        _leaderboardPage.ClickFirstGameTab();
        var firstActiveTab = _leaderboardPage.GetActiveTabName();

        var secondTabName = _leaderboardPage.ClickSecondGameTab();

        if (secondTabName is null)
        {
            Assert.Ignore("Test requires at least 2 games in the database");
            return;
        }

        var newActiveTab = _leaderboardPage.GetActiveTabName();

        Assert.Multiple(() =>
        {
            Assert.That(newActiveTab, Is.Not.EqualTo(firstActiveTab),
                "Clicking a different tab should change the active tab");

            Assert.That(newActiveTab, Is.EqualTo(secondTabName),
                "The active tab should have the name of the clicked game");

            Assert.That(_leaderboardPage.IsScoresTableVisible(), Is.True,
                "The scores table should still be visible after switching tabs");
        });
    }
    [Test]
    [Category("RequiresBackend")]
    public void LeaderboardTable_FirstRow_ContainsNonEmptyData()
    {
        var firstRow = _leaderboardPage.GetFirstRowData();

        Assert.That(firstRow, Is.Not.Null,
            "The table should have at least one result row");

        Assert.Multiple(() =>
        {
            Assert.That(firstRow!.Value.Rank, Is.Not.Empty,
                "Rank column (place number) should not be empty");

            Assert.That(firstRow.Value.PlayerName, Is.Not.Empty,
                "Player column (player name) should not be empty");

            Assert.That(firstRow.Value.Score, Is.Not.Empty,
                "Score column should not be empty");
        });
    }

    [Test]
    [Category("RequiresBackend")]
    public void LeaderboardTable_Ranks_AreInAscendingOrder()
    {
        var rows = _leaderboardPage.ScoreRows.ToList();

        if (rows.Count == 0)
        {
            Assert.Ignore("No results in the table — test cannot be performed");
            return;
        }

        var rankTexts = rows
            .Select(row => row.FindElements(By.TagName("td")).FirstOrDefault()?.Text.Trim())
            .Where(text => !string.IsNullOrEmpty(text))
            .ToList();

        var ranks = rankTexts
            .Select(text => int.TryParse(text, out var rank) ? rank : -1)
            .Where(r => r > 0)
            .ToList();

        Assert.That(ranks, Is.Not.Empty,
            "There should be at least a few rows with numeric rankings");

        Assert.That(ranks[0], Is.EqualTo(1),
            "The first row should have rank = 1 (best score)");

        for (int i = 1; i < ranks.Count; i++)
        {
            Assert.That(ranks[i], Is.GreaterThanOrEqualTo(ranks[i - 1]),
                $"Ranks should be in ascending order. Row {i}: rank={ranks[i]}, previous: {ranks[i - 1]}");
        }
    }

    [Test]
    [Category("RequiresBackend")]
    public void LeaderboardTable_EachRow_HasPlatformIcon()
    {
        var rows = _leaderboardPage.ScoreRows.ToList();

        if (rows.Count == 0)
        {
            Assert.Ignore("No results — test cannot be performed");
            return;
        }

        foreach (var row in rows)
        {
            var platformImages = row.FindElements(By.TagName("img"));

            Assert.That(platformImages, Is.Not.Empty,
                "Each table row should contain a platform icon (<img>)");
        }
    }
}

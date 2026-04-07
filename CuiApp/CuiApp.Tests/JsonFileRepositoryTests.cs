using System.Collections.Generic;
using CuiApp.Repositories;
using GameCore.Models;
using NUnit.Framework;

namespace CuiApp.Tests;

/// <summary>CT-6: JsonFileRepository round-trip test.</summary>
[TestFixture]
public class JsonFileRepositoryTests
{
    private string _tempPath = "";

    [SetUp]
    public void SetUp()
    {
        _tempPath = Path.Combine(Path.GetTempPath(), $"test_save_{Guid.NewGuid()}.json");
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(_tempPath))
            File.Delete(_tempPath);
    }

    [Test]
    public void Load_NoFile_ReturnsDefault()
    {
        var repo = new JsonFileRepository(_tempPath);
        var data = repo.Load();

        Assert.That(data.IsFirstRun, Is.True);
        Assert.That(data.Gold, Is.EqualTo(0));
    }

    [Test]
    public void SaveAndLoad_RoundTrip_PreservesData()
    {
        var repo = new JsonFileRepository(_tempPath);

        var original = PlayerData.CreateDefault().With(
            gold: 1234,
            fragments: 42,
            isFirstRun: false,
            stats: new Statistics(
                highestEnhance: 15,
                totalDestroys: 7,
                totalEnhances: 100));

        repo.Save(original);
        var loaded = repo.Load();

        Assert.That(loaded.Gold, Is.EqualTo(1234));
        Assert.That(loaded.Fragments, Is.EqualTo(42));
        Assert.That(loaded.IsFirstRun, Is.False);
        Assert.That(loaded.Stats.HighestEnhance, Is.EqualTo(15));
        Assert.That(loaded.Stats.TotalDestroys, Is.EqualTo(7));
        Assert.That(loaded.Stats.TotalEnhances, Is.EqualTo(100));
    }

    [Test]
    public void SaveAndLoad_PreservesCollectedSwords()
    {
        var repo = new JsonFileRepository(_tempPath);

        var original = PlayerData.CreateDefault().With(
            collectedSwords: new List<int> { 10, 12, 15 });

        repo.Save(original);
        var loaded = repo.Load();

        Assert.That(loaded.CollectedSwords, Is.EquivalentTo(new[] { 10, 12, 15 }));
    }

    [Test]
    public void SaveAndLoad_PreservesInventory()
    {
        var repo = new JsonFileRepository(_tempPath);

        var original = PlayerData.CreateDefault().With(
            items: new Inventory(protectionAmulets: 3, blessingScrolls: 2, goldPouches: 1));

        repo.Save(original);
        var loaded = repo.Load();

        Assert.That(loaded.Items.ProtectionAmulets, Is.EqualTo(3));
        Assert.That(loaded.Items.BlessingScrolls, Is.EqualTo(2));
        Assert.That(loaded.Items.GoldPouches, Is.EqualTo(1));
    }

    [Test]
    public void SaveAndLoad_PreservesMasteryExp()
    {
        var repo = new JsonFileRepository(_tempPath);

        var original = PlayerData.CreateDefault().With(masteryExp: 500);

        repo.Save(original);
        var loaded = repo.Load();

        Assert.That(loaded.MasteryExp, Is.EqualTo(500));
    }
}

using GameCore.Data;
using NUnit.Framework;

namespace CuiApp.Tests;

[TestFixture]
public class MasteryTableTests
{
    [TestCase(0, 1)]
    [TestCase(49, 1)]
    [TestCase(50, 2)]
    [TestCase(150, 3)]
    [TestCase(15000, 10)]
    public void GetLevel_ReturnsCorrectLevel(int exp, int expectedLevel)
    {
        Assert.That(MasteryTable.GetLevel(exp), Is.EqualTo(expectedLevel));
    }

    [Test]
    public void GetExpRange_AtZero_Returns0To50()
    {
        var (current, next) = MasteryTable.GetExpRange(0);
        Assert.That(current, Is.EqualTo(0));
        Assert.That(next, Is.EqualTo(50));
    }

    [Test]
    public void GetExpRange_AtMax_ReturnsSameValues()
    {
        var (current, next) = MasteryTable.GetExpRange(15000);
        Assert.That(current, Is.EqualTo(next));
    }

    [TestCase(1, "초보 장인")]
    [TestCase(4, "중급 장인")]
    [TestCase(7, "숙련 장인")]
    [TestCase(10, "마스터 스미스")]
    public void GetTitle_ReturnsExpectedTitle(int level, string expected)
    {
        Assert.That(MasteryTable.GetTitle(level), Is.EqualTo(expected));
    }
}

using NUnit.Framework;
using GameCore.Data;
using System.Linq;

namespace GameCore.Tests.Data
{
    [TestFixture]
    public class SwordDataLoaderTests
    {
        [Test]
        public void Parse_FullCsv_Loads21Swords()
        {
            var table = TestHelper.LoadFullTable();
            Assert.That(table.MaxLevel, Is.EqualTo(20));
        }

        [Test]
        public void Parse_WoodenSword_HasZeroValues()
        {
            var table = TestHelper.LoadFullTable();
            var sword = table.GetSword(0);

            Assert.That(sword.Name, Is.EqualTo("나무검"));
            Assert.That(sword.SuccessRate, Is.EqualTo(0));
            Assert.That(sword.EnhanceCost, Is.EqualTo(0));
            Assert.That(sword.SellPrice, Is.EqualTo(0));
            Assert.That(sword.Collectible, Is.False);
        }

        [Test]
        public void Parse_PercentageConversion()
        {
            var table = TestHelper.LoadFullTable();

            Assert.That(table.GetSword(1).SuccessRate, Is.EqualTo(0.95));
            Assert.That(table.GetSword(10).SuccessRate, Is.EqualTo(0.35));
            Assert.That(table.GetSword(20).SuccessRate, Is.EqualTo(0.001));
        }

        [Test]
        public void Parse_EnhanceCosts()
        {
            var table = TestHelper.LoadFullTable();

            Assert.That(table.GetSword(1).EnhanceCost, Is.EqualTo(5));
            Assert.That(table.GetSword(10).EnhanceCost, Is.EqualTo(270));
            Assert.That(table.GetSword(20).EnhanceCost, Is.EqualTo(30000));
        }

        [Test]
        public void Parse_SellPrices()
        {
            var table = TestHelper.LoadFullTable();

            Assert.That(table.GetSword(1).SellPrice, Is.EqualTo(4));
            Assert.That(table.GetSword(9).SellPrice, Is.EqualTo(505));
            Assert.That(table.GetSword(20).SellPrice, Is.EqualTo(133950));
        }

        [Test]
        public void Parse_CollectibleFlags()
        {
            var table = TestHelper.LoadFullTable();

            // Levels 0-9 are not collectible
            for (int i = 0; i <= 9; i++)
                Assert.That(table.GetSword(i).Collectible, Is.False, $"+{i} should not be collectible");

            // Levels 10-20 are collectible
            for (int i = 10; i <= 20; i++)
                Assert.That(table.GetSword(i).Collectible, Is.True, $"+{i} should be collectible");
        }

        [Test]
        public void Parse_FragmentRewards()
        {
            var table = TestHelper.LoadFullTable();

            Assert.That(table.GetSword(1).FragmentReward, Is.EqualTo(1));
            Assert.That(table.GetSword(6).FragmentReward, Is.EqualTo(3));
            Assert.That(table.GetSword(10).FragmentReward, Is.EqualTo(8));
            Assert.That(table.GetSword(15).FragmentReward, Is.EqualTo(20));
            Assert.That(table.GetSword(18).FragmentReward, Is.EqualTo(50));
        }

        [Test]
        public void Parse_ReturnRateConversion()
        {
            var table = TestHelper.LoadFullTable();

            Assert.That(table.GetSword(1).ReturnRate, Is.EqualTo(0.70));
            Assert.That(table.GetSword(9).ReturnRate, Is.EqualTo(1.00));
            Assert.That(table.GetSword(14).ReturnRate, Is.EqualTo(1.60));
            Assert.That(table.GetSword(20).ReturnRate, Is.EqualTo(2.00));
        }

        [Test]
        public void GetSword_ReturnsNull_ForInvalidLevel()
        {
            var table = TestHelper.LoadFullTable();
            Assert.That(table.GetSword(99), Is.Null);
            Assert.That(table.GetSword(-1), Is.Null);
        }

        [Test]
        public void Parse_EmptyInput_ReturnsEmpty()
        {
            var loader = new SwordDataLoader();
            var swords = loader.Parse("header\n");
            Assert.That(swords, Is.Empty);
        }

        [Test]
        public void Parse_MalformedLine_Skipped()
        {
            var loader = new SwordDataLoader();
            var csv = "header\n+0,나무검,기본,-,0,0,-,-,-,N\nbad,line\n+1,철검,기본,95,5,5,4,70%,1,N";
            var swords = loader.Parse(csv);

            Assert.That(swords.Count, Is.EqualTo(2));
        }
    }
}

using NUnit.Framework;
using GameCore.Data;

namespace GameCore.Tests.Data
{
    [TestFixture]
    public class MasteryDataLoaderTests
    {
        [Test]
        public void Parse_FullCsv_Loads10Levels()
        {
            var table = TestHelper.LoadFullMasteryTable();
            Assert.That(table.MaxLevel, Is.EqualTo(10));
        }

        [Test]
        public void Parse_Level1_HasZeroRequirements()
        {
            var table = TestHelper.LoadFullMasteryTable();
            var level = table.GetLevel(1);

            Assert.That(level.RequiredExp, Is.EqualTo(0));
            Assert.That(level.CostDiscount, Is.EqualTo(0));
            Assert.That(level.FragmentBonus, Is.EqualTo(0));
        }

        [Test]
        public void Parse_CostDiscountConversion()
        {
            var table = TestHelper.LoadFullMasteryTable();

            Assert.That(table.GetLevel(2).CostDiscount, Is.EqualTo(0.05));
            Assert.That(table.GetLevel(4).CostDiscount, Is.EqualTo(0.10));
            Assert.That(table.GetLevel(6).CostDiscount, Is.EqualTo(0.15));
            Assert.That(table.GetLevel(8).CostDiscount, Is.EqualTo(0.20));
            Assert.That(table.GetLevel(10).CostDiscount, Is.EqualTo(0.25));
        }

        [Test]
        public void Parse_FragmentBonus()
        {
            var table = TestHelper.LoadFullMasteryTable();

            Assert.That(table.GetLevel(6).FragmentBonus, Is.EqualTo(0));
            Assert.That(table.GetLevel(7).FragmentBonus, Is.EqualTo(1));
            Assert.That(table.GetLevel(10).FragmentBonus, Is.EqualTo(1));
        }

        [Test]
        public void Parse_RequiredExp()
        {
            var table = TestHelper.LoadFullMasteryTable();

            Assert.That(table.GetLevel(1).RequiredExp, Is.EqualTo(0));
            Assert.That(table.GetLevel(2).RequiredExp, Is.EqualTo(50));
            Assert.That(table.GetLevel(5).RequiredExp, Is.EqualTo(800));
            Assert.That(table.GetLevel(10).RequiredExp, Is.EqualTo(15000));
        }

        [Test]
        public void GetLevel_ReturnsNull_ForInvalidLevel()
        {
            var table = TestHelper.LoadFullMasteryTable();
            Assert.That(table.GetLevel(0), Is.Null);
            Assert.That(table.GetLevel(99), Is.Null);
        }

        [Test]
        public void Parse_EmptyInput_ReturnsEmpty()
        {
            var loader = new MasteryDataLoader();
            var levels = loader.Parse("header\n");
            Assert.That(levels, Is.Empty);
        }
    }
}

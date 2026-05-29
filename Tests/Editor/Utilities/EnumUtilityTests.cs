using System.Collections.Generic;
using NUnit.Framework;

namespace USTL.FaceTracking.Editor.Tests
{
    public sealed class EnumUtilityTests
    {
        [Test]
        public void GetAllElements_ReturnsNonNegativeValues_ByDefault()
        {
            List<TestEnum> elements = EnumUtility.GetAllElements<TestEnum>();

            Assert.That(elements, Is.EqualTo(new[] { TestEnum.Zero, TestEnum.Positive, }));
        }

        [Test]
        public void GetAllElements_ReturnsAllValues_WhenIncludeNegativeIsTrue()
        {
            List<TestEnum> elements = EnumUtility.GetAllElements<TestEnum>(true);

            Assert.That(elements, Is.EquivalentTo(new[] { TestEnum.Negative, TestEnum.Zero, TestEnum.Positive, }));
        }

        [Test]
        public void GetAllElements_ReturnsAllValues_WhenIncludeNegativeIsFalse()
        {
            List<TestEnum> elements = EnumUtility.GetAllElements<TestEnum>();

            Assert.That(elements, Is.EquivalentTo(new[] { TestEnum.Zero, TestEnum.Positive, }));
        }

        private enum TestEnum
        {
            Negative = -1,
            Zero = 0,
            Positive = 1,
        }
    }
}

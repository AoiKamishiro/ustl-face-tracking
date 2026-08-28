using NUnit.Framework;

namespace USTL.FaceTracking.Editor.Tests
{
    public sealed class BlendshapeUtilityTests
    {
        [TestCase(1.0f, 1.0f)]
        [TestCase(50.0f, 50.0f)]
        [TestCase(100.0f, 100.0f)]
        [TestCase(1000.0f, 1000.0f)]
        [TestCase(0.0f, 0.0f)]
        [TestCase(-1.0f, 0.0f)]
        [TestCase(1001.0f, 1000.0f)]
        public void ClampMaxValue_ClampsFiniteValueToSupportedRange(float value, float expected)
        {
            Assert.That(BlendshapeUtility.ClampMaxValue(value), Is.EqualTo(expected));
        }

        [Test]
        public void ClampMaxValue_ReturnsDefaultValue_WhenValueIsNotFinite()
        {
            Assert.That(BlendshapeUtility.ClampMaxValue(float.NaN), Is.EqualTo(BlendshapeUtility.DefaultMaxValue));
            Assert.That(BlendshapeUtility.ClampMaxValue(float.PositiveInfinity), Is.EqualTo(BlendshapeUtility.DefaultMaxValue));
            Assert.That(BlendshapeUtility.ClampMaxValue(float.NegativeInfinity), Is.EqualTo(BlendshapeUtility.DefaultMaxValue));
        }

        [TestCase(1.0f, 100.0f)]
        [TestCase(100.0f, 1.0f)]
        [TestCase(1000.0f, 0.1f)]
        [TestCase(0.0f, 0.0f)]
        [TestCase(-1.0f, 0.0f)]
        [TestCase(1001.0f, 0.1f)]
        public void GetMaxValueDeltaScale_UsesClampedMaxValue(float maxValue, float expected)
        {
            Assert.That(BlendshapeUtility.GetMaxValueDeltaScale(maxValue), Is.EqualTo(expected).Within(0.0001f));
        }

        [Test]
        public void GetMaxValueDeltaScale_ReturnsOne_WhenValueIsNotFinite()
        {
            Assert.That(BlendshapeUtility.GetMaxValueDeltaScale(float.NaN), Is.EqualTo(1.0f));
            Assert.That(BlendshapeUtility.GetMaxValueDeltaScale(float.PositiveInfinity), Is.EqualTo(1.0f));
            Assert.That(BlendshapeUtility.GetMaxValueDeltaScale(float.NegativeInfinity), Is.EqualTo(1.0f));
        }
    }
}

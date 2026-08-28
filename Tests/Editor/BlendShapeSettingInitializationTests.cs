using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace USTL.FaceTracking.Editor.Tests
{
    public sealed class BlendShapeSettingInitializationTests
    {
        [Test]
        public void AddComponent_InitializesBlendShapeSettingsWithoutOpeningInspector()
        {
            GameObject gameObject = new("FaceTrackingTest");
            try
            {
                USTLFaceTracking faceTracking = gameObject.AddComponent<USTLFaceTracking>();

                AssertNormalized(faceTracking.blendShapeSettings);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void OnValidate_RepairsInvalidSerializedSettings()
        {
            GameObject gameObject = new("FaceTrackingTest");
            try
            {
                USTLFaceTracking faceTracking = gameObject.AddComponent<USTLFaceTracking>();
                faceTracking.blendShapeSettings = new[]
                {
                    null,
                    new BlendShapeSetting
                    {
                        expression = UnifiedExpression.JawOpen,
                        blendShapeName = "First",
                        maxValue = float.NaN,
                    },
                    new BlendShapeSetting
                    {
                        expression = UnifiedExpression.None,
                        blendShapeName = "Ignored",
                        maxValue = 50.0f,
                    },
                    new BlendShapeSetting
                    {
                        expression = UnifiedExpression.JawOpen,
                        blendShapeName = "  ",
                        maxValue = 2000.0f,
                    },
                };

                typeof(USTLFaceTracking)
                    .GetMethod("OnValidate", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.Invoke(faceTracking, null);

                AssertNormalized(faceTracking.blendShapeSettings);
                BlendShapeSetting jawOpen = faceTracking.blendShapeSettings.Single(x => x.expression == UnifiedExpression.JawOpen);
                Assert.That(jawOpen.blendShapeName, Is.EqualTo(nameof(UnifiedExpression.JawOpen)));
                Assert.That(jawOpen.maxValue, Is.EqualTo(BlendShapeSettingNormalizer.MaxMaxValue));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Normalize_DoesNotReplaceAlreadyNormalizedArray()
        {
            BlendShapeSetting[] settings = BlendShapeSettingNormalizer.Normalize(null);

            Assert.That(BlendShapeSettingNormalizer.Normalize(settings), Is.SameAs(settings));
        }

        private static void AssertNormalized(BlendShapeSetting[] settings)
        {
            UnifiedExpression[] expected = Enum.GetValues(typeof(UnifiedExpression))
                .Cast<UnifiedExpression>()
                .Where(expression => Convert.ToInt64(expression) >= 0)
                .ToArray();

            Assert.That(settings, Has.Length.EqualTo(expected.Length));
            Assert.That(settings.Select(x => x.expression), Is.EqualTo(expected));
            Assert.That(settings, Has.None.Null);
            Assert.That(settings.All(x => !string.IsNullOrWhiteSpace(x.blendShapeName)), Is.True);
        }
    }
}

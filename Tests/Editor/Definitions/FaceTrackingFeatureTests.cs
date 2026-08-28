using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace USTL.FaceTracking.Editor.Tests
{
    public sealed class FaceTrackingFeatureTests
    {
        [Test]
        public void FaceTrackingFeatureDefinitions_ContainsAll()
        {
            List<FaceTrackingFeature> all = new();
            foreach (FaceTrackingFeature item in Enum.GetValues(typeof(FaceTrackingFeature)))
            {
                all.Add(item);
            }

            Assert.That(FaceTrackingFeatureDefinition.All.Count, Is.EqualTo(all.Count));

            foreach (FaceTrackingFeature item in all)
            {
                Assert.That(FaceTrackingFeatureDefinition.All.ContainsKey(item), Is.True);
            }
        }
    }
}

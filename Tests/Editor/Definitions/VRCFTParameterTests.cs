using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace USTL.FaceTracking.Editor.Tests
{
    public sealed class VRCFTParameterTests
    {
        [Test]
        public void VRCFTParameterDefinitions_ContainsAll()
        {
            List<VRCFTParameter> all = new();
            foreach (VRCFTParameter item in Enum.GetValues(typeof(VRCFTParameter)))
            {
                if (item != VRCFTParameter.None)
                {
                    all.Add(item);
                }
            }

            Assert.That(VRCFTParameterDefinition.All.Count, Is.EqualTo(all.Count));

            foreach (VRCFTParameter item in all)
            {
                Assert.That(VRCFTParameterDefinition.All.ContainsKey(item), Is.True);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
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

        [TestCase((int)VRCFTParameter.TongueArchY, UnifiedExpression.TongueCurlUp, UnifiedExpression.TongueBendDown)]
        [TestCase((int)VRCFTParameter.TongueShape, UnifiedExpression.TongueFlat, UnifiedExpression.TongueSquish)]
        public void TongueSignedDirections_MatchVRCFT5SenderImplementation(
            int parameterValue,
            UnifiedExpression positiveExpression,
            UnifiedExpression negativeExpression)
        {
            VRCFTParameter parameter = (VRCFTParameter)parameterValue;
            VRCFTParameterDefinition definition = VRCFTParameterDefinition.All[parameter];

            Assert.That(definition.Range, Is.EqualTo(ParameterRangeKind.Signed));
            Assert.That(
                definition.ExpressionTargets.Select(target => (target.Expression, target.Type)),
                Is.EqualTo(new[]
                {
                    (positiveExpression, WeightCurveType.PositiveSigned),
                    (negativeExpression, WeightCurveType.NegativeSigned),
                }));
        }
    }
}

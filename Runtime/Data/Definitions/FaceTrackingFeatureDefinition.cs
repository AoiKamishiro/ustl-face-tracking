using System;
using System.Collections.Generic;

namespace USTL.FaceTracking
{
    internal sealed class FaceTrackingFeatureDefinition
    {
        internal static readonly IReadOnlyDictionary<FaceTrackingFeature, FaceTrackingFeatureDefinition> All = new Dictionary<FaceTrackingFeature, FaceTrackingFeatureDefinition>
        {
            [FaceTrackingFeature.EyeDirection] = new(FaceTrackingFeature.EyeDirection, "Eye Direction", new VRCFTParameterSet(VRCFTParameterSetId.PerEyeGazeXY, VRCFTParameter.EyeLeftX, VRCFTParameter.EyeLeftY, VRCFTParameter.EyeRightX, VRCFTParameter.EyeRightY), new VRCFTParameterSet(VRCFTParameterSetId.UnifiedGazeXY, VRCFTParameter.EyeX, VRCFTParameter.EyeY), new VRCFTParameterSet(VRCFTParameterSetId.VRChatNative)),
            [FaceTrackingFeature.EyeLid] = new(FaceTrackingFeature.EyeLid, "Eye Lid", new VRCFTParameterSet(VRCFTParameterSetId.PerEyeLid, VRCFTParameter.EyeLidLeft, VRCFTParameter.EyeLidRight), new VRCFTParameterSet(VRCFTParameterSetId.UnifiedEyeLid, VRCFTParameter.EyeLid), new VRCFTParameterSet(VRCFTParameterSetId.VRChatNative)),
            [FaceTrackingFeature.EyeSquint] = new(FaceTrackingFeature.EyeSquint, "Eye Squint", new VRCFTParameterSet(VRCFTParameterSetId.PerEyeSquint, VRCFTParameter.EyeSquintLeft, VRCFTParameter.EyeSquintRight), new VRCFTParameterSet(VRCFTParameterSetId.UnifiedEyeSquint, VRCFTParameter.EyeSquint)),
            [FaceTrackingFeature.PupilDilationDiameter] = new(FaceTrackingFeature.PupilDilationDiameter, "Pupil Dilation / Diameter", new VRCFTParameterSet(VRCFTParameterSetId.PerEyePupilDiameter, VRCFTParameter.PupilDiameterLeft, VRCFTParameter.PupilDiameterRight), new VRCFTParameterSet(VRCFTParameterSetId.UnifiedPupilDilation, VRCFTParameter.PupilDilation), new VRCFTParameterSet(VRCFTParameterSetId.UnifiedPupilDiameter, VRCFTParameter.PupilDiameter)),
            [FaceTrackingFeature.Brow] = new(FaceTrackingFeature.Brow, "Brow", new VRCFTParameterSet(VRCFTParameterSetId.DetailedPerSideBrowParts, VRCFTParameter.BrowPinchRight, VRCFTParameter.BrowPinchLeft, VRCFTParameter.BrowLowererRight, VRCFTParameter.BrowLowererLeft, VRCFTParameter.BrowInnerUpRight, VRCFTParameter.BrowInnerUpLeft, VRCFTParameter.BrowOuterUpRight, VRCFTParameter.BrowOuterUpLeft), new VRCFTParameterSet(VRCFTParameterSetId.PerSideBrowDownUnifiedInnerOuterUp, VRCFTParameter.BrowDownRight, VRCFTParameter.BrowDownLeft, VRCFTParameter.BrowInnerUp, VRCFTParameter.BrowOuterUp), new VRCFTParameterSet(VRCFTParameterSetId.PerSideBrowDownUnifiedBrowUp, VRCFTParameter.BrowDownRight, VRCFTParameter.BrowDownLeft, VRCFTParameter.BrowUp), new VRCFTParameterSet(VRCFTParameterSetId.PerSideBrowExpression, VRCFTParameter.BrowExpressionRight, VRCFTParameter.BrowExpressionLeft), new VRCFTParameterSet(VRCFTParameterSetId.UnifiedBrowExpression, VRCFTParameter.BrowExpression)),
            [FaceTrackingFeature.NoseSneer] = new(FaceTrackingFeature.NoseSneer, "Nose Sneer", new VRCFTParameterSet(VRCFTParameterSetId.PerSideNoseSneer, VRCFTParameter.NoseSneerLeft, VRCFTParameter.NoseSneerRight), new VRCFTParameterSet(VRCFTParameterSetId.UnifiedNoseSneer, VRCFTParameter.NoseSneer)),
            [FaceTrackingFeature.NasalDilation] = new(FaceTrackingFeature.NasalDilation, "Nasal Dilation", new VRCFTParameterSet(VRCFTParameterSetId.PerSideNasalDilation, VRCFTParameter.NasalDilationLeft, VRCFTParameter.NasalDilationRight)),
            [FaceTrackingFeature.NasalConstrict] = new(FaceTrackingFeature.NasalConstrict, "Nasal Constrict", new VRCFTParameterSet(VRCFTParameterSetId.PerSideNasalConstrict, VRCFTParameter.NasalConstrictLeft, VRCFTParameter.NasalConstrictRight)),
            [FaceTrackingFeature.CheekSquint] = new(FaceTrackingFeature.CheekSquint, "Cheek Squint", new VRCFTParameterSet(VRCFTParameterSetId.PerSideCheekSquint, VRCFTParameter.CheekSquintLeft, VRCFTParameter.CheekSquintRight), new VRCFTParameterSet(VRCFTParameterSetId.UnifiedCheekSquint, VRCFTParameter.CheekSquint)),
            [FaceTrackingFeature.CheekPuffSuck] = new(FaceTrackingFeature.CheekPuffSuck, "Cheek Puff / Suck", new VRCFTParameterSet(VRCFTParameterSetId.PerSideCheekPuffSuck, VRCFTParameter.CheekPuffSuckLeft, VRCFTParameter.CheekPuffSuckRight), new VRCFTParameterSet(VRCFTParameterSetId.UnifiedCheekPuffSuck, VRCFTParameter.CheekPuffSuck)),
            [FaceTrackingFeature.JawOpen] = new(FaceTrackingFeature.JawOpen, "Jaw Open", new VRCFTParameterSet(VRCFTParameterSetId.SingleJawOpen, VRCFTParameter.JawOpen)),
            [FaceTrackingFeature.MouthClosed] = new(FaceTrackingFeature.MouthClosed, "Mouth Closed", new VRCFTParameterSet(VRCFTParameterSetId.SingleMouthClosed, VRCFTParameter.MouthClosed)),
            [FaceTrackingFeature.JawDirection] = new(FaceTrackingFeature.JawDirection, "Jaw Direction", new VRCFTParameterSet(VRCFTParameterSetId.JawXZAxes, VRCFTParameter.JawX, VRCFTParameter.JawZ)),
            [FaceTrackingFeature.JawClench] = new(FaceTrackingFeature.JawClench, "Jaw Clench", new VRCFTParameterSet(VRCFTParameterSetId.SingleJawClench, VRCFTParameter.JawClench)),
            [FaceTrackingFeature.JawMandibleRaise] = new(FaceTrackingFeature.JawMandibleRaise, "Jaw Mandible Raise", new VRCFTParameterSet(VRCFTParameterSetId.SingleMandibleRaise, VRCFTParameter.JawMandibleRaise)),
            [FaceTrackingFeature.LipSuck] = new(FaceTrackingFeature.LipSuck, "Lip Suck", new VRCFTParameterSet(VRCFTParameterSetId.DetailedLipSuck, VRCFTParameter.LipSuckUpperRight, VRCFTParameter.LipSuckUpperLeft, VRCFTParameter.LipSuckLowerRight, VRCFTParameter.LipSuckLowerLeft), new VRCFTParameterSet(VRCFTParameterSetId.UpperLowerLipSuckUnifiedSides, VRCFTParameter.LipSuckUpper, VRCFTParameter.LipSuckLower), new VRCFTParameterSet(VRCFTParameterSetId.UnifiedLipSuck, VRCFTParameter.LipSuck)),
            [FaceTrackingFeature.LipSuckCorner] = new(FaceTrackingFeature.LipSuckCorner, "Lip Suck Corner", new VRCFTParameterSet(VRCFTParameterSetId.PerSideLipSuckCorner, VRCFTParameter.LipSuckCornerLeft, VRCFTParameter.LipSuckCornerRight)),
            [FaceTrackingFeature.LipFunnel] = new(FaceTrackingFeature.LipFunnel, "Lip Funnel", new VRCFTParameterSet(VRCFTParameterSetId.DetailedLipFunnel, VRCFTParameter.LipFunnelUpperRight, VRCFTParameter.LipFunnelUpperLeft, VRCFTParameter.LipFunnelLowerRight, VRCFTParameter.LipFunnelLowerLeft), new VRCFTParameterSet(VRCFTParameterSetId.UpperLowerLipFunnelUnifiedSides, VRCFTParameter.LipFunnelUpper, VRCFTParameter.LipFunnelLower), new VRCFTParameterSet(VRCFTParameterSetId.UnifiedLipFunnel, VRCFTParameter.LipFunnel)),
            [FaceTrackingFeature.LipPucker] = new(FaceTrackingFeature.LipPucker, "Lip Pucker", new VRCFTParameterSet(VRCFTParameterSetId.DetailedLipPucker, VRCFTParameter.LipPuckerUpperRight, VRCFTParameter.LipPuckerUpperLeft, VRCFTParameter.LipPuckerLowerRight, VRCFTParameter.LipPuckerLowerLeft), new VRCFTParameterSet(VRCFTParameterSetId.UpperLowerLipPuckerUnifiedSides, VRCFTParameter.LipPuckerUpper, VRCFTParameter.LipPuckerLower), new VRCFTParameterSet(VRCFTParameterSetId.UnifiedLipPucker, VRCFTParameter.LipPucker)),
            [FaceTrackingFeature.MouthOpen] = new(FaceTrackingFeature.MouthOpen, "Mouth Open", new VRCFTParameterSet(VRCFTParameterSetId.DetailedMouthOpen, VRCFTParameter.MouthUpperUpRight, VRCFTParameter.MouthUpperUpLeft, VRCFTParameter.MouthLowerDownRight, VRCFTParameter.MouthLowerDownLeft), new VRCFTParameterSet(VRCFTParameterSetId.UpperLowerMouthOpenUnifiedSides, VRCFTParameter.MouthUpperUp, VRCFTParameter.MouthLowerDown), new VRCFTParameterSet(VRCFTParameterSetId.UnifiedMouthOpen, VRCFTParameter.MouthOpen)),
            [FaceTrackingFeature.MouthUpperDeepen] = new(FaceTrackingFeature.MouthUpperDeepen, "Mouth Upper Deepen", new VRCFTParameterSet(VRCFTParameterSetId.PerSideMouthUpperDeepen, VRCFTParameter.MouthUpperDeepenLeft, VRCFTParameter.MouthUpperDeepenRight)),
            [FaceTrackingFeature.MouthX] = new(FaceTrackingFeature.MouthX, "Mouth X", new VRCFTParameterSet(VRCFTParameterSetId.UpperLowerMouthX, VRCFTParameter.MouthUpperX, VRCFTParameter.MouthLowerX), new VRCFTParameterSet(VRCFTParameterSetId.UnifiedMouthX, VRCFTParameter.MouthX)),
            [FaceTrackingFeature.MouthSmileFrownSad] = new(FaceTrackingFeature.MouthSmileFrownSad, "Mouth Smile / Frown / Sad", new VRCFTParameterSet(VRCFTParameterSetId.DetailedMouthCorners, VRCFTParameter.MouthCornerPullRight, VRCFTParameter.MouthCornerPullLeft, VRCFTParameter.MouthCornerSlantRight, VRCFTParameter.MouthCornerSlantLeft, VRCFTParameter.MouthFrownRight, VRCFTParameter.MouthFrownLeft, VRCFTParameter.MouthStretchRight, VRCFTParameter.MouthStretchLeft), new VRCFTParameterSet(VRCFTParameterSetId.PerSideMouthSmileSad, VRCFTParameter.MouthSmileRight, VRCFTParameter.MouthSmileLeft, VRCFTParameter.MouthSadRight, VRCFTParameter.MouthSadLeft), new VRCFTParameterSet(VRCFTParameterSetId.PerSideSmileFrownStretch, VRCFTParameter.SmileFrownRight, VRCFTParameter.SmileFrownLeft, VRCFTParameter.MouthStretchRight, VRCFTParameter.MouthStretchLeft), new VRCFTParameterSet(VRCFTParameterSetId.PerSideSmileSad, VRCFTParameter.SmileSadRight, VRCFTParameter.SmileSadLeft), new VRCFTParameterSet(VRCFTParameterSetId.UnifiedSmileFrownPerSideStretch, VRCFTParameter.SmileFrown, VRCFTParameter.MouthStretchRight, VRCFTParameter.MouthStretchLeft), new VRCFTParameterSet(VRCFTParameterSetId.UnifiedSmileSad, VRCFTParameter.SmileSad)),
            [FaceTrackingFeature.MouthDimple] = new(FaceTrackingFeature.MouthDimple, "Mouth Dimple", new VRCFTParameterSet(VRCFTParameterSetId.PerSideMouthDimple, VRCFTParameter.MouthDimpleLeft, VRCFTParameter.MouthDimpleRight)),
            [FaceTrackingFeature.MouthRaiser] = new(FaceTrackingFeature.MouthRaiser, "Mouth Raiser", new VRCFTParameterSet(VRCFTParameterSetId.UpperLowerMouthRaiser, VRCFTParameter.MouthRaiserUpper, VRCFTParameter.MouthRaiserLower)),
            [FaceTrackingFeature.MouthPress] = new(FaceTrackingFeature.MouthPress, "Mouth Press", new VRCFTParameterSet(VRCFTParameterSetId.PerSideMouthPress, VRCFTParameter.MouthPressLeft, VRCFTParameter.MouthPressRight)),
            [FaceTrackingFeature.MouthTightener] = new(FaceTrackingFeature.MouthTightener, "Mouth Tightener", new VRCFTParameterSet(VRCFTParameterSetId.PerSideMouthTightener, VRCFTParameter.MouthTightenerLeft, VRCFTParameter.MouthTightenerRight)),
            [FaceTrackingFeature.TongueOut] = new(FaceTrackingFeature.TongueOut, "Tongue Out", new VRCFTParameterSet(VRCFTParameterSetId.SingleTongueOut, VRCFTParameter.TongueOut)),
            [FaceTrackingFeature.TongueDirection] = new(FaceTrackingFeature.TongueDirection, "Tongue Direction", new VRCFTParameterSet(VRCFTParameterSetId.TongueXYAxes, VRCFTParameter.TongueX, VRCFTParameter.TongueY)),
            [FaceTrackingFeature.TongueRoll] = new(FaceTrackingFeature.TongueRoll, "Tongue Roll", new VRCFTParameterSet(VRCFTParameterSetId.SingleTongueRoll, VRCFTParameter.TongueRoll)),
            [FaceTrackingFeature.TongueArchY] = new(FaceTrackingFeature.TongueArchY, "Tongue Arch Y", new VRCFTParameterSet(VRCFTParameterSetId.SingleTongueArchY, VRCFTParameter.TongueArchY)),
            [FaceTrackingFeature.TongueShape] = new(FaceTrackingFeature.TongueShape, "Tongue Shape", new VRCFTParameterSet(VRCFTParameterSetId.SingleTongueShape, VRCFTParameter.TongueShape)),
            [FaceTrackingFeature.TongueTwist] = new(FaceTrackingFeature.TongueTwist, "Tongue Twist", new VRCFTParameterSet(VRCFTParameterSetId.PerSideTongueTwist, VRCFTParameter.TongueTwistLeft, VRCFTParameter.TongueTwistRight)),
            [FaceTrackingFeature.SoftPalateClose] = new(FaceTrackingFeature.SoftPalateClose, "Soft Palate Close", new VRCFTParameterSet(VRCFTParameterSetId.SingleSoftPalateClose, VRCFTParameter.SoftPalateClose)),
            [FaceTrackingFeature.ThroatSwallow] = new(FaceTrackingFeature.ThroatSwallow, "Throat Swallow", new VRCFTParameterSet(VRCFTParameterSetId.SingleThroatSwallow, VRCFTParameter.ThroatSwallow)),
            [FaceTrackingFeature.NeckFlex] = new(FaceTrackingFeature.NeckFlex, "Neck Flex", new VRCFTParameterSet(VRCFTParameterSetId.PerSideNeckFlex, VRCFTParameter.NeckFlexLeft, VRCFTParameter.NeckFlexRight)),
        };

        private FaceTrackingFeatureDefinition(FaceTrackingFeature feature, string displayName, params VRCFTParameterSet[] outputFormats)
        {
            Feature = feature;
            DisplayName = displayName;
            OutputFormats = Array.AsReadOnly((VRCFTParameterSet[])outputFormats.Clone());
        }

        internal FaceTrackingFeature Feature { get; }
        internal string DisplayName { get; }
        internal string TranslationKey => $"feature.{Feature}";
        internal IReadOnlyList<VRCFTParameterSet> OutputFormats { get; }

        internal VRCFTParameterSet GetOutputFormatOrDefault(VRCFTParameterSetId id)
        {
            foreach (VRCFTParameterSet outputFormat in OutputFormats)
            {
                if (outputFormat.Id == id)
                {
                    return outputFormat;
                }
            }

            return OutputFormats.Count == 0 ? null : OutputFormats[0];
        }

        internal int IndexOfOutputFormat(VRCFTParameterSetId id)
        {
            for (int i = 0; i < OutputFormats.Count; i++)
            {
                if (OutputFormats[i].Id == id)
                {
                    return i;
                }
            }

            return OutputFormats.Count == 0 ? -1 : 0;
        }
    }

    internal sealed class VRCFTParameterSet
    {
        internal VRCFTParameterSet(VRCFTParameterSetId id, params VRCFTParameter[] parameters)
        {
            Id = id;
            Parameters = Array.AsReadOnly((VRCFTParameter[])parameters.Clone());
        }

        internal VRCFTParameterSetId Id { get; }

        internal string DisplayName
        {
            get
            {
                return Id switch
                {
                    VRCFTParameterSetId.PerEyeGazeXY => "Per-Eye Gaze XY",
                    VRCFTParameterSetId.UnifiedGazeXY => "Unified Gaze XY",
                    VRCFTParameterSetId.VRChatNative => "VRChat Native",
                    VRCFTParameterSetId.PerEyeLid => "Per-Eye Lid",
                    VRCFTParameterSetId.UnifiedEyeLid => "Unified Eye Lid",
                    VRCFTParameterSetId.PerEyeSquint => "Per-Eye Squint",
                    VRCFTParameterSetId.UnifiedEyeSquint => "Unified Eye Squint",
                    VRCFTParameterSetId.PerEyePupilDiameter => "Per-Eye Pupil Diameter",
                    VRCFTParameterSetId.UnifiedPupilDilation => "Unified Pupil Dilation",
                    VRCFTParameterSetId.UnifiedPupilDiameter => "Unified Pupil Diameter",
                    VRCFTParameterSetId.DetailedPerSideBrowParts => "Detailed Per-Side Brow Parts",
                    VRCFTParameterSetId.PerSideBrowDownUnifiedInnerOuterUp => "Per-Side Brow Down + Unified Inner/Outer Up",
                    VRCFTParameterSetId.PerSideBrowDownUnifiedBrowUp => "Per-Side Brow Down + Unified Brow Up",
                    VRCFTParameterSetId.PerSideBrowExpression => "Per-Side Brow Expression",
                    VRCFTParameterSetId.UnifiedBrowExpression => "Unified Brow Expression",
                    VRCFTParameterSetId.PerSideNoseSneer => "Per-Side Nose Sneer",
                    VRCFTParameterSetId.UnifiedNoseSneer => "Unified Nose Sneer",
                    VRCFTParameterSetId.PerSideNasalDilation => "Per-Side Nasal Dilation",
                    VRCFTParameterSetId.PerSideNasalConstrict => "Per-Side Nasal Constrict",
                    VRCFTParameterSetId.PerSideCheekSquint => "Per-Side Cheek Squint",
                    VRCFTParameterSetId.UnifiedCheekSquint => "Unified Cheek Squint",
                    VRCFTParameterSetId.PerSideCheekPuffSuck => "Per-Side Cheek Puff-Suck",
                    VRCFTParameterSetId.UnifiedCheekPuffSuck => "Unified Cheek Puff-Suck",
                    VRCFTParameterSetId.SingleJawOpen => "Single Jaw Open",
                    VRCFTParameterSetId.SingleMouthClosed => "Single Mouth Closed",
                    VRCFTParameterSetId.JawXZAxes => "Jaw XZ Axes",
                    VRCFTParameterSetId.SingleJawClench => "Single Jaw Clench",
                    VRCFTParameterSetId.SingleMandibleRaise => "Single Mandible Raise",
                    VRCFTParameterSetId.DetailedLipSuck => "Detailed Lip Suck (Upper and Lower Per-Side)",
                    VRCFTParameterSetId.UpperLowerLipSuckUnifiedSides => "Upper and Lower Lip Suck (Unified Sides)",
                    VRCFTParameterSetId.UnifiedLipSuck => "Unified Lip Suck",
                    VRCFTParameterSetId.PerSideLipSuckCorner => "Per-Side Lip Suck Corner",
                    VRCFTParameterSetId.DetailedLipFunnel => "Detailed Lip Funnel (Upper and Lower Per-Side)",
                    VRCFTParameterSetId.UpperLowerLipFunnelUnifiedSides => "Upper and Lower Lip Funnel (Unified Sides)",
                    VRCFTParameterSetId.UnifiedLipFunnel => "Unified Lip Funnel",
                    VRCFTParameterSetId.DetailedLipPucker => "Detailed Lip Pucker (Upper and Lower Per-Side)",
                    VRCFTParameterSetId.UpperLowerLipPuckerUnifiedSides => "Upper and Lower Lip Pucker (Unified Sides)",
                    VRCFTParameterSetId.UnifiedLipPucker => "Unified Lip Pucker",
                    VRCFTParameterSetId.DetailedMouthOpen => "Detailed Mouth Open (Upper and Lower Per-Side)",
                    VRCFTParameterSetId.UpperLowerMouthOpenUnifiedSides => "Upper and Lower Mouth Open (Unified Sides)",
                    VRCFTParameterSetId.UnifiedMouthOpen => "Unified Mouth Open",
                    VRCFTParameterSetId.PerSideMouthUpperDeepen => "Per-Side Mouth Upper Deepen",
                    VRCFTParameterSetId.UpperLowerMouthX => "Upper/Lower Mouth X",
                    VRCFTParameterSetId.UnifiedMouthX => "Unified Mouth X",
                    VRCFTParameterSetId.DetailedMouthCorners => "Detailed Mouth Corners (Per-Side)",
                    VRCFTParameterSetId.PerSideMouthSmileSad => "Per-Side Mouth Smile and Sad",
                    VRCFTParameterSetId.PerSideSmileFrownStretch => "Per-Side Smile-Frown + Stretch",
                    VRCFTParameterSetId.PerSideSmileSad => "Per-Side Smile-Sad",
                    VRCFTParameterSetId.UnifiedSmileFrownPerSideStretch => "Unified Smile-Frown + Per-Side Stretch",
                    VRCFTParameterSetId.UnifiedSmileSad => "Unified Smile-Sad",
                    VRCFTParameterSetId.PerSideMouthDimple => "Per-Side Mouth Dimple",
                    VRCFTParameterSetId.UpperLowerMouthRaiser => "Upper and Lower Mouth Raiser",
                    VRCFTParameterSetId.PerSideMouthPress => "Per-Side Mouth Press",
                    VRCFTParameterSetId.PerSideMouthTightener => "Per-Side Mouth Tightener",
                    VRCFTParameterSetId.SingleTongueOut => "Single Tongue Out",
                    VRCFTParameterSetId.TongueXYAxes => "Tongue XY Axes",
                    VRCFTParameterSetId.SingleTongueRoll => "Single Tongue Roll",
                    VRCFTParameterSetId.SingleTongueArchY => "Single Tongue Arch Y",
                    VRCFTParameterSetId.SingleTongueShape => "Single Tongue Shape",
                    VRCFTParameterSetId.PerSideTongueTwist => "Per-Side Tongue Twist",
                    VRCFTParameterSetId.SingleSoftPalateClose => "Single Soft Palate Close",
                    VRCFTParameterSetId.SingleThroatSwallow => "Single Throat Swallow",
                    VRCFTParameterSetId.PerSideNeckFlex => "Per-Side Neck Flex",
                    _ => Id.ToString(),
                };
            }
        }

        internal IReadOnlyList<VRCFTParameter> Parameters { get; }
    }

    public enum VRCFTParameterSetId
    {
        None,
        PerEyeGazeXY,
        UnifiedGazeXY,
        VRChatNative,
        PerEyeLid,
        UnifiedEyeLid,
        PerEyeSquint,
        UnifiedEyeSquint,
        PerEyePupilDiameter,
        UnifiedPupilDilation,
        UnifiedPupilDiameter,
        DetailedPerSideBrowParts,
        PerSideBrowDownUnifiedInnerOuterUp,
        PerSideBrowDownUnifiedBrowUp,
        PerSideBrowExpression,
        UnifiedBrowExpression,
        PerSideNoseSneer,
        UnifiedNoseSneer,
        PerSideNasalDilation,
        PerSideNasalConstrict,
        PerSideCheekSquint,
        UnifiedCheekSquint,
        PerSideCheekPuffSuck,
        UnifiedCheekPuffSuck,
        SingleJawOpen,
        SingleMouthClosed,
        JawXZAxes,
        SingleJawClench,
        SingleMandibleRaise,
        DetailedLipSuck,
        UpperLowerLipSuckUnifiedSides,
        UnifiedLipSuck,
        PerSideLipSuckCorner,
        DetailedLipFunnel,
        UpperLowerLipFunnelUnifiedSides,
        UnifiedLipFunnel,
        DetailedLipPucker,
        UpperLowerLipPuckerUnifiedSides,
        UnifiedLipPucker,
        DetailedMouthOpen,
        UpperLowerMouthOpenUnifiedSides,
        UnifiedMouthOpen,
        PerSideMouthUpperDeepen,
        UpperLowerMouthX,
        UnifiedMouthX,
        DetailedMouthCorners,
        PerSideMouthSmileSad,
        PerSideSmileFrownStretch,
        PerSideSmileSad,
        UnifiedSmileFrownPerSideStretch,
        UnifiedSmileSad,
        PerSideMouthDimple,
        UpperLowerMouthRaiser,
        PerSideMouthPress,
        PerSideMouthTightener,
        SingleTongueOut,
        TongueXYAxes,
        SingleTongueRoll,
        SingleTongueArchY,
        SingleTongueShape,
        PerSideTongueTwist,
        SingleSoftPalateClose,
        SingleThroatSwallow,
        PerSideNeckFlex,
    }
}

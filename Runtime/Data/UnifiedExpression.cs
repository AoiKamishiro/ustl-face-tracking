namespace USTL.FaceTracking
{
    /// <summary>
    ///     https://docs.vrcft.io/docs/tutorial-avatars/tutorial-avatars-extras/unified-blendshapes
    /// </summary>
    public enum UnifiedExpression
    {
        // Eyes
        EyeLookOutRight,
        EyeLookInRight,
        EyeLookUpRight,
        EyeLookDownRight,
        EyeLookOutLeft,
        EyeLookInLeft,
        EyeLookUpLeft,
        EyeLookDownLeft,
        EyeClosedRight,
        EyeClosedLeft,
        EyeSquintRight,
        EyeSquintLeft,
        EyeWideRight,
        EyeWideLeft,
        EyeDilationRight,
        EyeDilationLeft,
        EyeConstrictRight,
        EyeConstrictLeft,

        // Brow
        BrowPinchRight,
        BrowPinchLeft,
        BrowLowererRight,
        BrowLowererLeft,
        BrowInnerUpRight,
        BrowInnerUpLeft,
        BrowOuterUpRight,
        BrowOuterUpLeft,

        // Nose
        NoseSneerRight,
        NoseSneerLeft,
        NasalDilationRight,
        NasalDilationLeft,
        NasalConstrictRight,
        NasalConstrictLeft,

        // Cheek
        CheekSquintRight,
        CheekSquintLeft,
        CheekPuffRight,
        CheekPuffLeft,
        CheekSuckRight,
        CheekSuckLeft,

        // Jaw
        JawOpen,
        MouthClosed,
        JawRight,
        JawLeft,
        JawForward,
        JawBackward,
        JawClench,
        JawMandibleRaise,

        // Lip
        LipSuckUpperRight,
        LipSuckUpperLeft,
        LipSuckLowerRight,
        LipSuckLowerLeft,
        LipSuckCornerRight,
        LipSuckCornerLeft,
        LipFunnelUpperRight,
        LipFunnelUpperLeft,
        LipFunnelLowerRight,
        LipFunnelLowerLeft,
        LipPuckerUpperRight,
        LipPuckerUpperLeft,
        LipPuckerLowerRight,
        LipPuckerLowerLeft,

        // Mouth
        MouthUpperUpRight,
        MouthUpperUpLeft,
        MouthLowerDownRight,
        MouthLowerDownLeft,
        MouthUpperDeepenRight,
        MouthUpperDeepenLeft,
        MouthUpperRight,
        MouthUpperLeft,
        MouthLowerRight,
        MouthLowerLeft,
        MouthCornerPullRight,
        MouthCornerPullLeft,
        MouthCornerSlantRight,
        MouthCornerSlantLeft,
        MouthFrownRight,
        MouthFrownLeft,
        MouthStretchRight,
        MouthStretchLeft,
        MouthDimpleRight,
        MouthDimpleLeft,
        MouthRaiserUpper,
        MouthRaiserLower,
        MouthPressRight,
        MouthPressLeft,
        MouthTightenerRight,
        MouthTightenerLeft,

        // Tongue
        TongueOut,
        TongueUp,
        TongueDown,
        TongueRight,
        TongueLeft,
        TongueRoll,
        TongueBendDown,
        TongueCurlUp,
        TongueSquish,
        TongueFlat,
        TongueTwistRight,
        TongueTwistLeft,

        // Neck
        SoftPalateClose,
        ThroatSwallow,
        NeckFlexRight,
        NeckFlexLeft,

        // None
        None = -1,
    }
}

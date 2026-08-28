namespace USTL.FaceTracking
{
    /// <summary>
    ///     https://docs.vrcft.io/docs/tutorial-avatars/tutorial-avatars-extras/parameters
    /// </summary>
    internal enum VRCFTParameter
    {
        // Eye Gaze
        EyeLeftX,
        EyeLeftY,
        EyeRightX,
        EyeRightY,

        // Eye Expression
        EyeLidRight,
        EyeLidLeft,
        EyeLid,

        EyeSquintRight,
        EyeSquintLeft,
        EyeSquint,

        PupilDilation,
        PupilDiameterRight,
        PupilDiameterLeft,
        PupilDiameter,

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
        CheekPuffSuckRight,
        CheekPuffSuckLeft,

        // Jaw
        JawOpen,
        MouthClosed,
        JawX,
        JawZ,
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
        MouthUpperX,
        MouthLowerX,
        MouthCornerPullRight,
        MouthCornerPullLeft,
        MouthCornerSlantRight,
        MouthCornerSlantLeft,
        MouthDimpleRight,
        MouthDimpleLeft,
        MouthFrownRight,
        MouthFrownLeft,
        MouthStretchRight,
        MouthStretchLeft,
        MouthRaiserUpper,
        MouthRaiserLower,
        MouthPressRight,
        MouthPressLeft,
        MouthTightenerRight,
        MouthTightenerLeft,

        // Tongue
        TongueOut,
        TongueX,
        TongueY,
        TongueRoll,
        TongueArchY,
        TongueShape,
        TongueTwistRight,
        TongueTwistLeft,

        // Neck
        SoftPalateClose,
        ThroatSwallow,
        NeckFlexRight,
        NeckFlexLeft,

        // Simplified Eye
        EyeX,
        EyeY,

        // Simplified Brow
        BrowDownRight,
        BrowDownLeft,
        BrowOuterUp,
        BrowInnerUp,
        BrowUp,
        BrowExpressionRight,
        BrowExpressionLeft,
        BrowExpression,

        // Simplified Mouth
        MouthX,
        MouthUpperUp,
        MouthLowerDown,
        MouthOpen,
        MouthSmileRight,
        MouthSmileLeft,
        MouthSadRight,
        MouthSadLeft,
        SmileFrownRight,
        SmileFrownLeft,
        SmileFrown,
        SmileSadRight,
        SmileSadLeft,
        SmileSad,

        // Simplified Lip
        LipSuckUpper,
        LipSuckLower,
        LipSuck,
        LipFunnelUpper,
        LipFunnelLower,
        LipFunnel,
        LipPuckerUpper,
        LipPuckerLower,
        LipPucker,

        // Simplified Nose & Cheek
        NoseSneer,
        CheekSquint,
        CheekPuffSuck,

        // None
        None = -1,
    }
}

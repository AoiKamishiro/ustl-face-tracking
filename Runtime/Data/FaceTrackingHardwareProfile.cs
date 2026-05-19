using System;

namespace USTL.FaceTracking
{
    [Flags]
    internal enum FaceTrackingHardwareProfile
    {
        None = 0,
        MetaQuestPro = 1 << 0,
        ViveProEye = 1 << 1,
        ViveFacialTracker = 1 << 2,
        ViveFocus3EyeTrackingAddon = 1 << 3,
        ViveFocus3FacialTrackerAddon = 1 << 4,
        ViveXrEliteFullFaceTracker = 1 << 5,
        ViveFocusVisionFaceTrackingAddon = 1 << 6,
        VarjoAeroXr3Vr3 = 1 << 7,
        PimaxDroolonPi1 = 1 << 8,
        PimaxSuperCrystal = 1 << 9,
        Psvr2 = 1 << 10,
        EyeTrackVR = 1 << 11,
        ProjectBabble = 1 << 12,
        Pico4ProEnterprise = 1 << 13,
        ArkitIos = 1 << 14,
        HpReverbG2Omnicept = 1 << 15,
        SamsungGalaxyXr = 1 << 16,
        AndroidMeowFace = 1 << 17,
    }
}

using nadena.dev.ndmf;
using USTL.FaceTracking.Editor;

[assembly: ExportsPlugin(typeof(USTLFaceTrackingPlugin))]

namespace USTL.FaceTracking.Editor
{
    internal sealed class USTLFaceTrackingPlugin : Plugin<USTLFaceTrackingPlugin>
    {
        public override string QualifiedName => "jp.co.u-stella.facetracking";
        public override string DisplayName => "U-Stella FaceTracking";

        protected override void Configure()
        {
            InPhase(BuildPhase.Generating).Run(GenerateFaceTrackingPass.Instance);
        }
    }
}

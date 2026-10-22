
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace ChuChuGimmicks.MinamoChair.Runtime
{
    public partial class Manager
    {
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public static class FieldNames
        {
            public const string Station = nameof(_station);
            public const string FrameEventReceiver = nameof(_frameEventReceiver);

            public const string EnterPoint = nameof(_enterPoint);
            public const string ExitPoint = nameof(_exitPoint);
            public const string Model = nameof(_model);
            public const string Guide = nameof(_guide);
            public const string GuideImage = nameof(_guideImage);
            public const string GuideSprites = nameof(_guideSprites);
            public const string DisabledOverlay = nameof(_disabledOverlay);

            public const string AutoAdjustmentMinHeight = nameof(_autoAdjustmentMinHeight);
            public const string AutoAdjustmentMaxHeight = nameof(_autoAdjustmentMaxHeight);

            public const string ManualAdjustmentMinHeight = nameof(_manualAdjustmentMinHeight);
            public const string ManualAdjustmentMaxHeight = nameof(_manualAdjustmentMaxHeight);

            public const string ModelAdjustmentMode = nameof(_modelAdjustmentMode);
            public const string ModelSurfaceHeight = nameof(_modelSurfaceHeight);
        }
#endif
    }
}

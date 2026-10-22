namespace ChuChuGimmicks.MinamoChair.Runtime
{
    public partial class FrameEventReceiver
    {
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        public static class FieldNames
        {
            public const string Manager = nameof(_manager);
        }
#endif
    }
}

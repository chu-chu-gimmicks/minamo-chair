
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace ChuChuGimmicks.MinamoChair.Runtime
{
    public partial class Manager
    {
        private void Log(string className, string methodName, string message = "")
        {
            string coloredProductName = $"<color=#FFBF00>{nameof(MinamoChair)}</color>";
            string coloredClassName = $"<color=#FFCF3F>{className}</color>";
            string coloredMethodName = $"<color=#FFDF7F>{methodName}</color>";
            Debug.Log($"[{coloredProductName}/{coloredClassName}/{coloredMethodName}] {message}");
        }
    }
}

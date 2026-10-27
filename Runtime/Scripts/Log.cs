using UdonSharp;
using UnityEngine;

namespace ChuChuGimmicks.MinamoChair.Runtime
{
    public class Log : UdonSharpBehaviour
    {
        public static void Info(string className, string message = "")
        {
            string coloredProductName = $"<color=#FFBF00>{nameof(MinamoChair)}</color>";
            string coloredClassName = $"<color=#FFCF3F>{className}</color>";
            Debug.Log($"[{coloredProductName}/{coloredClassName}] {message}");
        }


        //public static void Warning(string className, string message) { }
        //public static void Error(string className, string message) { }
        //public static void Assert(bool condition, string className, string message) { }
    }
}

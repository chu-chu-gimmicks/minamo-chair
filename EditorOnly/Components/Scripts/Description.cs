using UnityEngine;

namespace ChuChuGimmicks.MinamoChair.EditorOnly
{
    public class Description : MonoBehaviour, VRC.SDKBase.IEditorOnly
    {
        public static class FieldName
        {
            public const string Description = nameof(_description);
        }




        [SerializeField]
        [TextArea]
        private string _description;
    }
}

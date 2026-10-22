using UdonSharp;
using UnityEngine;
using VRC.Udon.Common;

namespace ChuChuGimmicks.MinamoChair.Runtime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public partial class FrameEventReceiver : UdonSharpBehaviour
    {
        [SerializeField] private Manager _manager;




        public override void InputMoveHorizontal(float value, UdonInputEventArgs args)
        {
            _manager._InputMoveHorizontal(value, args);
        }


        public override void InputMoveVertical(float value, UdonInputEventArgs args)
        {
            _manager._InputMoveVertical(value, args);
        }


        public override void InputLookHorizontal(float value, UdonInputEventArgs args)
        {
            _manager._InputLookHorizontal(value, args);
        }


        public override void InputLookVertical(float value, UdonInputEventArgs args)
        {
            _manager._InputLookVertical(value, args);
        }


        public override void InputJump(bool value, UdonInputEventArgs args)
        {
            _manager._InputJump(value, args);
        }


        private void LateUpdate()
        {
            _manager._LateUpdate();
        }
    }
}

using UnityEngine;
using System.Collections;
using System;

namespace ACT
{
    [ActActionClass("动作", "震屏", "")]
    [Serializable]
    public class ActShake : ActItem
    {
        [SerializeField] public Int32 ShakeStrength;

        protected override bool Trigger()
        {
            GTCameraManager.Instance.CameraCtrl.PlayShake(ShakeStrength, Duration);
            return true;
        }

        protected override void Release()
        {
            GTCameraManager.Instance.CameraCtrl.StopShake();
        }

        public override ActItem Clone()
        {
            return this;
        }
    }
}
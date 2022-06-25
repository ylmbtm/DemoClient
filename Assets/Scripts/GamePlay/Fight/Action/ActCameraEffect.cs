using UnityEngine;
using System.Collections;
using System;

namespace ACT
{
    [ActActionClass("动作", "相机特效", "")]
    public class ActCameraEffect : ActItem
    {
        public Int32 CameraEffectID;

        protected override bool Trigger()
        {
            GTCameraManager.Instance.CreateCameraEffect(CameraEffectID, Release);
            return base.Trigger();
        }

        public override ActItem Clone()
        {
            return this;
        }
    }
}
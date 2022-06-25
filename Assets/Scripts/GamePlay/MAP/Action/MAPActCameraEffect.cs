using UnityEngine;
using System.Collections;
using CFG;

namespace MAP
{
    [MAPActionClass("动画/相机动画", false)]
    public class MAPActCameraEffect : MAPAction
    {
        [MAPFieldAttri] public int CameraEffectID;

        public override void Trigger()
        {
            base.Trigger();
            GTCameraManager.Instance.CreateCameraEffect(CameraEffectID, Release);
        }

        public override void Import(DCFG cfg)
        {
            MapActCameraEffect data = cfg as MapActCameraEffect;
            this.ID                  = data.ID;
            this.CameraEffectID      = data.CameraEffectID;
        }

        public override DCFG Export()
        {
            MapActCameraEffect data = new MapActCameraEffect();
            data.ID                  = this.ID;
            data.CameraEffectID      = this.CameraEffectID;
            return data;
        }
    }
}
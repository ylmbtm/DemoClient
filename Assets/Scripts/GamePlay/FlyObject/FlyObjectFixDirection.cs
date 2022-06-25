using UnityEngine;
using System.Collections;

namespace FLY
{
    public class FlyObjectFixDirection : FlyObject
    {
        protected override void Startup()
        {
            base.Startup();
            this.Pos   = this.CasterBind != null ? CasterBind.position : NowPos;
            this.Euler = this.NowEulerAngles;
        }

        protected override void Execute()
        {
            NowSpeed += Time.deltaTime * FlyingAcee;
            Vector3 dir = CacheTransform.forward;
            Vector3 move = dir * Time.deltaTime * NowSpeed;
            CacheTransform.position += move;

            CheckTargetObjects();
        }
    }
}

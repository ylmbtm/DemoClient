using UnityEngine;
using System.Collections;

namespace FLY
{
    public class FlyObjectFixTargetPos : FlyObject
    {
        protected override void Startup()
        {
            this.Pos   = CasterBind != null ? CasterBind.position : NowPos;
            this.Euler = this.NowEulerAngles;
        }

        protected override void Execute()
        {
            NowSpeed += Time.deltaTime * FlyingAcee;
            CacheTransform.LookAt(TarPos);
            float distance = Vector3.Distance(CacheTransform.position, TarPos);
            if (FlyingVAngle != 0 || FlyingHAngle != 0)
            {
                float percent = Mathf.Min(1, distance / 10);
                float vAngle = percent * FlyingVAngle;
                float hAngle = percent * FlyingHAngle;
                CacheTransform.rotation *= Quaternion.Euler(Mathf.Clamp(-vAngle, -60, 60), Mathf.Clamp(-hAngle, -60, 60), 0);
            }
            CacheTransform.position += CacheTransform.forward * NowSpeed * Time.deltaTime;
        }
    }
}
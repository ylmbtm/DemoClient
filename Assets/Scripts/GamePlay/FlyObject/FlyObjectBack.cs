using UnityEngine;
using System.Collections;

namespace FLY
{
    public class FlyObjectBack : FlyObject
    {
        public Vector3 StartPos
        {
            get; private set;
        }

        public bool    StartBack
        {
            get; private set;
        }

        protected override void Startup()
        {
            base.Startup();
            this.Pos      = this.CasterBind != null ? CasterBind.position : NowPos;
            this.StartPos = this.Pos;
            this.Euler    = this.NowEulerAngles;
        }

        protected override void Execute()
        {
            if (StartBack == false)
            {
                NowSpeed += Time.deltaTime * FlyingAcee;
                Vector3 dir = CacheTransform.forward;
                Vector3 move = dir * Time.deltaTime * NowSpeed;
                CacheTransform.position += move;
                if (GTTools.GetHorizontalDistance(CacheTransform.position, StartPos) >= FlyingBackDistance)
                {
                    Back();
                }
            }
            else
            {
                NowSpeed += Time.deltaTime * FlyingAcee;
                CacheTransform.LookAt(CasterBind.position);
                Vector3 dir = CacheTransform.forward;
                Vector3 move = dir * Time.deltaTime * NowSpeed;
                CacheTransform.position += move;
            }
        }

        public void Back()
        {
            StartBack = true;
            CacheTransform.forward = -CacheTransform.forward;
        }
    }
}
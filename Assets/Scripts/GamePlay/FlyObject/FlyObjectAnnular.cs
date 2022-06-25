using UnityEngine;
using System.Collections;

namespace FLY
{
    public class FlyObjectAnnular : FlyObject
    {
        protected override void Startup()
        {
            base.Startup();
            Vector3 nowPos = NowPos;
            if (CasterBind != null)
            {
                nowPos.y = CasterBind.position.y;
                this.Pos = nowPos;
            }
            else
            {
                this.Pos = nowPos;
            }
            this.Euler = this.NowEulerAngles;
        }

        protected override void Execute()
        {


            if (CasterBind == null)
            {
                Vector3 axis = new Vector3(0, this.NowPos.y, 0);
                this.CacheTransform.Rotate(axis, Time.deltaTime * NowSpeed, Space.Self);
            }
            else
            {
                Vector3 dir = this.CacheTransform.transform.position - CasterActor.transform.position;
                Quaternion quar = Quaternion.Euler(0, Time.deltaTime * NowSpeed, 0);
                Vector3 d = quar * dir;
                d.Normalize();
                this.CacheTransform.transform.position = CasterActor.transform.position + d * 5;
            }
        }
    }
}
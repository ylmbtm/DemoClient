using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ACT;

namespace FLY
{
    public class FlyObjectLink : FlyObject
    {
        //public    F3DBeam                 Line          { get; private set; }
        //public    float                   BoundDistance { get; private set; }

        protected override void Startup()
        {
            base.Startup();
            //this.Line = GetComponentInChildren<F3DBeam>();
            this.Execute();
            //this.BoundDistance = DB.RangeParams[0];
        }

        protected override void Execute()
        {
            //if (Line == null)
            //{
            //    return;
            //}
            //if (CasterActor == null || TargetActor == null)
            //{
            //    return;
            //}
            //Vector3 casterBindPos = CasterBind == null ? CasterActor.Pos : CasterBind.position;
            //Vector3 targetBindPos = TargetBind == null ? TargetActor.Pos : TargetBind.position;
            //Line.transform.position = casterBindPos;
            //Line.transform.LookAt(targetBindPos);
            //Line.MaxBeamLength = Vector3.Distance(Line.transform.position, targetBindPos);
        }
    }
}
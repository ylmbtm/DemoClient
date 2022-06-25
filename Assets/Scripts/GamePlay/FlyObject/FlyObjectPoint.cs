using UnityEngine;
using System.Collections;
using System;
using ACT;
using System.Collections.Generic;

namespace FLY
{
    public class FlyObjectPoint : FlyObject
    {
        protected override void Startup()
        {
            base.Startup();
            this.Pos             = this.NowPos;
            this.Euler           = this.NowEulerAngles;
        }
    }
}

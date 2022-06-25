using UnityEngine;
using System.Collections;
using System;

namespace ACT
{
    [ActResultClass("动作", "子弹", "")]
    public class ActFlyObject : ActResult
    {
       public int                ID           = 0;
       public float              Angle        = 0;
    }
}
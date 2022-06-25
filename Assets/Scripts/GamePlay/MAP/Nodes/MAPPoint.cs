using UnityEngine;
using System.Collections;
using CFG;

namespace MAP
{
    public class MAPPoint : MAPElement
    {
        public override DCFG Export()
        {
            DTPoint data       = new DTPoint();
            data.Pos            = Pos;
            data.EulerAngles    = EulerAngles;
            return data;
        }

        public override void Import(DCFG cfg)
        {
            DTPoint data       = cfg as DTPoint;
            this.Pos            = data.Pos;
            this.EulerAngles    = data.EulerAngles;
        }
    }
}
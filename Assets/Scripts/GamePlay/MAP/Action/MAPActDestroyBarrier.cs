using UnityEngine;
using System.Collections;
using CFG;

namespace MAP
{
    [MAPActionClass("销毁/屏障", false)]
    public class MAPActDestroyBarrier : MAPAction
    {
        [MAPFieldAttri] public int BarrierID;

        public override void Trigger()
        {
            base.Trigger();
            this.Map.ReleaseElement<MAPBarrier>(BarrierID);
            this.Release();
        }

        public override void Import(DCFG cfg)
        {
            MapActDestroyBarrier data = cfg as MapActDestroyBarrier;
            this.ID                      = data.ID;
            this.BarrierID               = data.BarrierID;
        }

        public override DCFG Export()
        {
            MapActDestroyBarrier data = new MapActDestroyBarrier();
            data.ID                      = this.ID;
            data.BarrierID               = this.BarrierID;
            return data;
        }
    }
}
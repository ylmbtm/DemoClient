using UnityEngine;
using System.Collections;
using CFG;

namespace MAP
{
    [MAPActionClass("触发/屏障", false)]
    public class MAPActTriggerBarrier : MAPAction
    {
        [MAPFieldAttri] public int BarrierID;

        public override void Trigger()
        {
            base.Trigger();
            this.Map.TriggerElement<MAPBarrier>(BarrierID);
            this.Release();
        }

        public override void Import(DCFG cfg)
        {
            MapActTriggerBarrier data = cfg as MapActTriggerBarrier;
            this.ID                      = data.ID;
            this.BarrierID               = data.BarrierID;
        }

        public override DCFG Export()
        {
            MapActTriggerBarrier data = new MapActTriggerBarrier();
            data.ID                      = this.ID;
            data.BarrierID               = this.BarrierID;
            return data;
        }
    }
}
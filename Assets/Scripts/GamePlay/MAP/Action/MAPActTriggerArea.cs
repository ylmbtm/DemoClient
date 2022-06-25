using UnityEngine;
using System.Collections;
using CFG;

namespace MAP
{
    [MAPActionClass("触发/区域", false)]
    public class MAPActTriggerArea : MAPAction
    {
        [MAPFieldAttri] public int AreaID;

        public override void Trigger()
        {
            base.Trigger();
            this.Map.TriggerElement<MAPArea>(AreaID);
            this.Release();
        }

        public override void Import(DCFG cfg)
        {
            MapActTriggerArea data = cfg as MapActTriggerArea;
            this.ID                   = data.ID;
            this.AreaID               = data.AreaID;
        }

        public override DCFG Export()
        {
            MapActTriggerArea data = new MapActTriggerArea();
            data.ID                   = this.ID;
            data.AreaID               = this.AreaID;
            return data;
        }
    }
}
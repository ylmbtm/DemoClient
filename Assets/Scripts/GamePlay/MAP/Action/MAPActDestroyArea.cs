using UnityEngine;
using System.Collections;
using CFG;

namespace MAP
{
    [MAPActionClass("销毁/区域", false)]
    public class MAPActDestroyArea : MAPAction
    {
        [MAPFieldAttri] public int AreaID;

        public override void Trigger()
        {
            base.Trigger();
            this.Map.ReleaseElement<MAPArea>(AreaID);
            this.Release();
        }

        public override void Import(DCFG cfg)
        {
            MapActDestroyArea data = cfg as MapActDestroyArea;
            this.ID                   = data.ID;
            this.AreaID               = data.AreaID;
        }

        public override DCFG Export()
        {
            MapActDestroyArea data = new MapActDestroyArea();
            data.ID                   = this.ID;
            data.AreaID               = this.AreaID;
            return data;
        }
    }
}
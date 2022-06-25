using UnityEngine;
using System.Collections;
using CFG;

namespace MAP
{
    [MAPActionClass("销毁/所有物体", false)]
    public class MAPActDestroyAllObjects : MAPAction
    {
        public override void Trigger()
        {
            base.Trigger();
            this.Release();
        }

        public override void Import(DCFG cfg)
        {
            MapActDestroyAllObjects data = cfg as MapActDestroyAllObjects;
            this.ID                      = data.ID;
        }

        public override DCFG Export()
        {
            MapActDestroyAllObjects data = new MapActDestroyAllObjects();
            data.ID                      = this.ID;
            return data;
        }
    }
}

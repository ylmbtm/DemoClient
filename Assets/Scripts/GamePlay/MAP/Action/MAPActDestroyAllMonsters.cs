using UnityEngine;
using System.Collections;
using CFG;

namespace MAP
{
    [MAPActionClass("销毁/所有怪物", false)]
    public class MAPActDestroyAllMonsters : MAPAction
    {
        public override void Trigger()
        {
            base.Trigger();
            this.Release();
        }

        public override void Import(DCFG cfg)
        {
            MapActDestroyAllMonsters data = cfg as MapActDestroyAllMonsters;
            this.ID                          = data.ID;
        }

        public override DCFG Export()
        {
            MapActDestroyAllMonsters data = new MapActDestroyAllMonsters();
            data.ID                     = this.ID;
            return data;
        }
    }
}
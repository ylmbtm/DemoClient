using UnityEngine;
using System.Collections;
using CFG;

namespace MAP
{
    [MAPActionClass("触发/传送", false)]
    public class MAPActTriggerTeleport : MAPAction
    {
        [MAPFieldAttri] public int TeleportID;

        public override void Trigger()
        {
            base.Trigger();
            GTWorld.Instance.Teleport(TeleportID);
            this.Release();
        }

        public override void Import(DCFG cfg)
        {
            MapActTriggerTeleport data = cfg as MapActTriggerTeleport;
            this.ID                       = data.ID;
            this.TeleportID               = data.TeleportID;
        }

        public override DCFG Export()
        {
            MapActTriggerTeleport data = new MapActTriggerTeleport();
            data.ID                       = this.ID;
            data.TeleportID               = this.TeleportID;
            return data;
        }
    }
}
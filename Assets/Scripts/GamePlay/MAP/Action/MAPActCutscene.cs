using UnityEngine;
using System.Collections;
using System;
using CFG;

namespace MAP
{
    [MAPActionClass("动画/剧情动画", false)]
    public class MAPActCutscene : MAPAction
    {
        [MAPFieldAttri] public Int32 CutsceneID;

        public override void Trigger()
        {
            base.Trigger();
            GTWorld.Instance.Plot.Trigger(CutsceneID, Release);
        }

        public override void Import(DCFG cfg)
        {
            MapActCutscene data = cfg as MapActCutscene;
            this.ID              = data.ID;
            this.CutsceneID      = data.CutsceneID;
        }

        public override DCFG Export()
        {
            MapActCutscene data = new MapActCutscene();
            data.ID              = this.ID;
            data.CutsceneID      = this.CutsceneID;
            return data;
        }
    }
}
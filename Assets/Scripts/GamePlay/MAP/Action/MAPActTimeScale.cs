using UnityEngine;
using System.Collections;
using CFG;

namespace MAP
{
    [MAPActionClass("其他/时间流逝", false)]
    public class MAPActTimeScale : MAPAction
    {
        [MAPFieldAttri] public float Rate;
        [MAPFieldAttri] public float Duration;

        public override void Trigger()
        {
            base.Trigger();
            GTTimerManager.Instance.AddTimer(Duration, Release);
        }

        public override DCFG Export()
        {
            MapActTimeScale data = new MapActTimeScale();
            data.ID              = ID;
            data.Rate            = Rate;
            data.Duration        = Duration;
            return data;
        }

        public override void Import(DCFG cfg)
        {
            MapActTimeScale data = cfg as MapActTimeScale;
            this.ID              = data.ID;
            this.Rate            = data.Rate;
            this.Duration        = data.Duration;
        }
    }
}
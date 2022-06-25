using UnityEngine;
using System.Collections;
using CFG;
using System;

namespace MAP
{
    [MAPActionClass("其他/延时事件", false)]
    public class MAPActDelay : MAPAction
    {
        [MAPFieldAttri] public float Delay;
        [NonSerialized] public int   TimerKey;

        public override void Trigger()
        {
            base.Trigger();
            this.TimerKey = GTTimerManager.Instance.AddTimer(Delay, Release).key;
        }

        public override void Release()
        {
            GTTimerManager.Instance.DelTimer(TimerKey);
            base.Release();
        }

        public override void Import(DCFG cfg)
        {
            MapActDelay data = cfg as MapActDelay;
            this.ID           = data.ID;
            this.Delay        = data.Delay;
        }

        public override DCFG Export()
        {
            MapActDelay data = new MapActDelay();
            data.ID           = this.ID;
            data.Delay        = this.Delay;
            return data;
        }
    }
}

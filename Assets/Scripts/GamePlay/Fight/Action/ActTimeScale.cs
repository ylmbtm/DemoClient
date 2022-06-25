using UnityEngine;
using System.Collections;
using System;

namespace ACT
{
    [ActActionClass("动作", "时间流逝", "")]
    public class ActTimeScale : ActItem
    {
         public float Value;

        protected override bool Trigger()
        {
            Time.timeScale = Value;
            return true;
        }

        protected override void Release()
        {
            Time.timeScale = 1;
        }

        protected override void End()
        {
            Time.timeScale = 1;
        }

        public override ActItem Clone()
        {
            return this;
        }
    }
}

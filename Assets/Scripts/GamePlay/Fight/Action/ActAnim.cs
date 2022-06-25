using UnityEngine;
using System.Collections;
using System;

namespace ACT
{
    [ActActionClass("动作", "动画", "")]
    public class ActAnim : ActItem
    {
        public string               AnimName  = string.Empty;
        public bool                 Looped   = false;
        public float                Speed     = 1;

        protected override bool Trigger()
        {
            this.Skill.CasterActor.PlayAnim(AnimName, null, Looped, Speed);
            return true;
        }

        public override ActItem Clone()
        {
            return this;
        }
    }
}
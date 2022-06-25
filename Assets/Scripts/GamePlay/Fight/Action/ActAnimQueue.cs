using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace ACT
{
    [ActActionClass("动作", "动画队列", "")]
    public class ActAnimQueue : ActItem
    {
        public List<string> Anims = new List<string>();
        protected override bool Trigger()
        {
            //this.Skill.CasterActor.Anim.PlaySkillQueue(Skill.LayerType, Anims);
            return true;
        }
        public override ActItem Clone()
        {
            return this;
        }
    }
}
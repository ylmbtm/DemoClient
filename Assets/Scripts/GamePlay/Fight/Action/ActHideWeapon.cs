using UnityEngine;
using System.Collections;
using System;

namespace ACT
{
    [ActActionClass("动作", "隐藏武器", "")]
    [Serializable]
    public class ActHideWeapon : ActItem
    {
        protected override bool Trigger()
        {
            this.Skill.CasterActor.Get<ActorAvator>().SetWeaponActive(false);
            return true;
        }

        protected override void End()
        {
            this.Skill.CasterActor.Get<ActorAvator>().SetWeaponActive(true);
        }

        protected override void Release()
        {
            this.Skill.CasterActor.Get<ActorAvator>().SetWeaponActive(true);
        }

        public override ActItem Clone()
        {
            return this;
        }
    }
}
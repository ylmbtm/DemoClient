using UnityEngine;
using System.Collections;
using ECT;
using System;

namespace ACT
{
    [ActActionClass("动作", "特效", "")]
    public class ActEffect : ActItem
    {
        public int          ID     = 0;
        public EBind        Bind   = EBind.Foot;
        public Vector3      Offset = Vector3.zero;
        public Vector3      Euler  = Vector3.zero;
        public float        Size   = 1;
        public bool         Retain = false;
        private ulong       BindEffectGuid = 0;

        protected override bool Trigger()
        {
            Transform bindTransform = this.Skill.CasterActor.Get<ActorAvator>().GetBindTransform(Bind);
            ActorEffect effectMgr = this.Skill.CasterActor.Get<ActorEffect>();
            BindEffectGuid = effectMgr.AddEffect(ID, bindTransform, Duration, Retain, Offset, Euler, Vector3.one * Size);
            return true;
        }

        protected override void Release()
        {
           // if (BindEffectGuid == 0)
           // {
           //     return;
           // }

           // ActorEffect effectMgr = this.Skill.CasterActor.Get<ActorEffect>();
           // if(effectMgr == null)
           // {
           //     return;
           // }

           // effectMgr.DelEffect(BindEffectGuid);

           // BindEffectGuid = 0; 
        }

        protected override void End()
        {
            BindEffectGuid = 0;
        }

        public override ActItem Clone()
        {
            return this;
        }
    }
}
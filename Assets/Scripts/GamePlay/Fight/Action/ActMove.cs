using UnityEngine;
using System.Collections;
using System;
using DG.Tweening;

namespace ACT
{
    [ActActionClass("动作", "位移", "")]
    public class ActMove : ActItem
    {
        public enum MoveType
        {
            MOVE_TODIRECTION = 0,
            MOVE_TOTARGET    = 1,
            MOVE_BACK        = 2,
        }

        public MoveType Type   = MoveType.MOVE_TODIRECTION;
        public float    MaxDis = 10;
        public override ActItem Clone()
        {
            return this;
        }
        protected override bool Trigger()
        {
            base.Trigger();
            Vector3 rayStartPoint = Skill.CasterActor.Pos + new Vector3(0, Skill.CasterActor.Height / 2, 0);
            switch (Type)
            {
                case MoveType.MOVE_TODIRECTION:
                    {
                        Vector3 endValue = Skill.CasterActor.Pos + Skill.CasterActor.Dir * MaxDis;
                        Skill.CasterActor.CheckMoveEndPointInFront(ref endValue, MaxDis);
                        Skill.CasterActor.CacheTransform.DOMove(endValue, Duration);
                    }
                    break;
                case MoveType.MOVE_TOTARGET:
                    {
                        if (Skill.TargetActor == null)
                        {
                            Vector3 endValue = Skill.CasterActor.Pos + Skill.CasterActor.Dir * MaxDis;
                            Skill.CasterActor.CheckMoveEndPointInFront(ref endValue, MaxDis);
                            Skill.CasterActor.CacheTransform.DOMove(endValue, Duration);
                        }
                        else
                        {
                            Vector3 targetPos = Skill.TargetActor.Pos;
                            Vector3 casterPos = Skill.CasterActor.Pos;
                            Vector3 dir = targetPos - casterPos;
                            dir.y = 0;
                            dir.Normalize();
                            Vector3 endValue = Skill.TargetActor.Pos - dir * (Skill.CasterActor.Radius + Skill.TargetActor.Radius);
                            Skill.CasterActor.CheckMoveEndPointInFront(ref endValue, MaxDis);
                            Skill.CasterActor.CacheTransform.DOMove(endValue, Duration);
                        }
                    }
                    break;
                case MoveType.MOVE_BACK:
                    {
                        Vector3 endValue = Skill.CasterActor.Pos - Skill.CasterActor.Dir * MaxDis;
                        Skill.CasterActor.CheckMoveEndPointInBack(ref endValue, MaxDis);
                        Skill.CasterActor.CacheTransform.DOMove(endValue, Duration);
                    }
                    break;
            }

            return true;
        }
    }
}
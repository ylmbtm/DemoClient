using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CFG;
using System;

namespace MAP
{
    [MAPActionClass("组合/串行事件", true)]
    public class MAPActSequence : MAPActComposite
    {
        [NonSerialized] public Int32     CurrIndex  = 0;

        public override void Trigger()
        {
            base.Trigger();
            this.CurrIndex = 0;
        }

        public override DCFG Export()
        {
            MapActSequence data = new MapActSequence();
            data.ID             = this.ID;
            for (int i = 0; i < Actions.Count; i++)
            {
                data.Actions.Add(Actions[i].ID);
            }
            return data;
        }

        public override void Import(DCFG cfg)
        {
            MapActSequence data = cfg as MapActSequence;
            this.ID             = data.ID;
            MAPGroupAction group = Map.GetGroup<MAPGroupAction>();
            for (int i = 0; i < data.Actions.Count; i++)
            {
                MAPAction e = group.GetElement(data.Actions[i]);
                if (e != null)
                {
                    Actions.Add(e);
                }
            }
        }

        public override void Execute()
        {
            for (; CurrIndex < Actions.Count; CurrIndex++)
            {
                MAPAction child = Actions[CurrIndex];
                if (child.State == ENTS.INITIAL)
                {
                    child.Trigger();
                }
                switch (child.State)
                {
                    case ENTS.RUNNING:
                        return;
                    case ENTS.SUCCESS:
                        continue;
                    case ENTS.FAILURE:
                        Release();
                        return;
                }
            }
            Release();
        }

        public override void Release()
        {
            base.Release();
        }
    }
}
using UnityEngine;
using System.Collections;
using CFG;
using System;
using System.Collections.Generic;

namespace MAP
{
    [MAPActionClass("组合/循环节点", false)]
    public class MAPActLoop : MAPActComposite
    {
        [MAPFieldAttri] public Int32     Loops = 1;

        [NonSerialized] public MAPAction CurrAction;
        [NonSerialized] public Int32     CurrTimes;

        public override void Trigger()
        {
            base.Trigger();
            this.CurrAction = Actions.Count > 0 ? Actions[0] : null;
        }

        public override DCFG Export()
        {
            MapActLoop data = new MapActLoop();
            data.ID                 = this.ID;
            data.Loops              = this.Loops;
            for (int i = 0; i < Actions.Count; i++)
            {
                data.Actions.Add(Actions[i].ID);
            }
            return data;
        }

        public override void Import(DCFG cfg)
        {
            MapActLoop data = cfg as MapActLoop;
            this.ID                 = data.ID;
            this.Loops              = data.Loops;
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
            if (CurrAction == null)
            {
                return;
            }
            if (CurrTimes >= Loops && Loops > 0)
            {
                return;
            }
            if (CurrAction.State == ENTS.INITIAL && CurrTimes == 0)
            {
                CurrAction.Trigger();
            }
            if (CurrAction.State == ENTS.SUCCESS)
            {
                this.CurrTimes++;
                if (Loops > 0 && CurrTimes >= Loops)
                {
                    Release();
                }
                else
                {
                    CurrAction.Trigger();
                }
            }
        }

        public override void Release()
        {
            base.Release();
            this.CurrAction = null;
        }
    }
}
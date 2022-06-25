using UnityEngine;
using System.Collections;
using CFG;
using System.Collections.Generic;
using System;

namespace MAP
{
    [MAPActionClass("组合/随机事件", true)]
    public class MAPActRandomce : MAPActComposite
    {
        [NonSerialized] public MAPAction CurrAction = null;

        public override void Trigger()
        {
            base.Trigger();
            int randomIndex = UnityEngine.Random.Range(0, Actions.Count);
            this.CurrAction = Actions.Count > randomIndex ? Actions[randomIndex] : null;
        }

        public override DCFG Export()
        {
            MapActRandomce data = new MapActRandomce();
            data.ID             = this.ID;
            for (int i = 0; i < Actions.Count; i++)
            {
                data.Actions.Add(Actions[i].ID);
            }
            return data;
        }

        public override void Import(DCFG cfg)
        {
            MapActRandomce data  = cfg as MapActRandomce;
            this.ID              = data.ID;
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
            if (CurrAction.State == ENTS.INITIAL)
            {
                CurrAction.Trigger();
            }
            if (CurrAction.State == ENTS.SUCCESS)
            {
                Release();
            }
        }

        public override void Release()
        {
            base.Release();
            this.CurrAction = null;
        }
    }
}
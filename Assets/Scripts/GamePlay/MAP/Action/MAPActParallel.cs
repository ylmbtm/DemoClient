using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using CFG;

namespace MAP
{
    [MAPActionClass("组合/并行事件", true)]
    public class MAPActParallel : MAPActComposite
    {
        private List<int> m_FinishChildIndexList = new List<int>();

        public override void Trigger()
        {
            base.Trigger();
            this.m_FinishChildIndexList.Clear();
        }

        public override DCFG Export()
        {
            MapActParallel data = new MapActParallel();
            data.ID = this.ID;
            for (int i = 0; i < Actions.Count; i++)
            {
                data.Actions.Add(Actions[i].ID);
            }
            return data;
        }

        public override void Import(DCFG cfg)
        {
            MapActParallel data = cfg as MapActParallel;
            this.ID = data.ID;
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
            for (int i = 0; i < Actions.Count; i++)
            {
                if (m_FinishChildIndexList.Contains(i))
                {
                    continue;
                }
                MAPAction child = Actions[i];
                if (child.State == ENTS.INITIAL)
                {
                    child.Trigger();
                }
                if (child.State != ENTS.RUNNING)
                {
                    m_FinishChildIndexList.Add(i);
                }
            }
            if (m_FinishChildIndexList.Count >= Actions.Count)
            {
                Release();
            }
        }
    }
}
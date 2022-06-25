using UnityEngine;
using System.Collections;
using CFG;

namespace MAP
{
    [MAPActionClass("组合/开始", true)]
    public class MAPActEnter : MAPActParallel
    {
        public override DCFG Export()
        {
            MapActEnter data = new MapActEnter();
            data.ID = this.ID;
            for (int i = 0; i < Actions.Count; i++)
            {
                data.Actions.Add(Actions[i].ID);
            }
            return data;
        }

        public override void Import(DCFG cfg)
        {
            MapActEnter data = cfg as MapActEnter;
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
    }
}
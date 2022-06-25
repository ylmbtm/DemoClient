using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using CFG;

namespace MAP
{
    [MAPActionClass("任务/收集矿石", false)]
    public class MAPActMissionCollectMine : MAPAction
    {
        [MAPFieldAttri] public List<EItem>         List           = new List<EItem>();
        [NonSerialized] public Dictionary<int, int> DictHasCollect = new Dictionary<int, int>();

        public override void Trigger()
        {
            base.Trigger();
        }

        public override void Import(DCFG cfg)
        {
            MapActMissionCollectMine data = cfg as MapActMissionCollectMine;
            this.ID                        = data.ID;
            this.List                      = data.List;
        }

        public override DCFG Export()
        {
            MapActMissionCollectMine data = new MapActMissionCollectMine();
            data.ID                        = this.ID;
            data.List                      = this.List;
            return data;
        }

        void Start()
        {
            GTEventCenter.AddHandler<int, ulong>(GTEventID.TYPE_MAP_COLLECT_MINE, OnCollectMine);
        }

        void OnDestroy()
        {
            GTEventCenter.DelHandler<int, ulong>(GTEventID.TYPE_MAP_COLLECT_MINE, OnCollectMine);
        }

        void OnCollectMine(int id, ulong guid)
        {
            bool finish = true;
            for (int i = 0; i < List.Count; i++)
            {
                int mineID = List[i].Id;
                int v = 0;
                DictHasCollect.TryGetValue(id, out v);
                if (mineID == id)
                {
                    if (v > 0)
                    {
                        DictHasCollect[id]++;
                    }
                    else
                    {
                        DictHasCollect[id] = 1;
                    }
                }
                if (v < List[i].Num)
                {
                    finish = false;
                }
            }
            if (finish)
            {
                Release();
            }
        }
    }
}
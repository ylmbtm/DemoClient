using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CFG;
using System;

namespace MAP
{
    [MAPActionClass("任务/杀死怪物", false)]
    public class MAPActMissionKillMonster : MAPAction
    {
        [MAPFieldAttri] public List<EItem>         List        = new List<EItem>();
        [NonSerialized] public Dictionary<int, int> DictHasKill = new Dictionary<int, int>();

        public override void Trigger()
        {
            base.Trigger();
        }

        public override void Import(DCFG cfg)
        {
            MapActMissionKillMonster data = cfg as MapActMissionKillMonster;
            this.ID                        = data.ID;
            this.List                      = data.List;
        }

        public override DCFG Export()
        {
            MapActMissionKillMonster data = new MapActMissionKillMonster();
            data.ID                        = this.ID;
            data.List                      = this.List;
            return data;
        }

        void Start()
        {
            GTEventCenter.AddHandler<int, ulong>(GTEventID.TYPE_MAP_ACTORDEAD, OnKillMonster);
        }

        void OnDestroy()
        {
            GTEventCenter.DelHandler<int, ulong>(GTEventID.TYPE_MAP_ACTORDEAD, OnKillMonster);
        }

        void OnKillMonster(int id , ulong guid)
        {
            bool finish = true;
            for (int i = 0; i < List.Count; i++)
            {
                int monsterID = List[i].Id;
                int v = 0;
                DictHasKill.TryGetValue(id, out v);
                if (monsterID == id)
                {
                    if (v > 0)
                    {
                        DictHasKill[id]++;
                    }
                    else
                    {
                        DictHasKill[id] = 1;
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
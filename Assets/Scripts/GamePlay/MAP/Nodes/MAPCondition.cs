using CFG;
using UnityEngine;
using System.Collections.Generic;

namespace MAP
{
    public class MAPCondition : MAPElement
    {
        public EWinCondition WinType = EWinCondition.EWC_NONE;
        public int NpcID;
        public int KillMonsterID;
        public int KillMonsterNum;

        public List<float> DestBox = new List<float>();
        public override void Startup()
        {

        }

        public override void Trigger()
        {

        }

        public override void Release()
        {

        }

        public override DCFG Export()
        {
            DTCondition data = new DTCondition();
            data.WinType = (int)this.WinType;
            data.NpcID = this.NpcID;
            data.KillMonsterID = this.KillMonsterID;
            data.KillMonsterNum = this.KillMonsterNum;
            return data;
        }

        public override void Import(DCFG cfg)
        {
            DTCondition data = cfg as DTCondition;
            this.WinType = (EWinCondition)data.WinType;
            this.NpcID = data.NpcID;
            this.KillMonsterID = data.KillMonsterID;
            this.KillMonsterNum = data.KillMonsterNum;
        }

        public override void OnDrawInspector()
        {
#if UNITY_EDITOR
            GUILayout.Space(10);
            this.WinType = (EWinCondition)UnityEditor.EditorGUILayout.EnumPopup("EWinCondition", this.WinType);
            GUILayout.Space(10);

            switch (this.WinType)
            {
                case EWinCondition.EWC_KILL_ALL:
                    break;
                case EWinCondition.EWC_KILL_NUM:
                    this.KillMonsterID = UnityEditor.EditorGUILayout.IntField("KillMonsterID", this.KillMonsterID);
                    this.KillMonsterNum = UnityEditor.EditorGUILayout.IntField("KillMonsterNum", this.KillMonsterNum);
                    break;
                case EWinCondition.EWC_DESTINATION:
                    GTTools.AdaptListCount(ref DestBox, 4);
                    this.DestBox[0] = UnityEditor.EditorGUILayout.FloatField("Left", this.DestBox[0]);
                    this.DestBox[1] = UnityEditor.EditorGUILayout.FloatField("Top", this.DestBox[1]);
                    this.DestBox[2] = UnityEditor.EditorGUILayout.FloatField("Right", this.DestBox[2]);
                    this.DestBox[3] = UnityEditor.EditorGUILayout.FloatField("Bottom", this.DestBox[3]);
                    break;

            }
#endif
        }
    }
}
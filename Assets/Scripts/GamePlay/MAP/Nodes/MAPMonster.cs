using UnityEngine;
using System.Collections;
using CFG;

namespace MAP
{
    public class MAPMonster : MAPElement
    {
        [MAPFieldAttri] public int MonsterID;
        [MAPFieldAttri] public int MonsterType;
        [MAPFieldAttri] public int MonsterAI;
        [MAPFieldAttri] public int Camp;
        [MAPFieldAttri] public int DropID;

        public override void Import(DCFG cfg)
        {
            DTMonster data  = cfg as DTMonster;
            this.MonsterID  = data.MonsterID;
            this.MonsterAI  = data.MonsterAI;
            this.MonsterType= data.MonsterType;
            this.DropID     = data.DropID;
            this.Camp       = data.Camp;
            this.Pos        = data.Pos;
            this.Face       = data.Face;
        }

        public override DCFG Export()
        {
            DTMonster data  = new DTMonster();
            data.MonsterID  = this.MonsterID;
            data.MonsterAI  = this.MonsterAI;
            data.MonsterType= this.MonsterType;
            data.DropID     = this.DropID;
            data.Pos        = this.Pos;
            data.Camp       = this.Camp;
            data.Face       = this.Face;
            return data;
        }

        public override void OnDrawInspector()
        {
#if UNITY_EDITOR
            this.MonsterID      = UnityEditor.EditorGUILayout.IntField("MonsterID",     MonsterID);
            this.MonsterType    = UnityEditor.EditorGUILayout.IntField("MonsterType",   MonsterType);
            this.MonsterAI      = UnityEditor.EditorGUILayout.IntField("MonsterAI",     MonsterAI);
            this.Camp           = UnityEditor.EditorGUILayout.IntField("Camp",          Camp);
            this.DropID         = UnityEditor.EditorGUILayout.IntField("DropID",        DropID);
#endif
        }

        public override void OnDrawGizmos()
        {
            transform.localScale = Vector3.one;
            transform.name = string.Format("对象[{0}]", MonsterID);
            EDraw.DrawGizmosCapsule(transform.position + new Vector3(0, 1, 0), transform.rotation, transform.localScale, Color.red);
        }
    }
}
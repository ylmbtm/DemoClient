using UnityEngine;
using System.Collections;
using System;
using CFG;

namespace MAP
{
    public class MAPBorn : MAPElement
    {
        public int Camp = 0;
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
            DTBorn data = new DTBorn();
            data.ID      = this.ID;
            data.Pos     = this.Pos;
            data.Face    = this.Face;
            data.Camp    = this.Camp;
            return data;
        }

        public override void Import(DCFG cfg)
        {
            DTBorn data = cfg as DTBorn;
            this.ID      = data.ID;
            this.Pos     = data.Pos;
            this.Face    = data.Face;
            this.Camp = data.Camp;
        }

        public override void OnDrawGizmos()
        {
            transform.localScale = Vector3.one;

            EDraw.DrawGizmosCapsule(transform.position + new Vector3(0, 2, 0), transform.rotation, transform.localScale * 2, Color.green);

        }

        public override void OnDrawInspector()
        {
#if UNITY_EDITOR
            GUILayout.Space(10);
            this.ID = UnityEditor.EditorGUILayout.IntField("ID", this.ID);
            this.Camp = UnityEditor.EditorGUILayout.IntField("Camp", this.Camp);
            GUILayout.Space(10);
#endif
        }
    }
}
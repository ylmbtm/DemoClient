using UnityEngine;
using System.Collections;
using CFG;
using System.Collections.Generic;
using System;

namespace MAP
{
    public class MAPArea : MAPElement
    {
        [MAPFieldAttri] public EAreaShape      Shape       = EAreaShape.TYPE_CIRCLE;
        [MAPFieldAttri] public bool            AllowRide   = false;
        [MAPFieldAttri] public bool            AllowPK     = false;
        [MAPFieldAttri] public bool            AllowTrade  = false;
        [MAPFieldAttri] public bool            AllowAwake  = false;
        [MAPFieldAttri] public bool            AllowLoop   = false;
        [MAPFieldAttri] public List<MAPAction> Actions     = new List<MAPAction>();

        [NonSerialized] public bool            HasTrigger  = false;

        public override void Startup()
        {
            if(AllowAwake && Application.isPlaying)
            {
                Trigger();
            }
        }

        public override void Trigger()
        {
            BoxCollider bc = gameObject.GET<BoxCollider>();
            bc.isTrigger = true;
        }

        public override void Release()
        {

        }

        public override DCFG Export()
        {
            DTArea data     = new DTArea();
            data.ID          = this.ID;
            data.Pos         = this.Pos;
            data.Shape       = (int)this.Shape;
            data.Scale       = this.Scale;
            data.AllowRide   = this.AllowRide;
            data.AllowPK     = this.AllowPK;
            data.AllowTrade  = this.AllowTrade;
            data.AllowActive = this.AllowAwake;
            for (int i = 0; i < Actions.Count; i++)
            {
                data.Actions.Add(Actions[i].ID);
            }
            return data;
        }

        public override void Import(DCFG cfg)
        {
            DTArea data            = cfg as DTArea;
            this.ID                 = data.ID;
            this.Pos                = data.Pos;
            this.Scale              = data.Scale;
            this.Shape              = (EAreaShape)data.Shape;
            this.AllowRide          = data.AllowRide;
            this.AllowPK            = data.AllowPK;
            this.AllowTrade         = data.AllowTrade;
            this.AllowAwake         = data.AllowActive;
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

        public override void OnDrawGizmos()
        {
            float x = transform.localScale.x;
            float z = transform.localScale.z;
            float y = transform.localScale.y;
            switch (Shape)
            {
                case EAreaShape.TYPE_CIRCLE:
                    float r = Mathf.Clamp(x, 0.1f, 2000);
                    transform.localScale = new Vector3(r, y, r);
                    EDraw.DrawGizmosCylinder(transform.position, transform.rotation, transform.localScale, new Color(0.00f, 1.00f, 1.00f, 0.5f));
                    break;
                case EAreaShape.TYPE_RECT:
                    float w = Mathf.Clamp(x, 0.1f, 2000);
                    float l = Mathf.Clamp(z, 0.1f, 2000);
                    transform.localScale = new Vector3(w, y, l);
                    EDraw.DrawGizmosCube(transform.position, transform.rotation, transform.localScale, new Color(0.00f, 1.00f, 1.00f, 0.5f), Vector3.one);
                    break;
            }
            for (int i = 0; i < Actions.Count; i++)
            {
                if (Actions[i] == null)
                {
                    Actions.RemoveAt(i);
                    break;
                }
            }
        }

        public          void OnTriggerEnter(Collider other)
        {
            if (AllowLoop && HasTrigger) return;
            Actor cc = other.gameObject.GetComponent<Actor>();
            if (cc == null) return;
            if (cc != GTWorld.Main) return;
            for (int i = 0; i < Actions.Count; i++)
            {
                Actions[i].Startup();
            }
            HasTrigger = true;
        }

        public override void OnDrawInspector()
        {
#if UNITY_EDITOR
            GUILayout.Space(10);
            this.ID         = UnityEditor.EditorGUILayout.IntField("ID", this.ID);
            this.Shape      = (EAreaShape)UnityEditor.EditorGUILayout.EnumPopup("Shape", this.Shape);
            this.AllowRide  = UnityEditor.EditorGUILayout.Toggle("AllowRide", this.AllowRide);
            this.AllowPK    = UnityEditor.EditorGUILayout.Toggle("AllowPK", this.AllowPK);
            this.AllowTrade = UnityEditor.EditorGUILayout.Toggle("AllowTrade", this.AllowTrade);
            this.AllowAwake = UnityEditor.EditorGUILayout.Toggle("AllowAwake", this.AllowAwake);
            GUILayout.Space(10);
#endif
        }
    }
}
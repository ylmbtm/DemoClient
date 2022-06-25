using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CFG;

namespace MAP
{
    public class MAPPath : MAPElement
    {
        [MAPFieldAttri] public EPathNodeType     Type         = EPathNodeType.Linear;
        [MAPFieldAttri] public List<MAPPathNode> PathNodes    = new List<MAPPathNode>();
        [MAPFieldAttri] public bool              PositionVary = true;
        [MAPFieldAttri] public bool              RotationVary = true;

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
            DTMapPath data      = new DTMapPath();
            data.ID           = this.ID;
            data.Type         = (int)this.Type;
            data.PositionVary = this.PositionVary;
            data.RotationVary = this.RotationVary;
            for (int i = 0; i < PathNodes.Count; i++)
            {
                DTPathNode d = (DTPathNode)PathNodes[i].Export();
                data.PathNodes.Add(d);
            }
            return data;
        }

        public override void Import(DCFG cfg)
        {
            DTMapPath data      = cfg as DTMapPath;
            this.ID           = data.ID;
            this.Type         = (EPathNodeType)data.Type;
            this.PositionVary = data.PositionVary;
            this.RotationVary = data.RotationVary;
            for (int i = 0; i < data.PathNodes.Count; i++)
            {
                DTPathNode d = data.PathNodes[i];
                MAPPathNode p = new GameObject().AddComponent<MAPPathNode>();
                p.transform.parent = transform;
                p.Import(d);
            }
        }

        public          void UpdateActor(Transform actor, float time)
        {
            Vector3    p = Vector3.zero;
            Quaternion q = Quaternion.identity;
            Vector3    s = Vector3.one;
            switch(Type)
            {
                case EPathNodeType.Linear:
                    MAPPathHelper.PathLinearInterp(time, PathNodes, out p, out q, out s);
                    break;
                case EPathNodeType.Bezier:
                    MAPPathHelper.PathBezierInterp(time, PathNodes, out p, out q, out s);
                    break;
            }
            if (this.PositionVary)
            {
                actor.position = p;
            }
            if (this.RotationVary)
            {
                actor.rotation = q;
            }
        }

        public override void OnDrawGizmos()
        {
#if UNITY_EDITOR
            transform.localScale = Vector3.one;
            if (PathNodes.Count < 2)
            {
                return;
            }
            UnityEditor.Handles.color = Color.green;
            for (int i = 0; i < PathNodes.Count; i++)
            {
                MAPPathNode n1 = PathNodes[i];
                if (n1 == null)
                {
                    PathNodes.RemoveAt(i);
                    break;
                }
                n1.name = string.Format("P {0}", i);
                EDraw.DrawGizmosSphere(n1.transform, Color.green, 0.5f);
                EDraw.DrawLabelIcon(n1.gameObject, 6);
                if (i < PathNodes.Count - 1)
                {
                    MAPPathNode n2 = PathNodes[i + 1];
                    if (n2 == null)
                    {
                        continue;
                    }
                    switch (Type)
                    {
                        case EPathNodeType.Linear:
                            UnityEditor.Handles.DrawBezier(n1.transform.position, n2.transform.position, n1.transform.position, n2.transform.position, Color.green, null, 2);
                            break;
                        case EPathNodeType.Bezier:
                            UnityEditor.Handles.DrawBezier(n1.transform.position, n2.transform.position, n1.TangentOut, n2.TangentIn, Color.green, null, 2);
                            break;
                    }
                }
            }
#endif
        }

        public override void OnDrawInspector()
        {
            MAPLayout.DrawPathNodes(this);
        }

        public MAPPathNode AddElement()
        {
            MAPPathNode t = new GameObject(typeof(MAPPathNode).Name).AddComponent<MAPPathNode>();
            t.transform.parent = transform;
            t.Init();
            if (PathNodes.Count >= 2)
            {
                MAPPathNode p1 = PathNodes[PathNodes.Count - 1];
                MAPPathNode p2 = PathNodes[PathNodes.Count - 2];
                t.Pos = (p1.Pos - p2.Pos) + p1.Pos;
            }
            PathNodes.Add(t);
            return t;
        }

        public MAPPathNode DelElement(int index)
        {
            MAPPathNode t = PathNodes[index];
            GameObject.DestroyImmediate(t.gameObject);
            PathNodes.RemoveAt(index);
            return null;
        }

        public MAPPathNode InsElement(int index)
        {
            MAPPathNode t = new GameObject(typeof(MAPPathNode).Name).AddComponent<MAPPathNode>();
            t.transform.parent = transform;
            t.Init();
            MAPPathNode p1 = null;
            MAPPathNode p2 = null;
            if (index > 0)
            {
                p1 = PathNodes[index - 1];
            }
            if (index <= PathNodes.Count - 1)
            {
                p2 = PathNodes[index];
            }
            if (p1 != null && p2 != null)
            {
                t.Pos = (p1.Pos + p2.Pos) * 0.5f;
            }
            PathNodes.Insert(index, t);
            return t;
        }

        public void        AddElement(Vector3 pos, Vector3 eulerAngles)
        {
            MAPPathNode p = AddElement();
            p.transform.position = pos;
            p.transform.eulerAngles = eulerAngles;
        }
    }
}
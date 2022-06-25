using UnityEngine;
using System.Collections;
using CFG;

namespace MAP
{
    public class MAPPathNode : MAPElement
    {
        [MAPFieldAttri] public Transform Tangent = null;
        [MAPFieldAttri] public float     Time    = 0;

        public Vector3 TangentIn
        {
            get { return 2 * transform.position - Tangent.position; }
            set { Tangent.position = 2 * transform.position - value; }
        }

        public Vector3 TangentOut
        {
            get { return Tangent.position;}
            set { Tangent.position = value; }
        }

        public void    Init()
        {
            this.Tangent = new GameObject("Tangent").transform;
            this.Tangent.position = transform.position;
            this.Tangent.parent = transform;
        }

        public override DCFG Export()
        {
            DTPathNode data      = new DTPathNode();
            data.Time             = Time;
            data.Pos              = Pos;
            data.EulerAngles      = EulerAngles;
            data.TangentPos       = Tangent.position;
            return data;
        }

        public override void Import(DCFG cfg)
        {
            DTPathNode data      = cfg as DTPathNode;
            this.Time             = data.Time;
            this.Pos              = data.Pos;
            this.EulerAngles      = data.EulerAngles;
            this.Init();
            this.Tangent.position = data.Pos;
        }
    }
}
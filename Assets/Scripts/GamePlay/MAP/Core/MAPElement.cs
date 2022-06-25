using UnityEngine;
using System.Collections;
using System;

namespace MAP
{
    public class MAPElement : MonoBehaviour
    {
        private MAPWorldMap m_FTWorldMap;

        [MAPFieldAttri] public Int32       ID = 0;

        [MAPFieldAttri] public Vector3     Pos
        {
            get { return transform.position; }
            set { transform.position = value; }
        }

        [MAPFieldAttri] public Vector3     EulerAngles
        {
            get { return transform.eulerAngles; }
            set { transform.eulerAngles = value; }
        }

        [MAPFieldAttri] public Vector3     Scale
        {
            get { return transform.localScale; }
            set { transform.localScale = value; }
        }

        [MAPFieldAttri] public float       Face
        {
            get { return transform.eulerAngles.y; }
            set { transform.eulerAngles = new Vector3(0, value, 0); }
        }

        [MAPFieldAttri] public MAPWorldMap Map
        {
            get
            {
                if (m_FTWorldMap == null)
                {
                    m_FTWorldMap = GetComponentInParent<MAPWorldMap>();
                }
                return m_FTWorldMap;
            }
        }

        [MAPFieldAttri] public ENTS        State
        {
            get;
            protected set;
        }

        public virtual void Startup()
        {

        }

        public virtual void Trigger()
        {

        }

        public virtual void Release()
        {

        }

        public virtual void Execute()
        {

        }

        public virtual DCFG Export()
        {
            return null;
        }

        public virtual void Import(DCFG cfg)
        {

        }

        public virtual void OnDrawGizmos()
        {

        }

        public virtual void OnDrawInspector()
        {

        }

        public virtual void OnMoveElementToGround()
        {
            Ray ray1 = new Ray(transform.position, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(ray1, out hit, Mathf.Infinity, LayerMask.GetMask("Default")))
            {
                transform.position = hit.point;
                return;
            }

            Ray ray2 = new Ray(transform.position, Vector3.up);
            if (Physics.Raycast(ray2, out hit, Mathf.Infinity, LayerMask.GetMask("Default")))
            {
                transform.position = hit.point;
                return;
            }
        }
    }
}
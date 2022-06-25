using UnityEngine;
using System.Collections;
using System;
using Protocol;
using System.Collections.Generic;

namespace ECT
{
    public class Effect : MonoBehaviour
    {
        public ulong     GUID { get; set; }
        public Int32     ID   { get; set; }

        public Transform CacheTransform
        {
            get { return transform; }
        }

        public Vector3   Pos
        {
            get { return transform.localPosition; }
            set { transform.localPosition = value; }
        }

        public Vector3   Euler
        {
            get { return transform.localEulerAngles; }
            set { transform.localEulerAngles = value; }
        }

        public Vector3   Scale
        {
            get { return transform.localScale; }
            set { transform.localScale = value; }
        }

        private bool   m_Retain;
        private bool   m_Loaded;
        private Single m_LifeTime;
        private Single m_StatTime;
        private bool m_Released;

        public void Init(ulong guid, int id, Transform parent, Vector3 offset, Vector3 eulerAngles, Vector3 scale, float lifeTime, bool retain)
        {
            this.GUID = guid;
            this.ID = id;
            this.transform.parent = parent;
            this.Pos = offset;
            this.Euler = eulerAngles;
            this.Scale = scale;
            this.m_LifeTime = lifeTime;
            this.m_StatTime = Time.realtimeSinceStartup;
            this.m_Retain = retain;
            this.m_Loaded = true;
            this.m_Released = false;
        }

        public bool IsEnd()
        {
            if (m_Loaded == false)
            {
                return false;
            }

            if (m_LifeTime > 0 && Time.realtimeSinceStartup - m_StatTime > m_LifeTime)
            {
                return true;
            }

            return false;
        }

        public void Release()
        {
            DEffect db = ReadCfgEffect.GetDataById(ID);
            if (db == null)
            {
                return ;
            }


            if(m_Released)
            {
                return;
            }

            m_Released = true;

            if (string.IsNullOrEmpty(db.Path) == false)
            {
                GTPoolManager.Instance.ReleaseGo(db.Path, gameObject, m_Retain);
            }
        }
    }
}
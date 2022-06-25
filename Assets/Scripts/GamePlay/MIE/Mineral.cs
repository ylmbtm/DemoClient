using UnityEngine;
using System.Collections;
using System;
using Protocol;

namespace MIE
{
    public class Mineral : MonoBehaviour, IActor
    {
        public ulong     GUID { get; set; }
        public Int32     ID   { get; set; }

        public Transform CacheTransform
        {
            get { return transform; }
        }

        public Vector3   Pos
        {
            get { return transform.position; }
            set { transform.position = value; }
        }

        public Vector3   Euler
        {
            get { return transform.eulerAngles; }
            set { transform.eulerAngles = value; }
        }

        public Vector3   Scale
        {
            get { return transform.localScale; }
            set { transform.localScale = value; }
        }

        public void Init(XMineral data)
        {
            this.GUID = data.GUID;
            this.ID = data.Id;
        }
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml;

namespace BIE
{
    public class GuidePath : GuideEffect
    {
        public Vector3       TargetScale      = new Vector3(5, 5, 5);
        public Vector3       TargetPos        = new Vector3(0, -2.16f, 0);
        public string        TargetEffectPath = string.Empty;

        private Actor             m_Character;
        private Vector3           m_DirectionToTarget;
        private ETriggerObject    m_TargetTriggerObject;
        private GameObject        m_TargetArrow;
        private List<GameObject>  m_ArrowGameObjectList = new List<GameObject>();
        private float             m_BetweenLength       = 2f;
        private float             m_PerLength           = 0.5f;
        private float             m_Offset              = 1.2f;

        public override void Enter()
        {

        }

        public override void Execute()
        {

        }

        public override void Finish()
        {

        }

        public override void Read(XmlElement os)
        {
            this.TargetScale       = os.GetVector3("TargetScale");
            this.TargetPos         = os.GetVector3("TargetPos");
            this.TargetEffectPath  = os.GetString("TargetEffectPath");
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            DCFG.Write(doc, os, "TargetScale",      TargetScale);
            DCFG.Write(doc, os, "TargetPos",        TargetPos);
            DCFG.Write(doc, os, "TargetEffectPath", TargetEffectPath);
        }
    }  
}
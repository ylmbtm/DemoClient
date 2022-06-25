using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml;

namespace ACT
{
    [ActActionClass("动作", "作用效果", "")]
    public partial class ActScope : ActItem
    {
        public ERangeType           RangeType         = ERangeType.ERT_OBJECTS;
        public List<float>          RangeParams       = new List<float>();
        public List<ActResult>      Results           = new List<ActResult>();
        public Int32                HitActionID       = 13;
        public Int32                HitEffectID       = 0;
        public float                HitDistance       = 0;

        private Int32           m_CurrLoopsNum;
        private float           m_LastProcessTime;
        private bool            m_StatProcess = true;

        public override ActItem Clone()
        {
            return this;
        }
        protected override bool Trigger()
        {
            m_StatProcess = false;
            return true;
        }

        protected override void Execute()
        {
            if (m_StatProcess == false)
            {
                m_CurrLoopsNum++;
                m_LastProcessTime = Time.realtimeSinceStartup;
                m_StatProcess = true;
            }
        }

        protected override void End()
        {
            m_CurrLoopsNum = 0;
        }

        protected override void Release()
        {
            m_CurrLoopsNum = 0;
            m_StatProcess = false;
        }

        public ActResult AddChild(Type type)
        {
            ActResult child = (ActResult)System.Activator.CreateInstance(type);
            if(child == null)
            {
                return null;
            }

            Results.Add(child);

            return child;
        }

        public ActResult DelChild(ActResult data)
        {
            Results.Remove(data);
            return null;
        }

        public override void Load(XmlElement element)
        {
            base.Load(element);
            if (EdTime < 0.000001)
            {
                EdTime = StTime;
            }
            XmlElement child = element.FirstChild as XmlElement;
            while (child != null)
            {
                Type type = System.Type.GetType("ACT" + "." + child.Name);
                if(type== null)
                {
                    child = child.NextSibling as XmlElement;
                    continue;
                }
                  
                try
                {
                    ActResult result = AddChild(type);
                    if (result == null)
                    {
                        child = child.NextSibling as XmlElement;
                        continue;
                    }
                    result.Load(child);
                    child = child.NextSibling as XmlElement;
                }
                catch (Exception ex)
                {
                    child = child.NextSibling as XmlElement;
                }
            }
        }

        public override void Save(XmlDocument doc, XmlElement element)
        {
            if (EdTime < 0.000001)
            {
                EdTime = StTime;
            }
            base.Save(doc, element);
            for (int i = 0; i < Results.Count; i++)
            {
                ActResult it = Results[i];
                XmlElement child = doc.CreateElement(it.GetType().Name);
                element.AppendChild(child);
                it.Save(doc, child);
            }
        }
    }
}
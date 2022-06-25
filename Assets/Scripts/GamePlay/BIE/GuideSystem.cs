using UnityEngine;
using System.Collections;
using System.Xml;
using System.Collections.Generic;
using System;
using Protocol;

namespace BIE
{
    public class GuideSystem
    {
        public bool                           UseGuide        = true;
        public bool                           PauseGuide      = false;

        private Int32                         m_CurrentId     = 0;
        private Guide                         m_CurrentGuide  = null;
        private Dictionary<string, Transform> m_GuideLockObjs = new Dictionary<string, Transform>();
        private GuideConfig                   m_GuideConfig   = new GuideConfig();
        private Guide                         m_TriggerGuide  = null;

        public void Startup()
        {
            if (UseGuide == false)
            {
                return;
            }
            m_GuideConfig.Load("Text/Guide/Guide");
            GuideItem data = DataDBSGuide.GetDataById(1);
            if (data != null)
            {
                this.m_CurrentId = data.Id;
                this.m_CurrentId++;
                this.m_CurrentGuide = CreateGuide(this.m_CurrentId);
            }
            else
            {
                this.m_CurrentId = 1;
                this.m_CurrentGuide = CreateGuide(this.m_CurrentId);
            }
        }

        public void Execute()
        {
            if (PauseGuide == true || UseGuide == false)
            {
                return;
            }
            UpdateCurrentGuide();
            UpdateTriggerGuide();
        }

        void MoveNext()
        {
            if(this.m_CurrentGuide.IsSavePoint)
            {
                GuideItem guide = new GuideItem();
                guide.Id = this.m_CurrentGuide.Id;
                DataDBSGuide.Update(1, guide);
            }
            this.m_CurrentId++;
            this.m_CurrentGuide = CreateGuide(this.m_CurrentId);
        }

        void UpdateCurrentGuide()
        {
            if (m_CurrentGuide == null)
            {
                return;
            }
            if (m_CurrentGuide.State == EGuideState.TYPE_NONE)
            {
                if (m_CurrentGuide.Check())
                {
                    m_CurrentGuide.Enter();
                };
            }
            if (m_CurrentGuide.State == EGuideState.TYPE_EXECUTE)
            {
                m_CurrentGuide.Execute();
            }
            if (m_CurrentGuide.State == EGuideState.TYPE_FINISH)
            {
                MoveNext();
            }
        }

        void UpdateTriggerGuide()
        {
            if (m_TriggerGuide == null)
            {
                return;
            }
            if (m_TriggerGuide.State == EGuideState.TYPE_NONE)
            {
                if (m_TriggerGuide.Check())
                {
                    m_TriggerGuide.Enter();
                };
            }
            if (m_TriggerGuide.State == EGuideState.TYPE_EXECUTE)
            {
                m_TriggerGuide.Execute();
            }
            if (m_TriggerGuide.State == EGuideState.TYPE_FINISH)
            {
                m_TriggerGuide = null;
            }
        }

        Guide CreateGuide(Int32 id)
        {
            Guide guide = null;
            if (id <= m_GuideConfig.Items.Count)
            {
                guide = m_GuideConfig.Items[id - 1];
            }
            if (guide != null)
            {
                return guide;
            }
            else
            {
                return null;
            }
        }

        public void TriggerGuide(int id)
        {
            m_TriggerGuide = CreateGuide(id);
        }

        public Transform GetData(string key)
        {
            if(string.IsNullOrEmpty(key))
            {
                return null;
            }
            else
            {
                Transform trans = null;
                m_GuideLockObjs.TryGetValue(key, out trans);
                return trans;
            }
        }

        public void AddGuideListener(string key, Transform trans)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }
            if (trans == null)
            {
                return;
            }
            m_GuideLockObjs[key] = trans;
        }

        public void DelGuideListener(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }
            m_GuideLockObjs.Remove(key);
        }
    }
}
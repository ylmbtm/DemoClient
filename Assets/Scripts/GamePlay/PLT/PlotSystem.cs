using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;

namespace PLT
{
    public class PlotSystem
    {
        private Action           m_Finish;
        private bool             m_Play;
        private PlayableDirector m_PlotDirector;

        public void Trigger(int id, Action onFinish = null)
        {
            DPlayable db = ReadCfgPlayable.GetDataById(id);
            GameObject directorAsset = GTResourceManager.Instance.Load<GameObject>(db.Path, true);
            if (directorAsset == null)
            {
                Debug.LogError(string.Format("directorAsset:{0} is null", id));
                End();
                return;
            }
            PlotTextComponent component = directorAsset.GetComponent<PlotTextComponent>();
            if (component != null)
            {
                component.enabled = false;
            }
            NGUITools.SetLayer(directorAsset, GTLayer.LAYER_DEFAULT);
            List<Camera> cams = new List<Camera>();
            directorAsset.GetComponents(cams);
            directorAsset.GetComponentsInChildren(cams);
            for (int i = 0; i < cams.Count; i++)
            {
                cams[i].cullingMask = 1 << GTLayer.LAYER_DEFAULT;
            }
            m_PlotDirector = directorAsset.GetComponent<PlayableDirector>();
            m_PlotDirector.Play();
            m_PlotDirector.extrapolationMode = DirectorWrapMode.Hold;
            m_Finish = onFinish;
            m_Play = true;
        }

        public void Execute()
        {
            if (m_Play == false)
            {
                return;
            }
            if (m_PlotDirector.time >= m_PlotDirector.duration)
            {
                End();
            }
        }

        public void Skip()
        {
            if (m_PlotDirector == null)
            {
                return;
            }
            m_PlotDirector.time = m_PlotDirector.duration;
        }

        public void End()
        {
            if (m_Finish != null)
            {
                m_Finish();
                m_Finish = null;
            }
            if (m_PlotDirector != null)
            {
                GameObject.DestroyImmediate(m_PlotDirector.gameObject);
                m_PlotDirector = null;
            }
            m_Play = false;
        }

        public bool IsPlaying()
        {
            return m_Play;
        }
    }
}
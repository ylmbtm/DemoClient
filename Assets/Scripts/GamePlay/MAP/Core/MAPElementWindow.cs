using UnityEngine;
using System.Collections;
using System;

namespace MAP
{
#if UNITY_EDITOR
    public class MAPElementWindow : UnityEditor.EditorWindow
    {
        private UnityEditor.SceneView.OnSceneFunc  m_SceneGUIDelegate = null;
        private IMAPContainer                      m_MAPContainer      = null;

        void OnEnable()
        {
            m_SceneGUIDelegate = new UnityEditor.SceneView.OnSceneFunc(OnSceneGUIDelegate);
            UnityEditor.SceneView.onSceneGUIDelegate += m_SceneGUIDelegate;
        }

        void OnDisable()
        {
            UnityEditor.SceneView.onSceneGUIDelegate -= m_SceneGUIDelegate;
            m_MAPContainer = null;
        }

        void OnDestroy()
        {
            UnityEditor.SceneView.onSceneGUIDelegate -= m_SceneGUIDelegate;
            m_MAPContainer = null;
        }

        void OnSceneGUIDelegate(UnityEditor.SceneView sceneView)
        {
            if(m_MAPContainer == null)
            {
                Close();
                return;
            }
            UnityEditor.HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            Event e = Event.current;
            if (e.button == 0 && e.type == EventType.MouseDown)
            {
                if (e.modifiers == EventModifiers.Shift)
                {
                    Ray ray = UnityEditor.HandleUtility.GUIPointToWorldRay(e.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Default")))
                    {
                        m_MAPContainer.OnGainElementOnGround(hit.point, Vector3.zero);
                    }
                }
            }
        }

        public void SetGroup(IMAPContainer fTContainer)
        {
            this.m_MAPContainer = fTContainer;
        }
    }
#endif
}
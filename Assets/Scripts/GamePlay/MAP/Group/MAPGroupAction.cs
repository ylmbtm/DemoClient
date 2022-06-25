using UnityEngine;
using System.Collections;
using System;

namespace MAP
{
    public class MAPGroupAction : MAPGroup<MAPAction>
    {
        public MAPAction AddEvent(Type type)
        {
            if (GetComponentInChildren<MAPActEnter>() == false)
            {
                AddEventWithoutCheck(typeof(MAPActEnter));
            }          
            return AddEventWithoutCheck(type);
        }

        public MAPAction AddEventWithoutCheck(Type type)
        {
            MAPAction t = (MAPAction)new GameObject(type.Name).AddComponent(type);
            t.transform.parent = transform;
            t.ID = GetUseID();
            m_Elements.Add(t);
            return t;
        }

        public MAPAction AddEvent(int id, Type type)
        {
            MAPAction t = (MAPAction)new GameObject(type.Name).AddComponent(type);
            t.ID = id;
            t.transform.parent = transform;
            m_Elements.Add(t);
            return t;
        }

        public MAPAction DelEvent(int id)
        {
            for (int i = 0; i < m_Elements.Count; i++)
            {
                MAPAction e = m_Elements[i];
                if (e != null && e.ID == id)
                {
                    GameObject.DestroyImmediate(e.gameObject);
                    m_Elements.RemoveAt(i);
                    break;
                }
            }
            return null;
        }

        public override void OnDrawInspector()
        {
#if UNITY_EDITOR
            GUILayout.Space(10);
            GUI.color = Color.green;
            if (GUILayout.Button("添加副本入口、退出", EGUIStyles.Button1, GUILayout.Height(35)))
            {
                if (GetComponentInChildren<MAPActEnter>() == false)
                {
                    AddEventWithoutCheck(typeof(MAPActEnter));
                }
            }
            GUILayout.Space(10);
            if (GUILayout.Button("添加波次", EGUIStyles.Button1, GUILayout.Height(35)))
            {
                AddEventWithoutCheck(typeof(MAPWave));
            }
            GUI.color = Color.white;
            GUILayout.Space(10);
#endif
        }

        public override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
        }
    }
}
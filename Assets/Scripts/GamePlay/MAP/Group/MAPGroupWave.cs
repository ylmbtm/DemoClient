using UnityEngine;
using System.Collections;
using System;

namespace MAP
{
    public class MAPGroupWave : MAPGroup<MAPWave>
    {
        public MAPWave AddEventWithoutCheck(Type type)
        {
            MAPWave t = (MAPWave)new GameObject(type.Name).AddComponent(type);
            t.transform.parent = transform;
            t.ID = GetUseID();
            m_Elements.Add(t);
            return t;
        }

        public MAPWave AddEvent(int id, Type type)
        {
            MAPWave t = (MAPWave)new GameObject(type.Name).AddComponent(type);
            t.ID = id;
            t.transform.parent = transform;
            m_Elements.Add(t);
            return t;
        }

        public MAPWave DelEvent(int id)
        {
            for (int i = 0; i < m_Elements.Count; i++)
            {
                MAPWave e = m_Elements[i];
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
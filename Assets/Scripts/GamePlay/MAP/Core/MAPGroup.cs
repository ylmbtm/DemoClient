using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace MAP
{
    public class MAPGroup<T> : MAPContainer where T : MAPElement
    {
        protected      List<T> m_Elements      = new List<T>();

        public         List<T> GetElements()
        {
            return m_Elements;
        }

        public          T      GetElement(int id)
        {
            List<T> list = GetElements();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].ID == id)
                {
                    return list[i];
                }
            }
            return null;
        }

        public          T      AddElement() 
        {
            T t = new GameObject(typeof(T).Name).AddComponent<T>();
            t.transform.parent = transform;
            t.ID = GetUseID();
            m_Elements.Add(t);
            return t;
        }

        public          void   DelElement(T item)
        {
            if (item != null)
            {
                m_Elements.Remove(item);
                GameObject.DestroyImmediate(item.gameObject);
            }
        }

        public          Int32  GetUseID()
        {
            HashSet<int> keys = new HashSet<int>();
            List<T>   list = GetElements();
            for (int i = 0; i < list.Count; i++)
            {
                if (keys.Contains(list[i].ID) == false)
                {
                    keys.Add(list[i].ID);
                }
            }
            int index = 1;
            while (keys.Contains(index))
            {
                index++;
            }
            return index;
        }

        public override void   OnDrawGizmos()
        {
            List<T> list = GetElements();
            for (int i = 0; i < list.Count; i++)
            {
                if(list[i] == null)
                {
                    list.RemoveAt(i);
                }
                else
                {
                    list[i].name = string.Format("[{0}] {1}", list[i].ID.ToString("000"), list[i].GetType().Name);
                }
            }
        }

        public override void   OnDrawInspector()
        {
#if UNITY_EDITOR
            GUILayout.Space(10);
            GUI.color = Color.green;
            if (this is MAPGroupAction)
            {
                return;
            }
            if (GUILayout.Button("添加元素", EGUIStyles.Button1, GUILayout.Height(35)))
            {
                OnGainElementOnGround(Vector3.zero, Vector3.zero);
            }

            GUILayout.Space(10);
            GUI.color = Color.green;
            if (GUILayout.Button("快捷编辑", EGUIStyles.Button1, GUILayout.Height(35)))
            {
                UnityEditor.EditorWindow.GetWindow<MAPElementWindow>().SetGroup(this);
                UnityEditor.EditorWindow.GetWindow<MAPElementWindow>().Show();
            }

            GUI.color = Color.green;
            GUILayout.Space(10);
            if (GUILayout.Button("元素贴地", EGUIStyles.Button1, GUILayout.Height(35)))
            {
                OnMoveElementToGround();
            }
            GUILayout.Space(10);
#endif
        }

        public override void   OnGainElementOnGround(Vector3 pos, Vector3 eulerAngles)
        {
            T t = AddElement();
            t.transform.position = pos;
            t.transform.eulerAngles = eulerAngles;
        }

        public override void   OnMoveElementToGround()
        {
            List<T> list = GetElements();
            for (int i = 0; i < list.Count; i++)
            {
                list[i].OnMoveElementToGround();
            }
        }
    }
}
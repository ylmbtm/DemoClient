using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace MAP
{
    public class MAPActComposite : MAPAction
    {
        [MAPFieldAttri] public List<MAPAction> Actions = new List<MAPAction>();

        public override void OnDrawInspector()
        {
#if UNITY_EDITOR
            GUILayout.Space(10);
            this.ID = UnityEditor.EditorGUILayout.IntField("ID", this.ID);
            GUILayout.Space(10);
            GUI.color = Color.green;
            if (GUILayout.Button("添加事件", EGUIStyles.Button1, GUILayout.Height(35)))
            {
                MAPEditorHelper.ShowMenuWithMAPClass(typeof(MAPAction), (obj) =>
                {
                    MAPAction element = Map.GetGroup<MAPGroupAction>().AddEvent(obj as Type);
                    Actions.Add(element);
                });
            }

            GUILayout.Space(10);
            GUI.color = Color.white;
            for (int i = 0; i < Actions.Count; i++)
            {
                Actions[i] = (MAPAction)UnityEditor.EditorGUILayout.ObjectField(Actions[i].ID.ToString("000"), Actions[i], Actions[i].GetType(), true);
            }
            GUILayout.Space(10);
            GUI.color = Color.white;
#endif
        }

        public override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            for (int i = 0; i < Actions.Count; i++)
            {
                if (Actions[i] == null)
                {
                    Actions.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace EDT
{
    [CustomEditor(typeof(Actor), true)]
    public class EditorActor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Actor cc = target as Actor;
            if (cc == null)
            {
                return;
            }
            EditorGUILayout.TextField("ID",       cc.ID.ToString());
            EditorGUILayout.TextField("Type",     cc.Type.ToString());
            EditorGUILayout.TextField("GUID",     cc.GUID.ToString());
            EditorGUILayout.TextField("Camp",     cc.Camp.ToString());
            EditorGUILayout.TextField("Name",     cc.Name);
            EditorGUILayout.ObjectField("Target", cc.Target == null ? null : cc.Target, typeof(Actor), true);

            GUI.color = Color.green;
            for (int i = 0; i < cc.Components.Count; i++)
            {
                GUILayout.Label(cc.Components[i].GetType().ToString());
            }
            GUI.color = Color.white;
        }
    }
}


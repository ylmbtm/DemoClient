using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace MAP
{
    public class MAPLayout
    {
        public static void DrawPathNodes(MAPPath path)
        {
#if UNITY_EDITOR
            GUILayout.Space(10);
            path.ID           = UnityEditor.EditorGUILayout.IntField("ID", path.ID);
            path.Type         = (EPathNodeType)UnityEditor.EditorGUILayout.EnumPopup("Type", path.Type);
            path.PositionVary = UnityEditor.EditorGUILayout.Toggle("PositionVary", path.PositionVary);
            path.RotationVary = UnityEditor.EditorGUILayout.Toggle("RotationVary", path.RotationVary);
            if (GUILayout.Button("添加路点", GUILayout.Height(40)))
            {
                path.AddElement();
            }
            int index = -1;
            for (int i = 0; i < path.PathNodes.Count; i++)
            {
                MAPPathNode node = path.PathNodes[i];
                if (node == null) continue;
                GUILayout.BeginHorizontal();
                GUILayout.Label(node.name);
                node.Time = UnityEditor.EditorGUILayout.FloatField(string.Empty, node.Time, GUILayout.Width(60));
                GUI.color = Color.cyan;
                if (GUILayout.Button("I"))
                {
                    path.InsElement(i);
                }
                GUI.color = Color.red;
                if (GUILayout.Button("X"))
                {
                    index = i;
                }
                GUI.color = Color.white;
                GUILayout.EndHorizontal();
            }
            if (index >= 0) path.DelElement(index);
            GUILayout.Space(5);
#endif
        }
    }
}
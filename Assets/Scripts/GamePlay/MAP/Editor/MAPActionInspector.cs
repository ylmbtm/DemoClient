using UnityEngine;
using System.Collections;
using UnityEditor;
using MAP;

namespace MAP
{
    [CustomEditor(typeof(MAPAction), true)]
    [CanEditMultipleObjects]
    public class MAPActionInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            MAPAction iMap = target as MAPAction;
            MAPActionClassAttribute attr = GTTools.GetAttribute<MAPActionClassAttribute>(iMap.GetType());
            if (attr == null)
            {
                return;
            }
            if (attr.DrawInspector)
            {
                iMap.OnDrawInspector();
            }
            else
            {
                base.OnInspectorGUI();
            }
        }
    }
}
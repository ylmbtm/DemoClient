using UnityEngine;
using System.Collections;
using UnityEditor;

namespace MAP
{
    [CustomEditor(typeof(MAPContainer), true)]
    public class MAPContainerInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            MAPContainer group = target as MAPContainer;
            group.transform.localPosition = Vector3.zero;
            group.transform.localRotation = Quaternion.identity;
            group.transform.localScale = Vector3.one;
            group.OnDrawInspector();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MAP
{
    [CustomEditor(typeof(MAPElement), true)]
    [CanEditMultipleObjects]
    public class MAPElementInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            MAPElement c = target as MAPElement;
            c.OnDrawInspector();
        }
    }
}
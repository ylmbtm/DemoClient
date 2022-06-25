using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Reflection;
using System.Linq;

[UnityEditor.InitializeOnLoad]
public class EditorInitialize : Editor
{
    static EditorInitialize()
    {
        var editor = Assembly.GetAssembly(typeof(Editor)).GetTypes().FirstOrDefault((a) =>
        {
            return a.Name == "TransformEditor";
        });
        
    }
}

using UnityEngine;
using System.Collections;
using System;
using System.Linq;

namespace MAP
{
    public class MAPEditorHelper
    {
#if UNITY_EDITOR
        public static void ShowMenuWithMAPClass(Type type, UnityEditor.GenericMenu.MenuFunction2 menuFunction2)
        {
            UnityEditor.GenericMenu menu = new UnityEditor.GenericMenu();
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(item => item.GetTypes())
                   .Where(item => item.IsSubclassOf(type)).ToList();
            for (int i = 0; i < types.Count; i++)
            {
                Type t = types[i];
                MAPActionClassAttribute attr = GTTools.GetAttribute<MAPActionClassAttribute>(t);
                if (attr != null)
                {
                    menu.AddItem(new GUIContent(attr.Strategy), false, menuFunction2, t);
                }
            }
            menu.ShowAsContext();
        }
#endif
    }
}

using UnityEngine;
using System.Collections;
using System;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

[AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
public class EEnumLabelAttribute : PropertyAttribute
{
    public string label;
 
    public EEnumLabelAttribute(string label)
    {
        this.label = label;
    }
}
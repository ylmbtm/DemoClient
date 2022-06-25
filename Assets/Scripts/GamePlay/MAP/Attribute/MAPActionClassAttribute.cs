using UnityEngine;
using System.Collections;
using System;

namespace MAP
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MAPActionClassAttribute : Attribute
    {
        public bool   DrawInspector = false;
        public string Strategy      = string.Empty;

        public MAPActionClassAttribute(string strategy , bool drawInspector)
        {
            this.Strategy      = strategy;
            this.DrawInspector = drawInspector;
        }
    }
}
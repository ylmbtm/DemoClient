using UnityEngine;
using System.Collections;
using System;

namespace ACT
{
    public class ActActionClassAttribute : Attribute
    {
        public string Category = string.Empty;
        public string Name     = string.Empty;
        public string Icon     = string.Empty;

        public ActActionClassAttribute(string category, string name, string icon)
        {
            this.Category = category;
            this.Name     = name;
            this.Icon     = icon;
        }
    }
}
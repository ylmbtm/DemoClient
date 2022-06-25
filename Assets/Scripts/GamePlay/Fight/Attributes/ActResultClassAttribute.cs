using UnityEngine;
using System.Collections;
using System;

namespace ACT
{
    public class ActResultClassAttribute : Attribute
    {
        public string Category = string.Empty;
        public string Name     = string.Empty;
        public string Icon     = string.Empty;

        public ActResultClassAttribute(string category, string name, string icon)
        {
            this.Category = category;
            this.Name = name;
            this.Icon = icon;
        }
    }
}
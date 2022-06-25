using UnityEngine;
using System.Collections;
using System;
using System.Xml;
using System.Collections.Generic;

namespace BIE
{
    public class GuideConfig : DCFG
    {
        public List<Guide> Items = new List<Guide>();

        public override void Read(XmlElement os)
        {
            foreach(var current in GetChilds(os))
            {
                Guide cc = new Guide();
                cc.Read(current);
                Items.Add(cc);
            }
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            DCFG.Write(doc, os, "Items", Items);
        }
    }
}
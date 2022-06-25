using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System;

namespace CFG
{
    public class MapActLoop : MapActComposite
    {
        public List<int> Actions = new List<int>();
        public Int32     Loops   = 1;

        public override void Read(XmlElement os)
        {
            this.ID             = os.GetInt32("ID");
            this.Loops          = os.GetInt32("Loops");
            this.Actions        = os.GetListForInt("Actions");
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            DCFG.Write(doc, os, "ID",             this.ID);
            DCFG.Write(doc, os, "Loops",          this.Loops);
            DCFG.Write(doc, os, "Actions",        this.Actions);
        }
    }
}
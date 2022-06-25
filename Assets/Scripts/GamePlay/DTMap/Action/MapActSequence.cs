using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace CFG
{
    public class MapActSequence : MapActComposite
    {
        public List<int> Actions = new List<int>();

        public override void Read(XmlElement os)
        {
            this.ID      = os.GetInt32("ID");
            this.Actions = os.GetListForInt("Actions");
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            DCFG.Write(doc, os, "ID",      this.ID);
            DCFG.Write(doc, os, "Actions", this.Actions);
        }
    }
}
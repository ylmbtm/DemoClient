using UnityEngine;
using System.Collections;
using System.Xml;

namespace CFG
{
    public class MapActDestroyAllObjects : MapAction
    {
        public override void Read(XmlElement os)
        {
            this.ID   = os.GetInt32("ID");
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            DCFG.Write(doc, os, "ID", ID);
        }
    }
}
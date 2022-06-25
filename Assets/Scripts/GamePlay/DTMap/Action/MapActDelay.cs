using UnityEngine;
using System.Collections;
using System.Xml;

namespace CFG
{
    public class MapActDelay : MapAction
    {
        public float Delay = 1;

        public override void Read(XmlElement os)
        {
            this.ID    = os.GetInt32("ID");
            this.Delay = os.GetFloat("Delay");
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            DCFG.Write(doc, os, "ID",    ID);
            DCFG.Write(doc, os, "Delay", Delay);
        }
    }
}
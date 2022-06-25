using UnityEngine;
using System.Collections;
using System.Xml;

namespace CFG
{
    public class MapActTimeScale : MapAction
    {
        public float Rate;
        public float Duration;

        public override void Read(XmlElement os)
        {
            this.ID       = os.GetInt32("ID");
            this.Rate     = os.GetFloat("Rate");
            this.Duration = os.GetFloat("Duration");
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            DCFG.Write(doc, os, "ID",       ID);
            DCFG.Write(doc, os, "Rate",     Rate);
            DCFG.Write(doc, os, "Duration", Duration);
        }
    }
}
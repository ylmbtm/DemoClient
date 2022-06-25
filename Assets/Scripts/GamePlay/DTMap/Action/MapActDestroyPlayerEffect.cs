using UnityEngine;
using System.Collections;
using System.Xml;

namespace CFG
{
    public class MapActDestroyPlayerEffect : MapAction
    {
        public int EffectID;

        public override void Read(XmlElement os)
        {
            this.ID       = os.GetInt32("ID");
            this.EffectID = os.GetInt32("EffectID");
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            DCFG.Write(doc, os, "ID",       ID);
            DCFG.Write(doc, os, "EffectID", EffectID);
        }
    }
}
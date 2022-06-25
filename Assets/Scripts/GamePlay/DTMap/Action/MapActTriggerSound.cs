using UnityEngine;
using System.Collections;
using System.Xml;

namespace CFG
{
    public class MapActTriggerSound : MapAction
    {
        public string AssetPath = string.Empty;
        public float  Volume    = 1;
        public float  Pitch     = 1;

        public override void Read(XmlElement os)
        {
            this.ID        = os.GetInt32("ID");
            this.AssetPath = os.GetString("AssetPath");
            this.Volume    = os.GetFloat("Volume");
            this.Pitch     = os.GetFloat("Pitch");
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            DCFG.Write(doc, os, "ID",        ID);
            DCFG.Write(doc, os, "AssetPath", AssetPath);
            DCFG.Write(doc, os, "Volume",    Volume);
            DCFG.Write(doc, os, "Pitch",     Pitch);
        }
    }
}
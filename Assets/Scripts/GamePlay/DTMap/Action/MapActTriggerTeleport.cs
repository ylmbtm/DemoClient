using UnityEngine;
using System.Collections;
using System.Xml;

namespace CFG
{
    public class MapActTriggerTeleport : MapAction
    {
        public int TeleportID;

        public override void Read(XmlElement os)
        {
            this.ID         = os.GetInt32("ID");
            this.TeleportID = os.GetInt32("TeleportID");
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            DCFG.Write(doc, os, "ID",         ID);
            DCFG.Write(doc, os, "TeleportID", TeleportID);
        }
    }
}
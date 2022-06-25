using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace CFG
{
    public class MapActTriggerArea : MapAction
    {
        public int AreaID;

        public override void Read(XmlElement os)
        {
            this.ID     = os.GetInt32("ID");
            this.AreaID = os.GetInt32("AreaID");
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            DCFG.Write(doc, os, "ID",     ID);
            DCFG.Write(doc, os, "AreaID", AreaID);
        }
    }
}
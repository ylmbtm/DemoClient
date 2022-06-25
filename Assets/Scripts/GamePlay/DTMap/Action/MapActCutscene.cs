using UnityEngine;
using System.Collections;
using System;
using System.Xml;

namespace CFG
{
    public class MapActCutscene : MapAction
    {
        public Int32 CutsceneID = 1;

        public override void Read(XmlElement os)
        {
            this.ID         = os.GetInt32("ID");
            this.CutsceneID = os.GetInt32("CutsceneID");
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            DCFG.Write(doc, os, "ID",         ID);
            DCFG.Write(doc, os, "CutsceneID", CutsceneID);
        }
    }
}
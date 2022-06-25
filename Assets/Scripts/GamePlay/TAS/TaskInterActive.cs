using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Xml;

namespace TAS
{
    [Serializable]
    public class TaskInterActive : TaskBase
    {
        public string       Cmd      = string.Empty;
        public string       AnimName = string.Empty;
        public Int32        NpcID    = 0;
        public Int32        SearchID = 0;

        public override void Read(XmlElement os)
        {
            base.Read(os);
            this.Cmd      = os.GetString("Cmd");
            this.AnimName = os.GetString("AnimName");
            this.NpcID    = os.GetInt32("NpcID");
            this.SearchID = os.GetInt32("SearchID");
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            base.Write(doc, os);
            DCFG.Write(doc, os, "Cmd",      Cmd);
            DCFG.Write(doc, os, "AnimName", AnimName);
            DCFG.Write(doc, os, "NpcID",    NpcID);
            DCFG.Write(doc, os, "SearchID", SearchID);
        }
    }
}


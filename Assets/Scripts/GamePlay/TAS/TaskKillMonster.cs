using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Xml;

namespace TAS
{
    [Serializable]
    public class TaskKillMonster : TaskBase
    {
        public Int32 ID;
        public Int32 Count;
        public Int32 SearchID;

        public override void Read(XmlElement os)
        {
            base.Read(os);
            this.ID       = os.GetInt32("ID");
            this.Count    = os.GetInt32("Count");
            this.SearchID = os.GetInt32("SearchID");
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            base.Write(doc, os);
            DCFG.Write(doc, os, "ID",       ID);
            DCFG.Write(doc, os, "Count",    Count);
            DCFG.Write(doc, os, "SearchID", SearchID);
        }
    }
}
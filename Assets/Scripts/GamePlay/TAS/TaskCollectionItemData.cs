using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Xml;

namespace TAS
{
    [Serializable]
    public class TaskCollectionItemData : DCFG
    {
        public int             ID;
        public int             Count;
        public int             NpcID;
        public float           Rate;
        public int             SearchID;

        public override void Read(XmlElement os)
        {
            this.ID        = os.GetInt32("ID");
            this.Count     = os.GetInt32("Count");
            this.NpcID     = os.GetInt32("NpcID");
            this.Rate      = os.GetFloat("Rate");
            this.SearchID  = os.GetInt32("SearchID");
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            DCFG.Write(doc, os, "ID",          ID);
            DCFG.Write(doc, os, "Count",       Count);
            DCFG.Write(doc, os, "NpcID",       NpcID);
            DCFG.Write(doc, os, "Rate",        Rate);
            DCFG.Write(doc, os, "SearchID",    SearchID);
        }
    }
}
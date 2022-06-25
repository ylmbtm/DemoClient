using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Xml;

namespace TAS
{
    [Serializable]
    public class TaskTalk: TaskBase
    {
        public Int32 StDialogueID = 1;
        public Int32 EdDialogueID = 2;
        public Int32 NpcID;
        public Int32 SearchID;

        public override void Read(XmlElement os)
        {
            base.Read(os);
            this.StDialogueID = os.GetInt32("StDialogueID");
            this.EdDialogueID = os.GetInt32("EdDialogueID");
            this.NpcID        = os.GetInt32("NpcID");
            this.SearchID     = os.GetInt32("SearchID");
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            base.Write(doc, os);
            DCFG.Write(doc, os, "StDialogueID", StDialogueID);
            DCFG.Write(doc, os, "EdDialogueID", EdDialogueID);
            DCFG.Write(doc, os, "NpcID",        NpcID);
            DCFG.Write(doc, os, "SearchID",     SearchID);
        }
    }
}


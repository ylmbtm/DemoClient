using UnityEngine;
using System.Collections;
using System;
using System.Xml;

namespace CFG
{
    public class MapActDialogue : MapAction
    {
        public Int32 StDialogueID = 1;
        public Int32 EdDialogueID = 2;

        public override void Read(XmlElement os)
        {
            this.ID           = os.GetInt32("ID");
            this.StDialogueID = os.GetInt32("StDialogueID");
            this.EdDialogueID = os.GetInt32("EdDialogueID");
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            DCFG.Write(doc, os, "ID", ID);
            DCFG.Write(doc, os, "StDialogueID", StDialogueID);
            DCFG.Write(doc, os, "EdDialogueID", EdDialogueID);
        }
    }
}
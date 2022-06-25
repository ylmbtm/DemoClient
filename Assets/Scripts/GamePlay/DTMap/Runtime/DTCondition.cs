using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Xml;

namespace CFG
{
    public class DTCondition : DTElement
    {
        public int WinType = 0;
        public int NpcID;
        public int KillMonsterID;
        public int KillMonsterNum;

        public override void Read(XmlElement os)
        {
            this.WinType = os.GetInt32("WinType");
            this.NpcID = os.GetInt32("NpcID");
            this.KillMonsterID = os.GetInt32("KillMonsterID");
            this.KillMonsterNum = os.GetInt32("KillMonsterNum");
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            DCFG.Write(doc,os, "WinType", WinType);
            DCFG.Write(doc, os, "NpcID", NpcID);
            DCFG.Write(doc, os, "KillMonsterID", KillMonsterID);
            DCFG.Write(doc, os, "KillMonsterNum", KillMonsterNum);
        }
    }
}
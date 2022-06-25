using UnityEngine;
using System.Collections;
using System.Xml;
using System.Collections.Generic;

namespace CFG
{
    public class DTWave : DTElement
    {
        public List<DTMonster> Monsters = new List<DTMonster>();
        public int TriggerType;
        public Vector4 TriggerBox;
        public int TriggerTime;
        public override void Read(XmlElement os)
        {
            this.ID = os.GetInt32("ID");
            this.TriggerType = os.GetInt32("TriggerType");
            this.TriggerBox = os.GetVector4("TriggerBox");
            this.TriggerTime = os.GetInt32("TriggerTime");
            this.Monsters = ReadList<DTMonster>(os);
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            DCFG.Write(doc, os, "ID",       ID);
            DCFG.Write(doc, os, "TriggerType", TriggerType);
            DCFG.Write(doc, os, "TriggerBox", TriggerBox);
            DCFG.Write(doc, os, "TriggerTime", TriggerTime);
            DCFG.Write(doc, os, Monsters);
        }
    }
}
using UnityEngine;
using System.Collections;
using System.Xml;

namespace CFG
{
    public class MapActDestroyTimer : MapAction
    {
        public int TimerID;

        public override void Read(XmlElement os)
        {
            this.ID      = os.GetInt32("ID");
            this.TimerID = os.GetInt32("TimerID");
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            DCFG.Write(doc, os, "ID",      ID);
            DCFG.Write(doc, os, "TimerID", TimerID);
        }
    }
}
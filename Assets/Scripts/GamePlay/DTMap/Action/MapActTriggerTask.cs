using UnityEngine;
using System.Collections;
using System.Xml;

namespace CFG
{
    public class MapActTriggerTask : MapAction
    {
        public int TaskID;

        public override void Read(XmlElement os)
        {
            this.ID     = os.GetInt32("ID");
            this.TaskID = os.GetInt32("TaskID");
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            DCFG.Write(doc, os, "ID",     ID);
            DCFG.Write(doc, os, "TaskID", TaskID);
        }
    }
}
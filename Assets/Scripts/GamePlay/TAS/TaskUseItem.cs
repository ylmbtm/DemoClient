using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Xml;

namespace TAS
{
    [Serializable]
    public class TaskUseItem : TaskBase
    {
        public int ID;
        public int Times;

        public override void Read(XmlElement os)
        {
            base.Read(os);
            this.ID    = os.GetInt32("ID");
            this.Times = os.GetInt32("Times");
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            base.Write(doc, os);
            DCFG.Write(doc, os, "ID",    ID);
            DCFG.Write(doc, os, "Times", Times);
        }
    }
}


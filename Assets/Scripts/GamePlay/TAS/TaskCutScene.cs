using UnityEngine;
using System.Collections;
using System.Xml;
using System.IO;
using System;

namespace TAS
{
    [Serializable]
    public class TaskCutscene : TaskBase
    {
        public int ID;

        public override void Read(XmlElement os)
        {
            base.Read(os);
            this.ID = os.GetInt32("ID");
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            base.Write(doc, os);
            DCFG.Write(doc, os, "ID", ID);
        }
    }
}

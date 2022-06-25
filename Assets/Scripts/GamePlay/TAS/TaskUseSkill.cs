using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Xml;

namespace TAS
{
    [Serializable]
    public class TaskUseSkill : TaskBase
    {
        public int       Pos;
        public int       Times;

        public override void Read(XmlElement os)
        {
            base.Read(os);
            this.Pos   = os.GetInt32("Pos");
            this.Times = os.GetInt32("Times");
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            base.Write(doc, os);
            DCFG.Write(doc, os, "Pos",        Pos);
            DCFG.Write(doc, os, "Times",      Times);
        }
    }
}
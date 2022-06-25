using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Xml;

namespace TAS
{
    public class TaskBase : DCFG
    {
        public string Desc = string.Empty;

        public override void Read(XmlElement os)
        {
            this.Desc = os.GetString("Desc");
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            DCFG.Write(doc, os, "Desc", Desc);
        }
    }
}
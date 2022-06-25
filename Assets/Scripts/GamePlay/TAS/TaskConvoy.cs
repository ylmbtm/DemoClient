using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;

namespace TAS
{
    [Serializable]
    public class TaskConvoy : TaskBase
    {
        public override void Read(XmlElement os)
        {
            base.Read(os);
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            base.Write(doc, os);
        }
    }
}
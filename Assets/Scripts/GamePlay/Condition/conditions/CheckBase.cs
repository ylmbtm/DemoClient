using UnityEngine;
using System.Collections;
using System;
using System.Xml;

namespace CON
{
    public class CheckBase  : DCFG
    {
        public virtual bool Check()
        {
            return true;
        }

        public override void Read(XmlElement os)
        {

        }

        public override void Write(XmlDocument doc, XmlElement os)
        {

        }
    }
}

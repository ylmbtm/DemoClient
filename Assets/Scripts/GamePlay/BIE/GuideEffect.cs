using UnityEngine;
using System.Collections;
using System;
using System.Xml;

namespace BIE
{
    public class GuideEffect : DCFG
    {
        public virtual void  Enter()   { }
        public virtual void  Execute() { }
        public virtual void  Finish()  { }
        public virtual Guide Current   { get; set; }

        public override void Read(XmlElement os)
        {

        }

        public override void Write(XmlDocument doc, XmlElement os)
        {

        }
    }
}

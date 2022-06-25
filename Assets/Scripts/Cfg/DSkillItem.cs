using UnityEngine;
using System.Collections;
using System;
using System.Xml;

namespace ACT
{
    public class DSkillItem : DObj<int>
    {
        public int Id;

        public override int GetKey()
        {
            return Id;
        }

        public override void Read(XmlElement element)
        {

        }
    }
}
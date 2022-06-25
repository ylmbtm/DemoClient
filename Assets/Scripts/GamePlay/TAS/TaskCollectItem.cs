using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;

namespace TAS
{
    [Serializable]
    public class TaskCollectItem : TaskBase
    {
        public List<TaskCollectionItemData> Items = new List<TaskCollectionItemData>();

        public override void Read(XmlElement os)
        {
            base.Read(os);
            foreach (var current in GetChilds(os))
            {
                switch (current.Name)
                {
                    case "Items":
                        this.Items = ReadList<TaskCollectionItemData>(current);
                        break;
                }
            }
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            base.Write(doc, os);
            DCFG.Write(doc, os, "Items", Items);
        }
    }
}
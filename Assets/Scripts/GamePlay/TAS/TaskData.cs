using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;

namespace TAS
{
    [Serializable]
    public class TaskData : DCFG
    {
        public int               Id;
        public string            Name                      = string.Empty;
        public ETaskType         TaskType                  = ETaskType.NONE;
        public bool              IsCanbeCancle             = false;
        public bool              IsCanbeSearch             = false;
        public bool              IsAutoPathFind            = true;
        public bool              IsFinishedTaskCount       = true;
        public bool              IsAutoFinish              = false;
        public int               PreTaskID                 = 0;
        public List<TaskBase>    SubTasks                  = new List<TaskBase>();

        public override void Read(XmlElement os)
        {
            this.Id                  = os.GetInt32("Id");
            this.Name                = os.GetString("Name");
            this.TaskType                = (ETaskType)os.GetInt32("Type");
            this.IsCanbeCancle       = os.GetBool("IsCanbeCancle");
            this.IsCanbeSearch       = os.GetBool("IsCanbeSearch");
            this.IsAutoPathFind      = os.GetBool("IsAutoPathFind");
            this.IsFinishedTaskCount = os.GetBool("IsFinishedTaskCount");
            this.IsAutoFinish        = os.GetBool("IsAutoFinish");
            this.PreTaskID           = os.GetInt32("PreTaskID");
            foreach (var current in GetChilds(os))
            {
                switch (current.Name)
                {
                    case "SubTasks":
                        foreach(var child in GetChilds(current))
                        {
                            string type = child.GetString("Type");
                            switch(type)
                            {
                                case "TaskTalk":
                                    this.SubTasks.Add(ReadObj<TaskTalk>(child));
                                    break;
                                case "TaskCollectItem":
                                    this.SubTasks.Add(ReadObj<TaskCollectItem>(child));
                                    break;
                                case "TaskConvoy":
                                    this.SubTasks.Add(ReadObj<TaskConvoy>(child));
                                    break;
                                case "TaskGather":
                                    this.SubTasks.Add(ReadObj<TaskGather>(child));
                                    break;
                                case "TaskInterActive":
                                    this.SubTasks.Add(ReadObj<TaskInterActive>(child));
                                    break;
                                case "TaskKillMonster":
                                    this.SubTasks.Add(ReadObj<TaskKillMonster>(child));
                                    break;
                                case "TaskCutscene":
                                    this.SubTasks.Add(ReadObj<TaskCutscene>(child));
                                    break;
                                case "TaskCG":
                                    this.SubTasks.Add(ReadObj<TaskCG>(child));
                                    break;
                                case "TaskUseItem":
                                    this.SubTasks.Add(ReadObj<TaskUseItem>(child));
                                    break;
                                case "TaskUseSkill":
                                    this.SubTasks.Add(ReadObj<TaskUseSkill>(child));
                                    break;
                            }
                        }
                        break;
                }
            }
        }

        public override void Write(XmlDocument doc, XmlElement os)
        {
            DCFG.Write(doc, os, "Id",                  Id);
            DCFG.Write(doc, os, "Name",                this.Name);
            DCFG.Write(doc, os, "TaskType",       (int)this.TaskType);
            DCFG.Write(doc, os, "IsCanbeCancle",       this.IsCanbeCancle);
            DCFG.Write(doc, os, "IsCanbeSearch",       this.IsCanbeSearch);
            DCFG.Write(doc, os, "IsAutoPathFind",      this.IsAutoPathFind);
            DCFG.Write(doc, os, "IsFinishedTaskCount", this.IsFinishedTaskCount);
            DCFG.Write(doc, os, "IsAutoFinish",        this.IsAutoFinish);
            DCFG.Write(doc, os, "PreTaskID",           this.PreTaskID);
            for (int i = 0; i < this.SubTasks.Count; i++)
            {
                DCFG.Write(doc, os, "Item", SubTasks[i]);
            }
        }
    }
}
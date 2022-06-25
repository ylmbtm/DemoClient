using UnityEngine;
using System.Collections;
using System;
using System.Xml;
using System.Collections.Generic;

public class DTaskBranch : DObj<int>
{
    public int Id;
    public Int32  Task1;
    public Int32  Task2;
    public Int32  Task3;
    public Int32  Task4;
    public string TaskOpt1;
    public string TaskOpt2;
    public string TaskOpt3;
    public string TaskOpt4;

    public override int GetKey()
    {
        return Id;
    }

    public override void Read(XmlElement element)
    {
        this.Id       = element.GetInt32("Id");
        this.Task1    = element.GetInt32("Task1");
        this.Task2    = element.GetInt32("Task2");
        this.Task3    = element.GetInt32("Task3");
        this.Task4    = element.GetInt32("Task4");
        this.TaskOpt1 = element.GetString("TaskOpt1");
        this.TaskOpt2 = element.GetString("TaskOpt2");
        this.TaskOpt3 = element.GetString("TaskOpt3");
        this.TaskOpt4 = element.GetString("TaskOpt4");
    }
}

public class ReadCfgTaskBranch : DReadBase<int, DTaskBranch>
{

}
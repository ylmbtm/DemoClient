using UnityEngine;
using System.Collections;
using System;
using System.Xml;
using System.Collections.Generic;

public class DTaskBase : DObj<int>
{
    public Int32       Id;
    public string      Name;
    public Int32       TaskType;
    public Int32       LogicType;
    public Int32       LogicID;
    public Int32       CycleType;
    public Int32       ScriptID;
    public Int32       MinLevel;
    public List<int>   Requirements = new List<int>();
    public Int32       PrevTaskID;
    public Int32       NextTaskID;
    public Int32       ChapterID;
    public string      TraceText;
    public Int32       StAcceptDialogueID;
    public Int32       EdAcceptDialogueID;
    public Int32       StCommitDialogueID;
    public Int32       EdCommitDialogueID;
    public Int32       TargetNpcSceneID;
    public Int32       TargetNpcMutiPointID;
    public Int32       TargetNpcID;
    public Int32       TargetNpcInSky;
    public Int32       AcceptNpcSceneID;
    public Int32       AcceptNpcMutiPointID;
    public Int32       AcceptNpcID;
    public Int32       AcceptNpcInSky;
    public Int32       CommitNpcSceneID;
    public Int32       CommitNpcMutiPointID;
    public Int32       CommitNpcID;
    public Int32       CommitNpcInSky;
    public Int32       Money;
    public Int32       Exp;
    public Int32       TitleID;
    public List<EItem> Items;
    public bool        IsAutoSearch;
    public bool        IsAutoFinish;
    public bool        IsSaveFinishTaskCount;
    public bool        IsCanbeCancel;
    public bool        IsCanbeRepeated;

    public override int GetKey()
    {
        return Id;
    }

    public override void Read(XmlElement element)
    {
        this.Id                       = element.GetInt32("Id");
        this.Name                     = element.GetString("Name");
        this.TaskType                 = element.GetInt32("TaskType");
        this.LogicType                = element.GetInt32("LogicType");
        this.LogicID                  = element.GetInt32("LogicID");
        this.CycleType                = element.GetInt32("CycleType");
        this.ScriptID                 = element.GetInt32("ScriptID");
        this.MinLevel                 = element.GetInt32("MinLevel");

        this.Requirements             = element.GetListForInt("Requirements");
        this.PrevTaskID               = element.GetInt32("PrevTaskID");
        this.NextTaskID               = element.GetInt32("NextTaskID");
        this.ChapterID                = element.GetInt32("ChapterID");
        this.TraceText                = element.GetString("TraceText");

        this.StAcceptDialogueID       = element.GetInt32("StAcceptDialogueID");
        this.EdAcceptDialogueID       = element.GetInt32("EdAcceptDialogueID");
        this.StCommitDialogueID       = element.GetInt32("StCommitDialogueID");
        this.EdCommitDialogueID       = element.GetInt32("EdCommitDialogueID");

        this.TargetNpcSceneID         = element.GetInt32("TargetNpcSceneID");
        this.TargetNpcMutiPointID     = element.GetInt32("TargetNpcMutiPointID");
        this.TargetNpcID              = element.GetInt32("TargetNpcID");
        this.TargetNpcInSky           = element.GetInt32("TargetNpcInSky");

        this.AcceptNpcSceneID         = element.GetInt32("AcceptNpcSceneID");
        this.AcceptNpcMutiPointID     = element.GetInt32("AcceptNpcMutiPointID");
        this.AcceptNpcID              = element.GetInt32("AcceptNpcID");
        this.AcceptNpcInSky           = element.GetInt32("AcceptNpcInSky");

        this.CommitNpcSceneID         = element.GetInt32("CommitNpcSceneID");
        this.CommitNpcMutiPointID     = element.GetInt32("CommitNpcMutiPointID");
        this.CommitNpcID              = element.GetInt32("CommitNpcID");
        this.CommitNpcInSky           = element.GetInt32("CommitNpcInSky");

        this.Money                    = element.GetInt32("Money");
        this.Exp                      = element.GetInt32("Exp");
        this.TitleID                  = element.GetInt32("TitleID");
        this.Items                    = element.GetListForItem("Items");

        this.IsAutoSearch             = element.GetBool("IsAutoSearch");
        this.IsAutoFinish             = element.GetBool("IsAutoFinish");
        this.IsSaveFinishTaskCount    = element.GetBool("IsSaveFinishTaskCount");
        this.IsCanbeCancel            = element.GetBool("IsCanbeCancel");
        this.IsCanbeRepeated          = element.GetBool("IsCanbeRepeated");
    }
}

public class ReadCfgTaskBase : DReadBase<int, DTaskBase>
{

}
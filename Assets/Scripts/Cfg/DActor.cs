using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Xml;
using Protocol;

public class DActor : DObj<int>
{
    public int                            Id;
    public int                            Level;
    public string                         Name;
    public string                         Desc;
    public string                         Icon;
    public EActorRace                     Race;
    public EActorSex                      Sex;
    public EObjectType                    Type;
    public EActorSort                     Sort;
    public int                            Group;
    public int                            Quality;
    public int                            BornEffectID;
    public int                            DeadEffectID;
    public int                            Model;
    public int                            DialogID;
    public int                            BubbleID;
    public int                            AiID;
    public float                          DefSpeed;
    public List<bool>                     Natures   = new List<bool>();

    public override int GetKey()
    {
        return Id;
    }

    public override void Read(XmlElement element)
    {
        this.Id           = element.GetInt32("Id");
        this.Level        = element.GetInt32("Level");
        this.Name         = element.GetString("Name");
        this.Desc         = element.GetString("Desc");
        this.Icon         = element.GetString("Icon");
        this.Race         = (EActorRace)element.GetInt32("Race");
        this.Type         = (EObjectType)element.GetInt32("Type");
        this.Sex          = (EActorSex)element.GetInt32("Sex");
        this.Sort         = (EActorSort)element.GetInt32("Sort");
        this.Group        = element.GetInt32("Group");
        this.Quality      = element.GetInt32("Quality");
        this.BornEffectID = element.GetInt32("BornEffectID");
        this.DeadEffectID = element.GetInt32("DeadEffectID");
        this.DefSpeed     = element.GetFloat("DefSpeed");
        this.Model        = element.GetInt32("Model");
        this.DialogID     = element.GetInt32("DialogID");
        this.BubbleID     = element.GetInt32("BubbleID");
        this.AiID         = element.GetInt32("AiId");
        this.Natures.Add(element.GetInt32("CanMove") == 1);
        this.Natures.Add(element.GetInt32("CanKill") == 1);
        this.Natures.Add(element.GetInt32("CanManualAttack") == 1);
        this.Natures.Add(element.GetInt32("CanTurn") == 1);
        this.Natures.Add(element.GetInt32("CanStun") == 1);
        this.Natures.Add(element.GetInt32("CanBeatBack") == 1);
        this.Natures.Add(element.GetInt32("CanBeatFly") == 1);
        this.Natures.Add(element.GetInt32("CanBeatDown") == 1);
        this.Natures.Add(element.GetInt32("CanWound") == 1);
        this.Natures.Add(element.GetInt32("CanReduceSpeed") == 1);
        this.Natures.Add(element.GetInt32("CanFixBody") == 1);
        this.Natures.Add(element.GetInt32("CanSleep") == 1);
        this.Natures.Add(element.GetInt32("CanVaristion") == 1);
        this.Natures.Add(element.GetInt32("CanParaly") == 1);
        this.Natures.Add(element.GetInt32("CanFear") == 1);
    }
}


public class ReadCfgActor : DReadBase<int, DActor>
{
  
}
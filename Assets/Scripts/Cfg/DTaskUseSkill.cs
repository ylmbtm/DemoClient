using UnityEngine;
using System.Collections;
using System;
using System.Xml;

public class DTaskUseSkill : DObj<int>
{
    public int Id;
    public int Times;
    public int SkillIDPro1;
    public int SkillIDPro2;
    public int SkillIDPro3;
    public int SkillIDPro4;
    public int SkillIDPro5;

    public override int GetKey()
    {
        return Id;
    }

    public override void Read(XmlElement element)
    {
        this.Id          = element.GetInt32("Id");
        this.Times       = element.GetInt32("Times");
        this.SkillIDPro1 = element.GetInt32("SkillIDPro1");
        this.SkillIDPro2 = element.GetInt32("SkillIDPro2");
        this.SkillIDPro3 = element.GetInt32("SkillIDPro3");
        this.SkillIDPro4 = element.GetInt32("SkillIDPro4"); 
        this.SkillIDPro5 = element.GetInt32("SkillIDPro5");
    }
}

public class ReadCfgTaskUseSkill : DReadBase<int, DTaskUseSkill>
{

}
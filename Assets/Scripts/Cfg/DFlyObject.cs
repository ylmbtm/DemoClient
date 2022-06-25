using ACT;
using System;
using System.Collections.Generic;
using System.Xml;

public class DFlyOjbect : DObj<Int32>
{
    public Int32            ID= 0;
    public string           Name = string.Empty;
    public EFlyObjectType   Type = EFlyObjectType.TYPE_CHASE;
    public Int32            FlyingEffect = 0;
    public Int32            FlyingImpact = 0;
    public string           FlyingSound = string.Empty;
    public bool             FlyingMusicLoop = false;
    public string           FlyingImpactSound = string.Empty;
    public EBind            CasterBind = EBind.Body;
    public EBind            TargetBind = EBind.Body;
    public bool             PassBody = false;
    public ERangeType       RangeType = ERangeType.ERT_CIRCLE;
    public List<float>      RangeParams = new List<float>();

    public override Int32 GetKey()
    {
        return ID;
    }

    public override void Read(XmlElement element)
    {
        this.ID   = element.GetInt32("Id");
        this.Type = (EFlyObjectType)element.GetInt32("Type");
        this.Name = element.GetString("Name");
        this.FlyingEffect = element.GetInt32("FlyingEffect");
        this.FlyingImpact = element.GetInt32("FlyingImpact");
        this.FlyingSound = element.GetString("FlyingSound");
        this.FlyingImpactSound = element.GetString("FlyingImpactSound");
        this.FlyingSound = element.GetString("FlyingSound");
        this.FlyingImpactSound = element.GetString("FlyingImpactSound");

        this.RangeType = (ERangeType)element.GetInt32("RangeType");
        this.RangeParams = GTStringArrayHelper.GetFloatParams(element.GetString("RangeParams"));
    }
}

public class ReadCfgFlyObject : DReadBase<Int32, DFlyOjbect>
{

}

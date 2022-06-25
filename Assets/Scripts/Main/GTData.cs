using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Protocol;

public class XCharacter
{
    public int          Id;
    public ulong        GUID;
    public string       Name;
    public int          Sex;
    public int          Level;
    public int          Title;
    public int          ActorStatus;
    public EObjectType  ActorType;
    public int          Camp;
    public int          Group;
    public int          Mount;
    public int          Relic;
    public int          Guild;
    public int          VipLevel;
    public int          VipExp;
    public int          CurExp;
    public float        PosX;
    public float        PosY;
    public float        PosZ;
    public float        Face;
    public int          Carrer;
    public ulong        ControlID;
    public ulong        HostID;  //主人ID
    public EActionType  ActionID;  //动作ID
    public List<int> CurEquips = new List<int>();
    public List<int> CurAttrs = new List<int>();
    public List<SkillItem> CurSkills = new List<SkillItem>();
}

public class GTData : GTSingleton<GTData>
{
    public static int          CopyID;
    public static int          CopyGUID;
    public static ELanguage    Language;
    public static XCharacter   Main = new XCharacter();
    public static XCharacter   MainPartner1;
    public static XCharacter   MainPartner2;
    public static int          LastCityID;
    public static bool         IsLaunched;
    public static GTNativeData NativeData;

    public long                CurServerTime
    {
        get { return GTTools.GetUtcTime() - RecordStartTime; }
    }

    public long                RecordStartTime
    {
        get; private set;
    }

    public static ulong        NewGUID
    {
        get
        {
            GTData.NativeData.GUID++;
            return GTData.NativeData.GUID;
        }
    }

    private List<Int32> m_ActionValueList = new List<int>();
    private List<ulong> m_ActionTimeList = new List<ulong>();

    string GetDataPath(string xmlName)
    {
        string pPath = string.Format("{0}/Data/{1}", GTResourceManager.Instance.GetExtPath(), Main.GUID);
        if (!Directory.Exists(pPath))
        {
            Directory.CreateDirectory(pPath);
        }
        string path = string.Format("{0}/{1}", pPath, xmlName);
        return path;
    }

    string GetCommonDataPath(string xmlName)
    {
        string pPath = string.Format("{0}/Data", GTResourceManager.Instance.GetExtPath());
        if (!Directory.Exists(pPath))
        {
            Directory.CreateDirectory(pPath);
        }
        string path = string.Format("{0}/{1}", pPath, xmlName);
        return path;
    }

    public override void Init()
    {
        string key = System.IO.Directory.GetCurrentDirectory();
        string json = PlayerPrefs.GetString(key, string.Empty);
        if (string.IsNullOrEmpty(json))
        {
            NativeData = new GTNativeData();
        }
        else
        {
            NativeData = LitJson.JsonMapper.ToObject<GTNativeData>(json);
        }
    }

    public void   LoadCommonData()
    {
        DataDBSCharacter.Read(GetCommonDataPath(GTDataKey.Data_Roles), EDataKeyType.Carrer);
    }

    public int    MaxInstance
    {
        get { return NativeData.MaxInstance; }
        set { NativeData.MaxInstance = value; }
    }

    void ActionTickStart()
    {
         GTEventCenter.AddHandler(GTEventID.TYPE_TICK_SECOND, ActionTickUpdate);
    }

    void ActionTickExit()
    {
        GTEventCenter.DelHandler(GTEventID.TYPE_TICK_SECOND, ActionTickUpdate);
    }

    void ActionTickUpdate()
    {
         for (int i = 0; i < m_ActionValueList.Count; i++)
         {
             int key = m_ActionValueList[i];
         }
        //GTEventCenter.FireEvent(GTEventID.TYPE_CHANGE_ACTION, id);
    }
}
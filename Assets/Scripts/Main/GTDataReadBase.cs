using Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using UnityEngine;


public class GTDataBase<TKey,TVal>
{
	public static Dictionary<TKey, TVal> Dict = new Dictionary<TKey, TVal>();

	public static void AddItem(TKey guid, TVal item)
    {
        if (Dict.ContainsKey(guid))
        {
            Dict[guid] = item;
        }
        else
        {
            Dict.Add(guid, item);
        }
    }

	public static void RemoveItem(TKey guid)
    {
        if (Dict.ContainsKey(guid))
        {
            Dict.Remove(guid);
        }
    }

	public static TVal GetItem(TKey guid)
    {
        if (!Dict.ContainsKey(guid))
        {
            return default(TVal);
        }

        return Dict[guid];
    }

    public static bool HasItem(TKey guid)
    {
        if (Dict.ContainsKey(guid))
        {
            return true;
        }

        return false;
    }

    public static int GetCount()
    {
        return Dict.Count;
    }
}


public sealed class GTBagData : GTDataBase<ulong,BagItem>
{
    public static  ulong GetItemCount(int itemid)
    {
        ulong nCount = 0;

        foreach (var item in Dict.Values)
        {
            if (item.ItemID == itemid)
            {
                nCount += (ulong)item.ItemNum;
            }
        }

        return nCount;
    }
}

public sealed class GTPartnerData : GTDataBase<ulong,PartnerItem> 
{
	public static ulong[] m_setupPartner = new ulong[2]{0,0};
}
public sealed class GTEquipData : GTDataBase<ulong,EquipItem> { }
public sealed class GTPetData : GTDataBase<ulong,PetItem>
{
    public static ulong m_setupPet = 0;
}
public sealed class GTMountData : GTDataBase<ulong,MountItem>
{
    public static ulong m_setupMount = 0;
}
public sealed class GTMailData : GTDataBase<ulong,MailItem> { }
public sealed class GTGemData : GTDataBase<ulong,GemItem> { }
public sealed class GTTaskData : GTDataBase<ulong,TaskItem> { }
public sealed class GTActionData : GTDataBase<int, ActionItem> { }
public sealed class GTCopyData : GTDataBase<int, CopyItem> { }
public sealed class GTChapterData : GTDataBase<int, ChapterItem> { }
public sealed class GTSkillData : GTDataBase<uint, SkillItem> { }

public class GTDataReadBase<TType, TVal>
{
    public static string                    XmlPath = string.Empty;
    public static EDataKeyType              KeyType = EDataKeyType.Id;
    public static Dictionary<int, TVal>     Dict    = new Dictionary<int, TVal>();
   

    public static void Read(string path, EDataKeyType keyType)
    {
        Clear();
        XmlPath = path;
        KeyType = keyType;
        XmlNodeList nodeList = GTXmlHelper.GetXmlNodeList(XmlPath);
        for (int i = 0; i < nodeList.Count; i++)
        {
            XmlElement xe = nodeList.Item(i) as XmlElement;
            if (xe == null)
            {
                continue;
            }
            int key = xe.GetAttribute(KeyType.ToString()).ToInt32();
            for (int j = 0; j < xe.Attributes.Count; j++)
            {
                string name = xe.Attributes[j].Name;
                string value = xe.Attributes[j].Value;
                AppendAttribute(key, name, value);
            }
        }
    }

    public static void AppendAttribute(int key, string name, string value)
    {
        if (Dict == null)
        {
            return;
        }
        TVal obj = default(TVal);
        Dict.TryGetValue(key, out obj);
        if (obj == null)
        {
            obj = Activator.CreateInstance<TVal>();
            Dict.Add(key, obj);
        }
        PropertyInfo[] fields = obj.GetType().GetProperties();
        for (int i = 0; i < fields.Length; i++)
        {
            PropertyInfo field = fields[i];
            if (field.Name == name)
            {
                Parse(ref field, obj, value);
                break;
            }
        }
    }

    public static void Insert(int key, TVal obj)
    {
        Dict[key] = obj;
        GTXmlHelper.Append(XmlPath, key.ToString(), obj, KeyType);
    }

    public static void Delete(int key)
    {
        Dict.Remove(key);
        GTXmlHelper.Delete(XmlPath, key.ToString(), KeyType);
    }

    public static void Update(int key, TVal obj)
    {
        if (Dict.ContainsKey(key))
        {
            GTXmlHelper.Update(XmlPath, key.ToString(), obj, KeyType);
        }
        else
        {
            Insert(key, obj);
        }
    }

    public static TVal GetDataById(int key)
    {
        TVal obj = default(TVal);
        Dict.TryGetValue(key, out obj);
        return obj;
    }

    public static void Clear()
    {
        Dict.Clear();
    }

    public static void ClearXml()
    {
        GTXmlHelper.ClearAll(XmlPath);
        Dict.Clear();
    }

    public static void Parse(ref PropertyInfo field, object obj, string value)
    {
        Type fieldType = field.PropertyType;
        if (fieldType      == typeof(Int32))
        {
            field.SetValue(obj, value.ToInt32(), null);
        }
        else if (fieldType == typeof(UInt16))
        {
            field.SetValue(obj, value.ToUInt16(), null);
        }
        else if(fieldType  == typeof(UInt32))
        {
            field.SetValue(obj, value.ToUInt32(), null);
        }
        else if (fieldType == typeof(UInt64))
        {
            field.SetValue(obj, value.ToUInt64(), null);
        }
        else if (fieldType == typeof(string))
        {
            field.SetValue(obj, value, null);
        }
        else if (fieldType == typeof(Vector3))
        {
            field.SetValue(obj, value.ToVector3(), null);
        }
        else if (fieldType == typeof(float))
        {
            field.SetValue(obj, value.ToFloat(), null);
        }
    }

    public static bool ContainsKey(int key)
    {
        return Dict.ContainsKey(key);
    }
}

public sealed class DataDBSGuide : GTDataReadBase<DataDBSGuide, GuideItem> { }
public sealed class DataDBSCharacter :    GTDataReadBase<DataDBSCharacter, XCharacter> { }
public sealed class DataDBSRelics :       GTDataReadBase<DataDBSRelics, XRelics> { }


using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GTAwardHelper
{
    public static List<EItem> GetAwardDataByID(int awardID)
    {
        List<EItem> list = new List<EItem>();
        DAward db = ReadCfgAward.GetDataById(awardID);
        if (db == null)
        {
            return null;
        }
        switch (db.DropType)
        {
            case EDropType.MULFIX:
                CalcMulFixItem(ref list, db.DropItems);
                break;
            case EDropType.ONERAN:
                CalcOneRanItem(ref list, db.DropItems);
                break;
            case EDropType.MULRAN:
                CalcMulFixItem(ref list, db.DropItems);
                break;
        }
        return list;
    }

    static void CalcOneFixItem(ref List<EItem> list, string s)
    {
        string[] array = s.Split('|');
        EItem m = new EItem(array[0].ToInt32(), array[1].ToInt32());
        list.Add(m);
    }

    static void CalcMulFixItem(ref List<EItem> list, string s)
    {
        string str = s.TrimStart('(').TrimEnd(')').Replace(")(", "#");
        string[] array = str.Split('#');
        for (int i = 0; i < array.Length; i++)
        {
            string[] a = array[i].Split('|');
            EItem m = new EItem(a[0].ToInt32(), a[1].ToInt32());
            list.Add(m);
        }
    }

    static void CalcOneRanItem(ref List<EItem> list, string s)
    {
        string[] idValue = s.Split('&');
        EItem m = new EItem();
        int random = UnityEngine.Random.Range(1, 101);
        string str = s.TrimStart('(').TrimEnd(')').Replace(")(", "#");
        string[] array = str.Split('#');
        for (int i = 0; i < array.Length; i++)
        {
            string[] a1 = array[i].Split('|');
            string[] a2 = a1[0].Split('-');
            if (random >= a2[0].ToInt32() && random <= a2[1].ToInt32())
            {
                m.Id = idValue[0].ToInt32();
                m.Num = a1[1].ToInt32();
                list.Add(m);
                break;
            }
        }
    }
}

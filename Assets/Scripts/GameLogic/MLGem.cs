using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Protocol;

public class MLGem : GTSingleton<MLGem>
{
    private Dictionary<int, List<int>> mGemSuits;
    private HashSet<int>               mStrengHashSet;
    private Dictionary<int, int>       mGemIndexDict;

    public MLGem()
    {
        mGemSuits = new Dictionary<int, List<int>>();
        mStrengHashSet = new HashSet<int>()
        {
            1031,1032,1033,1034,1035,
        };
        mGemIndexDict = new Dictionary<int, int>();
        for (int i = 1; i <= 40; i++)
        {
            int value = (i % 5 != 0) ? (i + 5 - i % 5) / 5 : i / 5;
            mGemIndexDict.Add(i, value);
        }
    }

    public string GetGemTypeName(int itemID)
    {
        DGem db = ReadCfgGem.GetDataById(itemID);
        switch (db.Pos)
        {
            case 1:
                return "金";
            case 2:
                return "木";
            case 3:
                return "水";
            case 4:
                return "火";
            case 5:
                return "土";
            default:
                return "空";
        }
    }

    public string GetGemSuitName(int itemID)
    {
        DGem gemDB = ReadCfgGem.GetDataById(itemID);
        if (gemDB.Suit == 0)
        {
            return string.Empty;
        }
        return ReadCfgGemSuit.GetDataById(gemDB.Suit).Name;
    }

    public List<int> GetSameSuitIDListByID(int itemID)
    {
        DGem gemDB = ReadCfgGem.GetDataById(itemID);

        if (gemDB.Suit == 0)
        {
            return new List<int>();
        }
        if (mGemSuits.ContainsKey(gemDB.Suit))
        {
            return mGemSuits[gemDB.Suit];
        }
        Dictionary<int, DGem>.Enumerator em = ReadCfgGem.Dict.GetEnumerator();
        while (em.MoveNext())
        {
            DGem db = em.Current.Value;
            List<int> list;
            if (mGemSuits.ContainsKey(db.Suit))
            {
                list = mGemSuits[db.Suit];
            }
            else
            {
                list = new List<int>();
                mGemSuits.Add(db.Suit, list);
            }
            if (!list.Contains(db.Id))
            {
                list.Add(db.Id);
            }
        }
        if (mGemSuits.ContainsKey(gemDB.Suit))
        {
            return mGemSuits[gemDB.Suit];
        }
        else
        {
            return new List<int>();
        }

    }
}

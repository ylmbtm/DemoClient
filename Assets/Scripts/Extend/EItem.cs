using UnityEngine;
using System.Collections;

public struct EItem
{
    public int Id;
    public int Num;

    public EItem(int id, int num)
    {
        this.Id = id;
        this.Num = num;
    }

    public string Read(string s)
    {
        string[] childArray = s.Split('(', ',', ')');
        Id                = childArray[1].ToInt32();
        Num               = childArray[2].ToInt32();
        return s;
    }

    public string Save()
    {
        return string.Format("({0},{2})", Id, Num);
    }
}
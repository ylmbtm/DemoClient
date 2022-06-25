using UnityEngine;
using System.Collections;

public class DStrengthValue
{
    public int        PropertyID;
    public int        Value;
    public int        UnlockLevel;

    public DStrengthValue(int id, int v, int l)
    {
        PropertyID    = id;
        Value         = v;
        UnlockLevel   = l;
    }

    public DStrengthValue()
    {

    }
}

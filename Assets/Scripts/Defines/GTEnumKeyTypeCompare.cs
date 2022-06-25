using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GTEventTypeComparer : IEqualityComparer<GTEventID>
{
    public bool Equals(GTEventID x, GTEventID y)
    {
        return  x == y;
    }

    public int GetHashCode(GTEventID obj)
    {
        return (int)obj;
    }
}

public class GTFSMStateTypeComparer : IEqualityComparer<FSMState>
{
    public bool Equals(FSMState x, FSMState y)
    {
        return x == y;
    }

    public int GetHashCode(FSMState obj)
    {
        return (int)obj;
    }
}

public class GTCopyTypeTypeComparer : IEqualityComparer<ECopyType>
{
    public bool Equals(ECopyType x, ECopyType y)
    {
        return x == y;
    }

    public int GetHashCode(ECopyType obj)
    {
        return (int)obj;
    }
}
    
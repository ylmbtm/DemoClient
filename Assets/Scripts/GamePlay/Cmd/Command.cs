using UnityEngine;
using System.Collections;
using System;

public class Command : ICommand
{
    public override Resp Do()
    {
        return Resp.TYPE_NO;
    }
}
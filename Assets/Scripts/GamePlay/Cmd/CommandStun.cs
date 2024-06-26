﻿using UnityEngine;
using System.Collections;

/// <summary>
/// 昏迷
/// </summary>
public class CommandStun : Command
{
    public float LastTime;

    public override Resp Do()
    {
        CmdHandler<CommandStun> call = Del as CmdHandler<CommandStun>;
        return call == null ? Resp.TYPE_NO : call(this);
    }

    public CommandStun Update(float lastTime)
    {
        this.LastTime = lastTime;
        return this;
    }
}


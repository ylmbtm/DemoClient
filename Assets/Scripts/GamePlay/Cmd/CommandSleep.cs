using UnityEngine;
using System.Collections;

/// <summary>
/// 睡眠
/// </summary>
public class CommandSleep : Command
{
    public float LastTime;

    public override Resp Do()
    {
        CmdHandler<CommandSleep> call = Del as CmdHandler<CommandSleep>;
        return call == null ? Resp.TYPE_NO : call(this);
    }

    public CommandSleep Update(float lastTime)
    {
        this.LastTime = lastTime;
        return this;
    }
}


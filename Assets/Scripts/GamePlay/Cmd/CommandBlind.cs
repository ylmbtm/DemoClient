using UnityEngine;
using System.Collections;

/// <summary>
/// 致盲
/// </summary>
public class CommandBlind : Command
{
    public float LastTime;

    public override Resp Do()
    {
        CmdHandler<CommandBlind> call = Del as CmdHandler<CommandBlind>;
        return call == null ? Resp.TYPE_NO : call(this);
    }

    public CommandBlind Update(float lastTime)
    {
        this.LastTime = lastTime;
        return this;
    }
}

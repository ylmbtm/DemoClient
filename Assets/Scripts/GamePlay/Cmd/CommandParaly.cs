using UnityEngine;
using System.Collections;

/// <summary>
/// 麻痹
/// </summary>
public class CommandParaly : Command
{
    public float LastTime;

    public override Resp Do()
    {
        CmdHandler<CommandParaly> call = Del as CmdHandler<CommandParaly>;
        return call == null ? Resp.TYPE_NO : call(this);
    }

    public CommandParaly Update(float lastTime)
    {
        this.LastTime = lastTime;
        return this;
    }
}
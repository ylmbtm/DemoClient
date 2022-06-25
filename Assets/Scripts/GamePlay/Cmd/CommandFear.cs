using UnityEngine;
using System.Collections;


/// <summary>
/// 恐惧
/// </summary>
public class CommandFear : Command
{
    public float LastTime;

    public override Resp Do()
    {
        CmdHandler<CommandFear> call = Del as CmdHandler<CommandFear>;
        return call == null ? Resp.TYPE_NO : call(this);
    }

    public CommandFear Update(float lastTime)
    {
        this.LastTime = lastTime;
        return this;
    }
}

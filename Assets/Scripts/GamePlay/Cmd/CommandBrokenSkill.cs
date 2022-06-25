using UnityEngine;
using System.Collections;

public class CommandBrokenSkill : Command
{
    public override Resp Do()
    {
        CmdHandler<CommandBrokenSkill> call = Del as CmdHandler<CommandBrokenSkill>;
        return call == null ? Resp.TYPE_NO : call(this);
    }
}

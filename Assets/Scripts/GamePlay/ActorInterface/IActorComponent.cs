using UnityEngine;
using System.Collections;

public interface IActorComponent
{
    void Initial(Actor actor);

    void Execute();

    void Release();
}

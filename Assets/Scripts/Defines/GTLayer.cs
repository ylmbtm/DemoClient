using UnityEngine;
using System.Collections;

public class GTLayer
{
    public const int LAYER_DEFAULT       = 0;
    public const int LAYER_UI            = 5;

    public const int LAYER_PLAYER        = 8;
    public const int LAYER_PET           = 9;
    public const int LAYER_ACTOR         = 10;
    public const int LAYER_MINE          = 11;
    public const int LAYER_BARRER        = 12;
    public const int LAYER_MONSTER       = 13;
    public const int LAYER_MINI          = 14;
    public const int LAYER_TOUCHEFFECT   = 15;

    public const int LAYER_RENDER_START  = 28;
    public const int LAYER_CAMERARAYCAST = 1 << LAYER_DEFAULT + 1 << LAYER_ACTOR;
}

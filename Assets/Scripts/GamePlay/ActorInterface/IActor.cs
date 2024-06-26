﻿using UnityEngine;
using System.Collections;

public interface IActor
{
    int        ID             { get; set; }
    ulong      GUID           { get; set; }
    Vector3    Pos            { get; set; }
    Vector3    Euler          { get; set; }
    Vector3    Scale          { get; set; }
    Transform  CacheTransform { get; }
}

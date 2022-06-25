using UnityEngine;
using System.Collections;
using Protocol;

public struct ETransform
{
    public Vector3 Pos;
    public Vector3 Euler;

    static ETransform()
    {
        Default = new ETransform();
    }

    public static ETransform Create(Vector3 pos, Vector3 angle)
    {
        ETransform data = new ETransform();
        data.Pos = pos;
        data.Euler = angle;
        return data;
    }

    public static ETransform Create(Vector3 pos, Vector3 angle, Vector3 scale)
    {
        ETransform data = new ETransform();
        data.Pos = pos;
        data.Euler = angle;
        return data;
    }

    public static ETransform Create(Vector3 pos, float face)
    {
        ETransform data = new ETransform();
        data.Pos = pos;
        data.Euler = new Vector3(0, face, 0);
        return data;
    }

    public static ETransform Create(Vector3 pos, float vx, float vy, float vz)
    {
        ETransform data = new ETransform();
        data.Pos = pos;
        data.Euler = Quaternion.FromToRotation(Vector3.forward, new Vector3(vx, vy, vz)).eulerAngles;
        return data;
    }

    public static ETransform Create(float x, float y, float z,float face)
    {
        ETransform data = new ETransform();
        data.Pos   = new Vector3(x, y, z);
        data.Euler = new Vector3(0, face, 0);
        return data;
    }

    public static ETransform Create(float x, float y, float z, float vx, float vy, float vz)
    {
        ETransform data = new ETransform();
        data.Pos   = new Vector3(x, y, z);
        data.Euler = Quaternion.FromToRotation(Vector3.forward, new Vector3(vx, vy, vz)).eulerAngles;
        return data;
    }

    public static ETransform Default { get; private set; }
}

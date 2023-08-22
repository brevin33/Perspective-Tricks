using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct VertInfo
{
    public Vector3 pos;
    public Vector3 normal;
    public Vector2 uv;
    public Vector2 uv2;
    public Vector4 tangent;
}


public struct posAndIndex
{
    public Vector3 pos;
    public int index;
}

public struct edge { 
    public int vert1;
    public int vert2;
}
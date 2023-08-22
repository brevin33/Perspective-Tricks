using System;
using UnityEngine;

public class Silhouette
{

    string n;
    Vector3 viewPos;
    public int[,] silouetEdges;
    int[,,] edgeToTrianlge;
    public Silhouette(string Name, Vector3 ViewPos)
    {
        n = Name;
        viewPos = ViewPos;
    }

    public void setSilouetEdges(Mesh mesh)
    {
        makeEdgeToTrianlge(mesh);
        Vector3[] verts = mesh.vertices;
        int[] trianlges = mesh.triangles;
        silouetEdges = new int[trianlges.Length, 2];
        int silouetEdgeIndex = 0;
        for (int i = 0; i < trianlges.Length; i += 3)
        {
            int[,] edges = new int[,] { { trianlges[i], trianlges[i + 1] }, { trianlges[i], trianlges[i + 1] }, { trianlges[i + 1], trianlges[i + 2] } };
            int[,] edgesIndex = new int[,] { { i, i + 1 }, { i, i + 1 }, { i + 1, i + 2 } };
            for (int j = 0; j < edges.Length; j++)
            {
                int trianlge1Start = edgeToTrianlge[edges[j, 0], edges[j, 1], 0];
                int trianlge2Start = edgeToTrianlge[edges[j, 0], edges[j, 1], 1];
                bool triangle1CanSeeViewPoint = trianlgeCanSeeViewPos(trianlges[trianlge1Start], trianlges[trianlge1Start + 1], trianlges[trianlge1Start + 2], verts);
                bool triangle2CanSeeViewPoint = trianlgeCanSeeViewPos(trianlges[trianlge2Start], trianlges[trianlge2Start + 1], trianlges[trianlge2Start + 2], verts);
                if (triangle1CanSeeViewPoint && triangle2CanSeeViewPoint)
                {
                    continue;
                }
                if (!triangle1CanSeeViewPoint && !triangle2CanSeeViewPoint)
                {
                    continue;
                }
                silouetEdges[silouetEdgeIndex, 0] = edgesIndex[j, 0];
                silouetEdges[silouetEdgeIndex, 1] = edgesIndex[j, 1];
                silouetEdgeIndex++;
            }
        }
        ResizeArray(silouetEdges,silouetEdgeIndex,2);
    }

    T[,] ResizeArray<T>(T[,] original, int rows, int cols)
    {
        var newArray = new T[rows, cols];
        int minRows = Math.Min(rows, original.GetLength(0));
        int minCols = Math.Min(cols, original.GetLength(1));
        for (int i = 0; i < minRows; i++)
            for (int j = 0; j < minCols; j++)
                newArray[i, j] = original[i, j];
        return newArray;
    }


    void makeEdgeToTrianlge(Mesh mesh)
    {
        Vector3[] verts = mesh.vertices;
        int[] trianlges = mesh.triangles;
        edgeToTrianlge = new int[verts.Length, verts.Length, 2];
        for (int i = 0; i < verts.Length; i++)
        {
            for (int j = 0; j < verts.Length; j++)
            {
                edgeToTrianlge[i, j, 0] = -1;
            }
        }

        for (int i = 0; i < trianlges.Length; i += 3)
        {
            int[,] edges = new int[,] { { trianlges[i], trianlges[i + 1] }, { trianlges[i], trianlges[i + 1] }, { trianlges[i + 1], trianlges[i + 2] } };
            for (int j = 0; j < edges.Length; j++)
            {
                edgeToTrianlge[edges[j, 0], edges[j, 1], 0] = edgeToTrianlge[i, j, 0] != -1 ? edgeToTrianlge[i, j, 0] : i;
                edgeToTrianlge[edges[j, 1], edges[j, 0], 0] = edgeToTrianlge[i, j, 0] != -1 ? edgeToTrianlge[i, j, 0] : i;
                edgeToTrianlge[edges[j, 0], edges[j, 1], 1] = i;
                edgeToTrianlge[edges[j, 1], edges[j, 0], 1] = i;

            }
        }
    }

    bool trianlgeCanSeeViewPos(int v1, int v2, int v3, Vector3[] verts)
    {
        Vector3 point = (verts[v1] + verts[v2] + verts[v3]) / 3;
        if (Physics.Raycast(point, viewPos - point, out RaycastHit hit))
        {
            return Vector3.Distance(hit.point, point) >= Vector3.Distance(viewPos, point);
        }
        return true;
    }

    public bool pointBehind(Vector3 point)
    {
        RaycastHit hit;
        if (Physics.Raycast(point, viewPos - point, out hit))
        {
            return hit.transform.gameObject.name == n;
        }
        return false;
    }

    public Vector3 findPointInSilhouetteAlongEdge(Vector3 inSilhouette, Vector3 outSilhouette, int accuracy)
    {
        float inBetween = 0.5f;
        float change = 0.25f;
        Vector3 point = Vector3.Lerp(inSilhouette, outSilhouette, inBetween);
        for (int i = 0; i < accuracy; i++)
        {
            if (pointBehind(point))
            {
                inBetween += change;
            }
            else
            {
                inBetween -= change;
            }
            change *= .5f;
            point = Vector3.Lerp(inSilhouette, outSilhouette, inBetween);
        }

        return point;
    }

    public void setVertInfoUVs(ref VertInfo vertInfo)
    {
        RaycastHit hit;
        if (Physics.Raycast(viewPos, vertInfo.pos - viewPos, out hit))
        {
            vertInfo.uv = hit.textureCoord;
            vertInfo.uv2 = hit.textureCoord2;
        }
    }

    public VertInfo findVertInfoInSilhouetteAlongEdge(VertInfo inSilhouette, VertInfo outSilhouette, int accuracy)
    {
        float inBetween = 0.5f;
        float change = 0.25f;
        Vector3 inSilhouettePoint = inSilhouette.pos;
        Vector3 outSilhouettePoint = outSilhouette.pos;
        Vector3 point = Vector3.Lerp(inSilhouettePoint, outSilhouettePoint, inBetween);
        Vector3 outPoint = point;
        for (int i = 0; i < accuracy; i++)
        {
            if (pointBehind(point))
            {
                inBetween += change;
                outPoint = point;
            }
            else
            {
                inBetween -= change;
            }
            change *= .5f;
            point = Vector3.Lerp(inSilhouettePoint, outSilhouettePoint, inBetween);
        }
        outPoint = point;
        return new VertInfo
        {
            pos = outPoint,
            normal = Vector3.Lerp(inSilhouette.normal, outSilhouette.normal, inBetween),
            tangent = Vector4.Lerp(inSilhouette.tangent, outSilhouette.tangent, inBetween)
        };
    }



}

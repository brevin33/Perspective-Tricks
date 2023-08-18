using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class SplatAble : MonoBehaviour
{

    // this is unfinished

    MeshFilter meshFilter;
    Mesh mesh;
    Vector3[] verts;
    int[] triangles;

    [SerializeField]
    Camera cam;

    [SerializeField]
    LayerMask splatOnto;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = meshFilter.mesh;
        verts = mesh.vertices;
        triangles = mesh.triangles;
        string[] vertHits = new string[verts.Length];
    }
    private void OnMouseDown()
    {
        Vector3[] newVerts = new Vector3[verts.Length * 50];
        int[] newTriangles = new int[triangles.Length * 50];
        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 vertWorld = transform.TransformPoint(verts[i]);
            Vector3 vertLocal = verts[i];
            Vector3 newPos = transformVertToWall(vertWorld,vertLocal);
            newVerts[i] = newPos;
        }
        int newTrinaglesIndex = 0;
        int newVertexIndex = verts.Length;
        for (int i = 0; i + 2 < triangles.Length; i += 3)
        {
            int[] tri = new int[4]; 
            tri[1] = triangles[i + 0];
            tri[2] = triangles[i + 1];
            tri[3] = triangles[i + 2];
            Vector3 vertWorld1 = transform.TransformPoint(newVerts[tri[1]]);
            Vector3 vertWorld2 = transform.TransformPoint(newVerts[tri[2]]);
            Vector3 vertWorld3 = transform.TransformPoint(newVerts[tri[3]]);
            Vector3 vertWorldOld1 = transform.TransformPoint(verts[tri[1]]);
            Vector3 vertWorldOld2 = transform.TransformPoint(verts[tri[2]]);
            Vector3 vertWorldOld3 = transform.TransformPoint(verts[tri[3]]);

            int[][] newPointSplatIndex = new int[4][];
            for (int j = 1; j < 4; j++)
            {
                newPointSplatIndex[j] = new int[4];
                for (int k = 1; k < 4; k++)
                {
                    newPointSplatIndex[j][k] = -1;
                }
            }


            Vector3 newPoint12 = Vector3.negativeInfinity;
            Vector3 newPoint21 = Vector3.negativeInfinity;
            Vector3 newPoint12Local = Vector3.negativeInfinity;
            Vector3 newPoint21Local = Vector3.negativeInfinity;
            Vector3 newPoint12Splat = Vector3.negativeInfinity;
            Vector3 newPoint21Splat = Vector3.negativeInfinity;

            Vector3 newPoint23 = Vector3.negativeInfinity;
            Vector3 newPoint32 = Vector3.negativeInfinity;
            Vector3 newPoint23Local = Vector3.negativeInfinity;
            Vector3 newPoint32Local = Vector3.negativeInfinity;
            Vector3 newPoint23Splat = Vector3.negativeInfinity;
            Vector3 newPoint32Splat = Vector3.negativeInfinity;

            Vector3 newPoint13 = Vector3.negativeInfinity;
            Vector3 newPoint31 = Vector3.negativeInfinity;
            Vector3 newPoint13Local = Vector3.negativeInfinity;
            Vector3 newPoint31Local = Vector3.negativeInfinity;
            Vector3 newPoint13Splat = Vector3.negativeInfinity;
            Vector3 newPoint31Splat = Vector3.negativeInfinity;

            RaycastHit hit;
            if (Physics.Raycast(vertWorld1,vertWorld2, out hit,splatOnto))
            {

                float lerpValue =  binaryFindEdge(vertWorldOld1, vertWorldOld2, vertWorld1, vertWorld2);
                newPoint12 = Vector3.Lerp(vertWorldOld1,vertWorldOld2,lerpValue + 0.1f);
                newPoint21 = Vector3.Lerp(vertWorldOld1, vertWorldOld2, lerpValue - 0.1f);
                newPoint12Local = transform.InverseTransformPoint(newPoint12);
                newPoint21Local = transform.InverseTransformPoint(newPoint21);
                Debug.Log(lerpValue);
                newPoint12Splat =  transformVertToWall(newPoint12,newPoint12Local);
                newPoint21Splat = transformVertToWall(newPoint21, newPoint21Local);
                newVerts[newVertexIndex] = newPoint12Splat;
                newPointSplatIndex[1][2] = newVertexIndex;
                newVertexIndex++;
                newVerts[newVertexIndex] = newPoint21Splat;
                newPointSplatIndex[2][1] = newVertexIndex;
                newVertexIndex++;

            }
            if (Physics.Linecast(vertWorld2, vertWorld3, out hit, splatOnto))
            {

                float lerpValue = binaryFindEdge(vertWorldOld2, vertWorldOld3, vertWorld2, vertWorld3);
                newPoint23 = Vector3.Lerp(vertWorldOld2, vertWorldOld3, lerpValue + 0.1f);
                newPoint32 = Vector3.Lerp(vertWorldOld2, vertWorldOld3, lerpValue - 0.1f);
                newPoint23Local = transform.InverseTransformPoint(newPoint23);
                newPoint32Local = transform.InverseTransformPoint(newPoint32);
                newPoint23Splat = transformVertToWall(newPoint23, newPoint23Local);
                newPoint32Splat = transformVertToWall(newPoint32, newPoint32Local);
                newVerts[newVertexIndex] = newPoint23Splat;
                newPointSplatIndex[2][3] = newVertexIndex;
                newVertexIndex++;
                newVerts[newVertexIndex] = newPoint32Splat;
                newPointSplatIndex[3][2] = newVertexIndex;
                newVertexIndex++;
            }
            if (Physics.Linecast(vertWorld1, vertWorld3, out hit, splatOnto))
            {

                float lerpValue = binaryFindEdge(vertWorldOld1, vertWorldOld3, vertWorld1, vertWorld3);
                newPoint13 = Vector3.Lerp(vertWorldOld1, vertWorldOld3, lerpValue + 0.1f);
                newPoint31 = Vector3.Lerp(vertWorldOld1, vertWorldOld3, lerpValue - 0.1f);
                newPoint13Local = transform.InverseTransformPoint(newPoint13);
                newPoint31Local = transform.InverseTransformPoint(newPoint31);
                newPoint13Splat = transformVertToWall(newPoint13, newPoint13Local);
                newPoint31Splat = transformVertToWall(newPoint31, newPoint31Local);
                newVerts[newVertexIndex] = newPoint13Splat;
                newPointSplatIndex[1][3] = newVertexIndex; 
                newVertexIndex++;
                newVerts[newVertexIndex] = newPoint31Splat;
                newPointSplatIndex[3][1] = newVertexIndex;
                newVertexIndex++;
            }

            // connect new splat point if they exist
            bool hasNewVerts = false;

            for (int from = 1; from < 4; from++)
            {
                for (int to = 1; to < 4; to++)
                {
                    if(from == to)
                    {
                        continue;
                    }
                    int vertIndex = newPointSplatIndex[from][to];
                    if (vertIndex == -1)
                    {
                        continue;
                    }
                    hasNewVerts = true;
                    int homeVertIndex = tri[from];
                    int awayIndex = getAwayIndex(to,from);
                    int otherWayVertIndex = newPointSplatIndex[awayIndex][to] != -1 ? newPointSplatIndex[awayIndex][to] : tri[awayIndex];
                    newTriangles[newTrinaglesIndex] = vertIndex;
                    newTriangles[newTrinaglesIndex + 1] = homeVertIndex;
                    newTriangles[newTrinaglesIndex + 2] = otherWayVertIndex;
                    newTrinaglesIndex += 3;
                }
            }

            if (!hasNewVerts)
            {
                newTriangles[newTrinaglesIndex] = tri[1];
                newTriangles[newTrinaglesIndex + 1] = tri[2]; 
                newTriangles[newTrinaglesIndex + 2] = tri[3]; 
                newTrinaglesIndex += 3;
            }
        }
        verts = newVerts;
        triangles = newTriangles;
        mesh.vertices = verts;
        mesh.triangles = triangles;
    }


    int getAwayIndex(int to, int home)
    {
        if (to + home == 5) {
            return 1;
        }
        else if (to + home == 3)
        {
            return 3;
        }
        else if (to + home == 4)
        {
            return 2;
        }
        return -1;
    }

    float binaryFindEdge(Vector3 vert1, Vector3 vert2, Vector3 vert1Splat, Vector3 vert2Splat)
    {
        float lastLerpValue = 0f;
        float lerpValue = 0.5f;
        for (int i = 2; i < 22 && lerpValue != lastLerpValue; i++)
        {
            lastLerpValue = lerpValue;
            RaycastHit hit;
            RaycastHit hit2;
            if (Physics.Raycast(cam.transform.position, Vector3.Lerp(vert1, vert2, lerpValue) - cam.transform.position, out hit, 99, splatOnto))
            {
                if(Physics.Linecast(vert2Splat,  hit.point, out hit2, splatOnto))
                {
                    lerpValue += Mathf.Pow(.5f,i);
                }
                else if(Physics.Linecast(vert1Splat, hit.point, out hit2, splatOnto))
                {
                    lerpValue -= Mathf.Pow(.5f, i);
                }
            }
        }
        return lerpValue;
    }



    public Vector3 transformVertToWall(Vector3 worldPos, Vector3 localPos)
    {
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, worldPos - cam.transform.position, out hit, 99, splatOnto))
        {
            Vector3 newPos = transform.InverseTransformPoint(hit.point);
            newPos = Vector3.Lerp(localPos, newPos, .999f);
            return newPos;
        }
        return Vector3.negativeInfinity;
    }


}
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class SplatAble : MonoBehaviour
{

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
        Vector3[] newVerts = new Vector3[verts.Length * 3];
        int[] newTriangles = new int[triangles.Length * 3];
        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 vertWorld = transform.TransformPoint(verts[i]);
            Vector3 vertLocal = verts[i];
            Vector3 newPos = transformVertToWall(vertWorld,vertLocal);
            newVerts[i] = newPos;
        }
        int newTrinaglesIndex = 0;
        int newVertexIndex = 0;
        for (int i = 0; i + 2 < triangles.Length; i += 3)
        {
            int tri1 = triangles[i + 0];
            int tri2 = triangles[i + 1];
            int tri3 = triangles[i + 2];
            Vector3 vertWorld1 = transform.TransformPoint(newVerts[i + 0]);
            Vector3 vertWorld2 = transform.TransformPoint(newVerts[i + 1]);
            Vector3 vertWorld3 = transform.TransformPoint(newVerts[i + 2]);
            Vector3 vertWorldOld1 = transform.TransformPoint(verts[i + 0]);
            Vector3 vertWorldOld2 = transform.TransformPoint(verts[i + 1]);
            Vector3 vertWorldOld3 = transform.TransformPoint(verts[i + 2]);


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

            if (Physics.Raycast(vertWorld1,vertWorld2, Vector3.Distance(vertWorld1, vertWorld2) - .001f,splatOnto))
            {
                float lerpValue =  binaryFindEdge(vertWorldOld1, vertWorldOld2, vertWorld1, vertWorld2);
                newPoint12 = Vector3.Lerp(vertWorldOld1,vertWorldOld2,lerpValue - 0.01f);
                newPoint21 = Vector3.Lerp(vertWorldOld1, vertWorldOld2, lerpValue + 0.01f);
                newPoint12Local = transform.InverseTransformPoint(newPoint12);
                newPoint21Local = transform.InverseTransformPoint(newPoint21);
                newPoint12Splat =  transformVertToWall(newPoint12,newPoint12Local);
                newPoint21Splat = transformVertToWall(newPoint21, newPoint21Local);
            }
            if (Physics.Raycast(vertWorld2, vertWorld3, Vector3.Distance(vertWorld2, vertWorld3) - .001f, splatOnto))
            {
                float lerpValue = binaryFindEdge(vertWorldOld2, vertWorldOld3, vertWorld2, vertWorld3);
                newPoint23 = Vector3.Lerp(vertWorldOld2, vertWorldOld3, lerpValue - 0.01f);
                newPoint32 = Vector3.Lerp(vertWorldOld2, vertWorldOld3, lerpValue + 0.01f);
                newPoint23Local = transform.InverseTransformPoint(newPoint23);
                newPoint32Local = transform.InverseTransformPoint(newPoint32);
                newPoint23Splat = transformVertToWall(newPoint23, newPoint23Local);
                newPoint32Splat = transformVertToWall(newPoint32, newPoint32Local);
            }
            if (Physics.Raycast(vertWorld1, vertWorld3, Vector3.Distance(vertWorld1, vertWorld3) - .001f, splatOnto))
            {
                float lerpValue = binaryFindEdge(vertWorldOld1, vertWorldOld3, vertWorld1, vertWorld3);
                newPoint13 = Vector3.Lerp(vertWorldOld1, vertWorldOld3, lerpValue - 0.01f);
                newPoint31 = Vector3.Lerp(vertWorldOld1, vertWorldOld3, lerpValue + 0.01f);
                newPoint13Local = transform.InverseTransformPoint(newPoint13);
                newPoint31Local = transform.InverseTransformPoint(newPoint31);
                newPoint13Splat = transformVertToWall(newPoint13, newPoint13Local);
                newPoint31Splat = transformVertToWall(newPoint31, newPoint31Local);
            }

            // connect new splat point if they exist
            
        }
        mesh.vertices = verts;
    }

    float binaryFindEdge(Vector3 vert1, Vector3 vert2, Vector3 vert1Splat, Vector3 vert2Splat)
    {
        float lastLerpValue = 0f;
        float lerpValue = 0.5f;
        for (int i = 2; i < 8 && lerpValue != lastLerpValue; i++)
        {
            lastLerpValue = lerpValue;
            RaycastHit hit;
            RaycastHit hit2;
            if (Physics.Raycast(cam.transform.position, Vector3.Lerp(vert1, vert2, lerpValue) - cam.transform.position, out hit, 99, splatOnto))
            {
                if(Physics.Raycast(vert2Splat,  hit.point, out hit2, Vector3.Distance(vert2Splat, hit.point) - .001f, splatOnto))
                {
                    lerpValue += Mathf.Pow(.5f,i);
                }
                else if(Physics.Raycast(vert1Splat, hit.point, out hit2, Vector3.Distance(vert1Splat, hit.point) - .001f, splatOnto))
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
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
    string[] vertHits;

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
        Vector3[] newVerts = new Vector3[verts.Length];
        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 vertWorld = transform.TransformPoint(verts[i]);
            Vector3 vertLocal = verts[i];
            Vector3 newPos = transformVertToWall(vertWorld,vertLocal,i);
            verts[i] = newPos;
        }
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int tri1 = triangles[i + 0];
            int tri2 = triangles[i + 1];
            int tri3 = triangles[i + 2];
            Vector3 vertWorld1 = transform.TransformPoint(verts[i + 0]);
            Vector3 vertWorld2 = transform.TransformPoint(verts[i + 1]);
            Vector3 vertWorld3 = transform.TransformPoint(verts[i + 2]);
            if (vertHits[tri1] != vertHits[tri2])
            {
                float lertBetweenVerts = binaryFindEdge(vertWorld1,vertWorld2,tri1,tri2);
            }
            if (vertHits[tri1] != vertHits[tri3])
            {
                float lertBetweenVerts = binaryFindEdge(vertWorld1, vertWorld3,tri1, tri3);
            }
            if (vertHits[tri2] != vertHits[tri3])
            {
                float lertBetweenVerts = binaryFindEdge(vertWorld2, vertWorld3,tri2,tri3);
            }
        }
        mesh.vertices = verts;
    }

    float binaryFindEdge(Vector3 vert1, Vector3 vert2, int vert1Index,int vert2Index)
    {
        float lerpValue = 0.5f;
        for (int i = 2; i < 8; i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(cam.transform.position, Vector3.Lerp(vert1, vert2, lerpValue) - cam.transform.position, out hit, 99, splatOnto))
            {
                if(hit.transform.gameObject.name == vertHits[vert1Index])
                {
                    lerpValue += Mathf.Pow(.5f,i);
                }
                else
                {
                    lerpValue -= Mathf.Pow(.5f, i);
                }
            }
        }
        return lerpValue;
    }



    public Vector3 transformVertToWall(Vector3 worldPos, Vector3 localPos, int vertIndex)
    {
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, worldPos - cam.transform.position, out hit, 99, splatOnto))
        {
            Vector3 newPos = transform.InverseTransformPoint(hit.point);
            newPos = Vector3.Lerp(localPos, newPos, .999f);
            vertHits[vertIndex] = hit.transform.gameObject.name;
            return newPos;
        }
        return Vector3.negativeInfinity;
    }


}
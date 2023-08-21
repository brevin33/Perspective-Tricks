using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SplatAbleAttempt3TheFinally : MonoBehaviour
{
    [SerializeField]
    int accuracy;

    Mesh mesh;

    Camera cam;

    Vector3 camPos;

    MeshFilter[] allMeshes;

    VertInfo[] vertInfos;

    int vertCount;

    int triangleCount;

    Vector3[] newNormals;
    Vector3[] newVerticies;
    int[] newTrianlges;
    Vector2[] newUVs;
    Vector2[] newUVs2;
    Vector4[] newTangents;

    private void Awake()
    {

        mesh = GetComponent<MeshFilter>().mesh;
        cam = Camera.main;
        camPos = cam.transform.position;
        allMeshes = FindObjectsOfType<MeshFilter>();
    }

    private void OnMouseDown()
    {
        Silhouette silhouette = new Silhouette(gameObject.name, camPos);
        MeshFilter[] meshsInView = allMeshes;
        vertInfos = new VertInfo[mesh.vertices.Length * 1000];
        vertCount = 0;
        triangleCount = 0;
        newNormals = new Vector3[mesh.normals.Length * 1000];
        newTrianlges = new int[mesh.triangles.Length * 1000];
        newVerticies = new Vector3[mesh.vertices.Length * 1000];
        newTangents = new Vector4[mesh.vertices.Length * 1000];
        newUVs = new Vector2[mesh.uv.Length * 1000];
        newUVs2 = new Vector2[mesh.uv2.Length * 1000];
        generateNewSplatedMesh(ref silhouette, ref meshsInView);

    }

    void generateNewSplatedMesh(ref Silhouette silhouette, ref MeshFilter[] meshsInView)
    {
        for (int i = 0; i < meshsInView.Length; i++)
        {
            MeshFilter meshFilter = meshsInView[i];
            if (meshFilter.gameObject.name == gameObject.name) continue;
            Mesh otherMesh = meshFilter.mesh;
            VertInfo[] otherVerts = new VertInfo[otherMesh.vertices.Length];
            for (int j = 0; j < otherMesh.vertices.Length; j++)
            {
                otherVerts[j] = new VertInfo()
                {
                    pos = meshFilter.gameObject.transform.TransformPoint(otherMesh.vertices[j]),
                    normal = otherMesh.normals[j],
                    tangent = otherMesh.tangents[j],
                };
            }
            bool[] vertsBehind = vertsBehindSilhouette(ref otherVerts, ref silhouette);
            generateAndAddedNewVertsOnEdges(ref otherVerts, otherMesh.triangles, ref vertsBehind, ref silhouette);
        }
        cutOffAt(ref newVerticies,vertCount);
        cutOffAt(ref newUVs, vertCount);
        cutOffAt(ref newUVs2, vertCount);
        cutOffAt(ref newNormals, vertCount);
        cutOffAt(ref newTangents, vertCount);
        cutOffAt(ref newTrianlges, triangleCount);
        mesh.triangles = new int[3] { 0,0,0};
        mesh.SetVertices(newVerticies);
        mesh.SetUVs(0,newUVs);
        mesh.SetUVs(1, newUVs2);
        mesh.SetNormals( newNormals);
        mesh.SetTangents( newTangents);
        mesh.triangles = newTrianlges;
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh; 
    }

    void generateAndAddedNewVertsOnEdges(ref VertInfo[] otherVerts, int[] otherTriangles, ref bool[] behind, ref Silhouette silhouette)
    {
        for (int i = 0; i < otherTriangles.Length; i += 3)
        {
            List<VertInfo> toAdd = new List<VertInfo>();
            bool oneIn = false;
            if (behind[otherTriangles[i]])
            {
                oneIn = true;
                toAdd.Add(otherVerts[otherTriangles[i]]);
            }
            if (behind[otherTriangles[i + 1]])
            {
                oneIn = true;
                toAdd.Add(otherVerts[otherTriangles[i + 1]]);
            }
            if (behind[otherTriangles[i + 2]])
            {
                oneIn = true;
                toAdd.Add(otherVerts[otherTriangles[i + 2]]);
            }
            if (!oneIn)
            {
                continue;
            }
            int[,] otherEdges = new int[3,2] { { i, i + 1 }, { i, i + 2 }, { i + 1, i + 2 } };
            for (int j = 0; j < 3; j++)
            {
                int v1 = otherTriangles[otherEdges[j, 0]];
                int v2 = otherTriangles[otherEdges[j, 1]];
                if (!behind[v1] && !behind[v2])
                {
                    continue;
                }
                if (behind[v1] && behind[v2])
                {
                    continue;
                }

                if (behind[v1])
                {
                    VertInfo silhouetteEdgeVert = silhouette.findVertInfoInSilhouetteAlongEdge(otherVerts[v1], otherVerts[v2], accuracy);
                    toAdd.Add(silhouetteEdgeVert);
                }
                else // behind[v2]
                {
                    VertInfo silhouetteEdgeVert = silhouette.findVertInfoInSilhouetteAlongEdge(otherVerts[v2], otherVerts[v1], accuracy);
                    toAdd.Add(silhouetteEdgeVert);
                }
            }

            Plane edge1 = new Plane(otherVerts[otherTriangles[i]].pos, otherVerts[otherTriangles[i + 1]].pos, camPos);
            Plane edge2 = new Plane(otherVerts[otherTriangles[i]].pos, otherVerts[otherTriangles[i + 2]].pos, camPos);
            Plane edge3 = new Plane(otherVerts[otherTriangles[i + 2]].pos, otherVerts[otherTriangles[i + 1]].pos, camPos);
            Vector3 pointInPlane = (otherVerts[otherTriangles[i]].pos + otherVerts[otherTriangles[i + 1]].pos + otherVerts[otherTriangles[i + 2]].pos)/3;
            VertInfo[] vertsInTrianlge = getVertsInTriangle(edge1,edge2,edge3,pointInPlane);

            for (int j = 0; j < vertsInTrianlge.Length; j++)
            {
                //toAdd.Add(vertsInTrianlge[j]); ---------------------------------------------------------------------------------------------------------------------------------
            }

            addVerts(ref toAdd, ref silhouette);
        }
    }




    VertInfo[] getVertsInTriangle(Plane edge1, Plane edge2, Plane edge3, Vector3 pointInPlane)
    {
        VertInfo[] vertsInTrianlges = new VertInfo[mesh.vertices.Length];
        int numberOfVertsInTriangle = 0;
        for (int i = 0; i < mesh.vertices.Length; i++) {
            Vector3 vert = mesh.vertices[i];
            if(edge1.SameSide(vert, pointInPlane) && edge2.SameSide(vert, pointInPlane) && edge3.SameSide(vert, pointInPlane))
            {
                vertsInTrianlges[numberOfVertsInTriangle++] = new VertInfo {
                    pos = vert,
                    normal = mesh.normals[i],
                    tangent = mesh.tangents[i],
                    uv = mesh.uv[i],
                    uv2 = mesh.uv2[i]
                };
            }
        }
        cutOffAt(ref vertsInTrianlges,numberOfVertsInTriangle);
        return vertsInTrianlges;
    }

    void addVerts(ref List<VertInfo> toAdd, ref Silhouette silhouette)
    {
        for (int i = 0; i < toAdd.Count; i++)
        {
            VertInfo vertInfo = toAdd[i];
            silhouette.setVertInfoUVs(ref vertInfo);
            newVerticies[vertCount] = transform.InverseTransformPoint(Vector3.Lerp(vertInfo.pos, camPos, .001f));
            newNormals[vertCount] = vertInfo.normal;
            newTangents[vertCount] = vertInfo.tangent;
            newUVs[vertCount] = vertInfo.uv;
            newUVs2[vertCount] = vertInfo.uv2;
            vertCount++;
        }
        if (toAdd.Count == 3)
        {
            newTrianlges[triangleCount] = vertCount - 3;
            newTrianlges[triangleCount + 1] = vertCount - 2;
            newTrianlges[triangleCount + 2] = vertCount - 1;
            triangleCount += 3;
        }
        else
        {
            newTrianlges[triangleCount] = vertCount - 4;
            newTrianlges[triangleCount + 1] = vertCount - 1;
            newTrianlges[triangleCount + 2] = vertCount - 3;
            triangleCount += 3;

            newTrianlges[triangleCount] = vertCount - 4;
            newTrianlges[triangleCount + 1] = vertCount - 2;
            newTrianlges[triangleCount + 2] = vertCount - 1;
            triangleCount += 3;
        }
    }


    bool[] vertsBehindSilhouette(ref VertInfo[] verts, ref Silhouette silhouette)
    {
        bool[] behind = new bool[verts.Length];
        Array.Fill(behind, false);
        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 vert = verts[i].pos;
            if (silhouette.pointBehind(vert))
            {
                behind[i] = true;
            }
        }
        return behind;
    }

    MeshFilter[] getmeshsInView()
    {
        MeshFilter[] meshInView = new MeshFilter[allMeshes.Length];
        int numMeshInView = 0;
        for (int i = 0; i < allMeshes.Length; i++)
        {
            MeshFilter m = allMeshes[i];
            if (m.gameObject.name == gameObject.name && CheckIfObjectWithinView(m, cam))
            {
                meshInView[numMeshInView] = m;
                numMeshInView++;
            }
        }
        MeshFilter[] shortObjInView = new MeshFilter[numMeshInView];
        for (int i = 0; i < numMeshInView; i++)
        {
            shortObjInView[i] = meshInView[i];
        }
        return shortObjInView;
    }


    public bool CheckIfObjectWithinView(MeshFilter m, Camera cam)
    {
        if (m == null) return false;

        var bounds = m.mesh.bounds;

        var planes = GeometryUtility.CalculateFrustumPlanes(cam);
        var points = GetEightBoundsVertices(bounds);

        if (points.Count(p => TestPoint(planes, p)) == points.Count)
        {
            //the whole object is within cam view
            return true;
        }
        return false;
    }


    //check if all vertices points are in the positive direction of all cam frustum planes
    //if it is true, the gameobject is within camview
    bool TestPoint(Plane[] planes, Vector3 point) => !planes.Any(plane => plane.GetDistanceToPoint(point) < 0);

    List<Vector3> GetEightBoundsVertices(Bounds bounds) => new List<Vector3>() {
     bounds.min,
       bounds.max,
       new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
       new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
       new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
       new Vector3(bounds.min.x, bounds.max.y, bounds.max.z),
       new Vector3(bounds.max.x, bounds.min.y, bounds.max.z),
       new Vector3(bounds.max.x, bounds.max.y, bounds.min.z)
   };

    void cutOffAt<T>(ref T[] list, int size)
    {
        T[] l =  new T[size];
        for (int i = 0; i < size; i++)
        {
            l[i] = list[i];
        }
        list = l;
    }

}

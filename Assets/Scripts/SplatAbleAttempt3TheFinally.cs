using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

// got verts on silouet just make them part of the edge triangles

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


    [SerializeField]
    LayerMask splatOnto;

    VertInfo[] splatedVerts;

    Dictionary<triangleIndex, List<int>> triangleIndexToSplattedVertsIndex;


    private void Awake()
    {

        mesh = GetComponent<MeshFilter>().mesh;
        cam = Camera.main;
        camPos = cam.transform.position;
        allMeshes = FindObjectsOfType<MeshFilter>();
        triangleIndexToSplattedVertsIndex = new Dictionary<triangleIndex, List<int>>();
    }

    private void OnMouseDown()
    {
        camPos = cam.transform.position;
        Silhouette silhouette = new Silhouette(gameObject.name, camPos);
        MeshFilter[] meshsInView = allMeshes;
        vertInfos = new VertInfo[mesh.vertices.Length * 100];
        vertCount = 0;
        triangleCount = 0;
        newNormals = new Vector3[mesh.normals.Length * 100];
        newTrianlges = new int[mesh.triangles.Length * 100];
        newVerticies = new Vector3[mesh.vertices.Length * 100];
        newTangents = new Vector4[mesh.vertices.Length * 100];
        newUVs = new Vector2[mesh.uv.Length * 100];
        newUVs2 = new Vector2[mesh.uv2.Length * 100];
        setSplatedVerts();
        generateNewSplatedMesh(ref silhouette, ref meshsInView);

    }



    public Vector3 transformVertToWall(Vector3 worldPos, Vector3 localPos, out RaycastHit hit)
    {
        if (Physics.Raycast(cam.transform.position, worldPos - cam.transform.position, out hit, 99, splatOnto))
        {
            Vector3 newPos = transform.InverseTransformPoint(hit.point);
            newPos = Vector3.Lerp(localPos, newPos, .999f);
            return transform.TransformPoint(newPos);
        }
        return Vector3.negativeInfinity;
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
            generateAndAddedNewVertsOnEdges(ref otherVerts, otherMesh.triangles, ref vertsBehind, ref silhouette, meshFilter.gameObject.name);
        }
        cutOffAt(ref newVerticies, vertCount);
        cutOffAt(ref newUVs, vertCount);
        cutOffAt(ref newUVs2, vertCount);
        cutOffAt(ref newNormals, vertCount);
        cutOffAt(ref newTangents, vertCount);
        cutOffAt(ref newTrianlges, triangleCount);
        mesh.triangles = new int[3] { 0, 0, 0 };
        mesh.SetVertices(newVerticies);
        mesh.SetUVs(0, newUVs);
        mesh.SetUVs(1, newUVs2);
        mesh.SetNormals(newNormals);
        mesh.SetTangents(newTangents);
        mesh.triangles = newTrianlges;
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    void generateAndAddedNewVertsOnEdges(ref VertInfo[] otherVerts, int[] otherTriangles, ref bool[] behind, ref Silhouette silhouette, string gameObjectName)
    {
        for (int i = 0; i < otherTriangles.Length; i += 3)
        {
            List<VertInfo> toAdd = new List<VertInfo>();
            bool oneIn = false;
            int amountFromWall = 0;
            bool isEdge = true;
            bool oneEdge = false;

            if (behind[otherTriangles[i]])
            {
                amountFromWall++;
                oneIn = true;
                toAdd.Add(otherVerts[otherTriangles[i]]);
            }
            if (behind[otherTriangles[i + 1]])
            {
                amountFromWall++;
                oneIn = true;
                toAdd.Add(otherVerts[otherTriangles[i + 1]]);
            }
            if (behind[otherTriangles[i + 2]])
            {
                amountFromWall++;
                oneIn = true;
                toAdd.Add(otherVerts[otherTriangles[i + 2]]);
            }
            if (!oneIn)
            {
                continue;
            }
            int[,] otherEdges = new int[3, 2] { { i, i + 1 }, { i, i + 2 }, { i + 1, i + 2 } };
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

            triangleIndex triIndex = new triangleIndex() {
                index = i,
                gameObject = gameObjectName
            };
            VertInfo[] vertsInTrianlge = getVertsInTriangle(triIndex);

            for (int j = 0; j < vertsInTrianlge.Length; j++)
            {
                toAdd.Add(vertsInTrianlge[j]);
            }
            if (amountFromWall == 3)
            {
                isEdge = false;
            }
            else if (amountFromWall == 2)
            {
                oneEdge = false;
            }
            else
            {
                oneEdge = true;
            }

            addVerts(ref toAdd, ref silhouette, isEdge, oneEdge);
        }
    }

    void setSplatedVerts()
    {
        int sizeSplattedVerts = 0;
        splatedVerts = new VertInfo[mesh.vertices.Length];
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            Vector3 vert = transform.TransformPoint(mesh.vertices[i]);
            Vector3 vertSplat = transformVertToWall(transform.TransformPoint(mesh.vertices[i]), mesh.vertices[i], out RaycastHit hit2);
            if (Physics.Raycast(camPos, vert - camPos, out RaycastHit hit))
            {
                if (Vector3.Distance(hit.point, vert) <= .02f)
                {
                    triangleIndex triIndex = new triangleIndex { index = hit2.triangleIndex * 3, gameObject = hit.transform.gameObject.name };
                    if (triangleIndexToSplattedVertsIndex[triIndex] is null)
                    {
                        triangleIndexToSplattedVertsIndex[triIndex] = new List<int>();
                    }
                    triangleIndexToSplattedVertsIndex[triIndex].Add(sizeSplattedVerts);

                    splatedVerts[sizeSplattedVerts++] = new VertInfo
                    {
                        pos = vertSplat,
                        normal = mesh.normals[i],
                        tangent = mesh.tangents[i],
                        uv = mesh.uv[i],
                        uv2 = mesh.uv2[i]
                    };
                }
            }

        }

        cutOffAt(ref splatedVerts, sizeSplattedVerts);
    }



    VertInfo[] getVertsInTriangle(triangleIndex triIndex)
    {
        List<int> hits = triangleIndexToSplattedVertsIndex[triIndex];
        VertInfo[] vertInfos = new VertInfo[hits.Count];
        for (int i = 0; i < hits.Count; i++)
        {
            vertInfos[i] = splatedVerts[hits[i]];
        }
        return vertInfos;
    }

    void addVerts(ref List<VertInfo> toAdd, ref Silhouette silhouette, bool edge, bool oneEdge)
    {
        int toAddSize = toAdd.Count;

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

        // no verts in triangle
        if (toAdd.Count == 3 || (toAdd.Count == 4 && edge))
        {
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
            return;
        }

        edge[] edgesToComplete = new edge[toAddSize * toAddSize];
        List<int> unusedVerts;
        int edgesToCompleteSize = 0;

        //set up bounding area


        if (edge && !oneEdge)
        {
            edge[] minBoundingBox = new edge[4];
            minBoundingBox[0] = new edge { vert1 = vertCount - toAddSize, vert2 = vertCount - toAddSize + 1 };
            minBoundingBox[1] = new edge { vert1 = vertCount - toAddSize + 3, vert2 = vertCount - toAddSize + 1 };
            minBoundingBox[2] = new edge { vert1 = vertCount - toAddSize, vert2 = vertCount - toAddSize + 2 };
            minBoundingBox[3] = new edge { vert1 = vertCount - toAddSize + 2, vert2 = vertCount - toAddSize + 3 };
            Vector3 pointInMinBoundingPlane = (newVerticies[vertCount - toAddSize] + newVerticies[vertCount - toAddSize + 1] + newVerticies[vertCount - toAddSize + 2] + newVerticies[vertCount - toAddSize + 3]) * .25f;
            Plane[] minBoundingPlanes = new Plane[4];
            for (int i = 0; i < minBoundingBox.Length; i++)
            {
                edge e = minBoundingBox[i];
                minBoundingPlanes[i] = new Plane(newVerticies[e.vert1], newVerticies[e.vert2], camPos);
            }
            int[] vertsMakingBoundingPlane = new int[toAddSize];
            int vertsMakingBoundingPlaneSize = 0;
            unusedVerts = new List<int>();
            for (int i = 4; i < toAddSize; i++)
            {
                int vertIndex = vertCount - toAddSize + i;
                Vector3 vert = newVerticies[vertIndex];
                if (!pointInPlanes(minBoundingPlanes, vert, pointInMinBoundingPlane))
                {
                    vertsMakingBoundingPlane[vertsMakingBoundingPlaneSize] = vertIndex;
                    vertsMakingBoundingPlaneSize++;
                }
                else
                {
                    unusedVerts.Add(i);
                }
            }
            if (unusedVerts.Count == 0)
            {
                if (Physics.Raycast(camPos, pointInMinBoundingPlane - camPos, out RaycastHit hit))
                {
                    newVerticies[vertCount] = transform.InverseTransformPoint(pointInMinBoundingPlane);
                    newNormals[vertCount] = hit.normal;
                    newTangents[vertCount] = Vector4.Lerp(toAdd[0].tangent, toAdd[1].tangent, .5f);
                    newUVs[vertCount] = hit.textureCoord;
                    newUVs2[vertCount] = hit.textureCoord2;
                    vertCount++;
                }
                unusedVerts.Add(toAddSize + 1);
            }
            cutOffAt(ref vertsMakingBoundingPlane, vertsMakingBoundingPlaneSize);

            edgesToComplete[edgesToCompleteSize++] = minBoundingBox[0];
            edgesToComplete[edgesToCompleteSize++] = minBoundingBox[1];
            edgesToComplete[edgesToCompleteSize++] = minBoundingBox[2];

            int currentVertIndex = vertCount - toAddSize + 2;
            int endVert = vertCount - toAddSize + 3;
            Vector3 startVert = newVerticies[currentVertIndex];
            Comparison<int> closestToStartVert = (x, y) => Vector3.Distance(newVerticies[vertCount - toAddSize + x], startVert) == Vector3.Distance(newVerticies[vertCount - toAddSize + y], startVert) ? 0 : Vector3.Distance(newVerticies[vertCount - toAddSize + x], startVert) > Vector3.Distance(newVerticies[vertCount - toAddSize + y], startVert) ? 1 : -1;
            Array.Sort(vertsMakingBoundingPlane, closestToStartVert);
            for (int i = 0; i < vertsMakingBoundingPlaneSize; i++)
            {
                edgesToComplete[edgesToCompleteSize++] = new edge { vert1 = currentVertIndex, vert2 = vertsMakingBoundingPlane[i] };
                currentVertIndex = vertsMakingBoundingPlane[i];
            }
            edgesToComplete[edgesToCompleteSize++] = new edge { vert1 = currentVertIndex, vert2 = endVert };
        }
        else if (oneEdge)
        {
            // ------------------------------------------------------------------------------------------------------------------------------------------------------------------------- not done
            unusedVerts = new List<int>();
            return;
        }
        else // is not edge
        {
            edgesToComplete[0] = new edge { vert1 = vertCount - toAddSize, vert2 = vertCount - toAddSize + 1 };
            edgesToComplete[1] = new edge { vert1 = vertCount - toAddSize, vert2 = vertCount - toAddSize + 2 };
            edgesToComplete[2] = new edge { vert1 = vertCount - toAddSize + 2, vert2 = vertCount - toAddSize + 1 };
            edgesToCompleteSize = 3;
            unusedVerts = new List<int>();
            for (int i = 3; i < toAddSize; i++)
            {
                unusedVerts.Add(i);
            }
        }
        int edgesToCompleteIndex = 0;

        //  fill bounding area using all verts
        while (unusedVerts.Count > 1)
        {
            edge toComplete = edgesToComplete[edgesToCompleteIndex++];
            Plane p = new Plane(newVerticies[toComplete.vert1], newVerticies[toComplete.vert2], camPos);

            int unusedVertIndex = -1;
            float lowestDistance = 999;
            for (int i = 0; i < unusedVerts.Count; i++)
            {
                float distance = p.GetDistanceToPoint(newVerticies[vertCount - toAddSize + unusedVerts[i]]);
                if (distance < lowestDistance)
                {
                    lowestDistance = distance;
                    unusedVertIndex = i;
                }
            }
            unusedVerts.RemoveAt(unusedVertIndex);

            int vertToAdd = vertCount - toAddSize + unusedVertIndex;

            newTrianlges[triangleCount] = vertToAdd;
            newTrianlges[triangleCount + 1] = toComplete.vert1;
            newTrianlges[triangleCount + 2] = toComplete.vert2;
            triangleCount += 3;

            edgesToComplete[edgesToCompleteSize++] = new edge { vert1 = toComplete.vert1, vert2 = vertToAdd };
            edgesToComplete[edgesToCompleteSize++] = new edge { vert1 = toComplete.vert2, vert2 = vertToAdd };
        }
        int finalVertex = vertCount - toAddSize + unusedVerts[0];
        while (edgesToCompleteIndex < edgesToCompleteSize)
        {
            edge toComplete = edgesToComplete[edgesToCompleteIndex++];
            newTrianlges[triangleCount] = finalVertex;
            newTrianlges[triangleCount + 1] = toComplete.vert1;
            newTrianlges[triangleCount + 2] = toComplete.vert2;
            triangleCount += 3;
        }

    }

    bool pointInPlanes(Plane[] planes, Vector3 point, Vector3 pointInPlane)
    {
        bool inPlane = true;
        for (int i = 0; i < planes.Length; i++)
        {
            inPlane = planes[i].SameSide(point, pointInPlane) ? inPlane : false;
        }
        return inPlane;
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
        T[] l = new T[size];
        for (int i = 0; i < size; i++)
        {
            l[i] = list[i];
        }
        list = l;
    }

}

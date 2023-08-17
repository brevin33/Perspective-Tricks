using System.Collections.Generic;
using UnityEngine;

public class SplatAbleAttempt2 : MonoBehaviour
{

    MeshFilter meshFilter;
    Mesh mesh;
    Vector3[] verts;
    int[] triangles;

    [SerializeField]
    Camera cam;

    [SerializeField]
    LayerMask splatOnto;

    Vector3[] newVerts;

    int[] newTriangles;

    GameObject[] hitGameObjects;

    string[] hitNames;

    Vector3 camPos;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = meshFilter.mesh;
        verts = mesh.vertices;
        triangles = mesh.triangles;
        hitGameObjects = new GameObject[verts.Length];
        camPos = cam.transform.position;
    }
    private void OnMouseDown()
    {
        newVerts = new Vector3[verts.Length * 50];
        newTriangles = new int[triangles.Length * 50];

        splatVerts();

        fixSmearingOnEdges();

        verts = newVerts;
        mesh.vertices = verts;
    }

    int fixSmearingOnEdges()
    {
        int newVertsLastAddedIndex = verts.Length;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            List<int> triangle = new List<int>(3);
            triangle[0] = triangles[i];
            triangle[1] = triangles[i + 1];
            triangle[2] = triangles[i + 2];

            triangle.Sort(distFromCam);

            for (int j = 0; j < triangle.Count; j++)
            {
                int vertIndex = triangle[i];
                getNewVertsFromHitMesh(hitGameObjects[vertIndex], triangle[0], triangle[1], triangle[2], newVertsLastAddedIndex);
            }

        }
        return newVertsLastAddedIndex;
    }

    int distFromCam(int x, int y)
    {
        if (Vector3.Distance(newVerts[x], cam.transform.position) > Vector3.Distance(newVerts[y], cam.transform.position))
        {
            return -1;
        }
        else
        {
            return 1;
        }
    }

    void getNewVertsFromHitMesh(ref GameObject getVertsFrom, int pointIndex, int point2Index, int point3Index, ref int lastAddedNewVertIndex, ref int lastAddedNewTriangleIndex)
    {
        Vector3 point = newVerts[pointIndex];
        Vector3 point2 = newVerts[point2Index];
        Vector3 point3 = newVerts[point3Index];
        Plane plane1 = new Plane(point, point2, camPos);
        Plane plane2 = new Plane(point2, point3, camPos);
        Plane plane3 = new Plane(point, point2, camPos);
        Vector3 pointInPlane = (point + point2 + point3) / 3;
        MeshFilter meshFilter = getVertsFrom.GetComponent<MeshFilter>();
        Mesh otherMesh = meshFilter.mesh;
        int[] otherTriangles = otherMesh.triangles;
        Vector3[] otherVerts = otherMesh.vertices;
        for (int i = 0; i > otherTriangles.Length; i += 3)
        {
            int[] otherTriangle = new int[3];
            otherTriangle[0] = otherTriangles[i + 0];
            otherTriangle[1] = otherTriangles[i + 1];
            otherTriangle[2] = otherTriangles[i + 2];

            List<int> vertIndexToAddToTriangles = new List<int>();
            bool addedVerts = false;
            bool[,] checkedEdges = new bool[3, 3] { {false,false,false}, { false, false, false }, { false, false, false } };

            checkAndAddToNewVerts(0, 1, 2, ref otherVerts, ref otherTriangle, ref plane1, ref plane2, ref plane3, ref pointInPlane, ref lastAddedNewTriangleIndex, ref vertIndexToAddToTriangles, ref lastAddedNewVertIndex, ref addedVerts, ref checkedEdges);
            checkAndAddToNewVerts(1, 2, 0, ref otherVerts, ref otherTriangle, ref plane1, ref plane2, ref plane3, ref pointInPlane, ref lastAddedNewTriangleIndex, ref vertIndexToAddToTriangles, ref lastAddedNewVertIndex, ref addedVerts, ref checkedEdges);
            checkAndAddToNewVerts(2, 0, 1, ref otherVerts, ref otherTriangle, ref plane1, ref plane2, ref plane3, ref pointInPlane, ref lastAddedNewTriangleIndex, ref vertIndexToAddToTriangles, ref lastAddedNewVertIndex, ref addedVerts, ref checkedEdges);
            if (addedVerts)
            {

            }

            List<int> trianglesToAdd = makeTriangles(vertIndexToAddToTriangles);
            for (int k = 0; k < trianglesToAdd.Count; k++)
            {
                newTriangles[lastAddedNewTriangleIndex] = trianglesToAdd[k];
                lastAddedNewTriangleIndex++;
            }



        }
    }

    void checkAndAddToNewVerts(int otherTriangleIndex, int otherTriangleIndexConnected, int otherTriangleIndexConnected2, ref Vector3[] otherVerts, ref int[] otherTriangle, ref Plane plane1, ref Plane plane2, ref Plane plane3, ref Vector3 pointInPlane, ref int lastAddedNewTriangleIndex, ref List<int> vertIndexToAddToTriangles, ref int lastAddedNewVertIndex, ref bool addedVerts, ref bool[,] checkedEdges)
    {
        if (insidePlanes(otherVerts[otherTriangle[otherTriangleIndex]], plane1, plane2, plane3, pointInPlane))
        {
            addedVerts = true;
            newVerts[lastAddedNewTriangleIndex] = otherVerts[otherTriangle[0]];
            vertIndexToAddToTriangles.Add(lastAddedNewVertIndex);
            lastAddedNewVertIndex++;
        }
        else
        {
            if (!checkedEdges[otherTriangleIndex, otherTriangleIndexConnected])
            {


                Vector3 dir = (otherVerts[otherTriangle[otherTriangleIndexConnected]] - otherVerts[otherTriangle[otherTriangleIndex]]).normalized;
                Ray ray = new Ray(otherVerts[otherTriangle[otherTriangleIndex]], dir);
                if (plane1.Raycast(ray, out float dist))
                {
                    if (dist <= Vector3.Distance(otherVerts[otherTriangle[otherTriangleIndexConnected]], otherVerts[otherTriangle[otherTriangleIndex]]))
                    {
                        addedVerts = true;
                        newVerts[lastAddedNewTriangleIndex] = otherVerts[otherTriangle[0]] + (dir * dist);
                        vertIndexToAddToTriangles.Add(lastAddedNewVertIndex);
                        lastAddedNewVertIndex++;
                    }
                }

                if (plane2.Raycast(ray, out dist))
                {
                    if (dist <= Vector3.Distance(otherVerts[otherTriangle[otherTriangleIndexConnected]], otherVerts[otherTriangle[otherTriangleIndex]]))
                    {
                        addedVerts = true;
                        newVerts[lastAddedNewTriangleIndex] = otherVerts[otherTriangle[0]] + (dir * dist);
                        vertIndexToAddToTriangles.Add(lastAddedNewVertIndex);
                        lastAddedNewVertIndex++;
                    }
                }

                if (plane3.Raycast(ray, out dist))
                {
                    if (dist <= Vector3.Distance(otherVerts[otherTriangle[otherTriangleIndexConnected]], otherVerts[otherTriangle[otherTriangleIndex]]))
                    {
                        addedVerts = true;
                        newVerts[lastAddedNewTriangleIndex] = otherVerts[otherTriangle[0]] + (dir * dist);
                        vertIndexToAddToTriangles.Add(lastAddedNewVertIndex);
                        lastAddedNewVertIndex++;
                    }
                }

                checkedEdges[otherTriangleIndex, otherTriangleIndexConnected] = true;
                checkedEdges[otherTriangleIndexConnected, otherTriangleIndex] = true;
            }

            if (!checkedEdges[otherTriangleIndex, otherTriangleIndexConnected])
            {
                // again with other edge
                Vector3 dir = (otherVerts[otherTriangle[otherTriangleIndexConnected2]] - otherVerts[otherTriangle[otherTriangleIndex]]).normalized;
                Ray ray = new Ray(otherVerts[otherTriangle[0]], dir);
                if (plane1.Raycast(ray, out float dist))
                {
                    if (dist <= Vector3.Distance(otherVerts[otherTriangle[otherTriangleIndexConnected2]], otherVerts[otherTriangle[otherTriangleIndex]]))
                    {
                        addedVerts = true;
                        newVerts[lastAddedNewTriangleIndex] = otherVerts[otherTriangle[0]] + (dir * dist);
                        vertIndexToAddToTriangles.Add(lastAddedNewVertIndex);
                        lastAddedNewVertIndex++;
                    }
                }

                if (plane2.Raycast(ray, out dist))
                {
                    if (dist <= Vector3.Distance(otherVerts[otherTriangle[otherTriangleIndexConnected2]], otherVerts[otherTriangle[otherTriangleIndex]]))
                    {
                        addedVerts = true;
                        newVerts[lastAddedNewTriangleIndex] = otherVerts[otherTriangle[0]] + (dir * dist);
                        vertIndexToAddToTriangles.Add(lastAddedNewVertIndex);
                        lastAddedNewVertIndex++;
                    }
                }

                if (plane3.Raycast(ray, out dist))
                {
                    if (dist <= Vector3.Distance(otherVerts[otherTriangle[otherTriangleIndexConnected2]], otherVerts[otherTriangle[otherTriangleIndex]]))
                    {
                        addedVerts = true;
                        newVerts[lastAddedNewTriangleIndex] = otherVerts[otherTriangle[0]] + (dir * dist);
                        vertIndexToAddToTriangles.Add(lastAddedNewVertIndex);
                        lastAddedNewVertIndex++;
                    }
                }

                checkedEdges[otherTriangleIndex, otherTriangleIndexConnected] = true;
                checkedEdges[otherTriangleIndexConnected, otherTriangleIndex] = true;

            }

        }
    }

    bool insidePlanes(Vector3 point, Plane p1, Plane p2, Plane p3, Vector3 pointInPlane)
    {
        return p1.SameSide(point, pointInPlane) && p2.SameSide(point, pointInPlane) && p3.SameSide(point, pointInPlane);
    }


    List<int> makeTriangles(List<int> vertices)
    {
        if (vertices.Count == 3)
        {
            return vertices;
        }
        else if (vertices.Count == 4)
        {
            return new List<int>(6) { vertices[0], vertices[1], vertices[2], vertices[3], vertices[1], vertices[2] };
        }
        return null;
    }

    int getAwayIndex(int to, int home)
    {
        if (to + home == 5)
        {
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

    void splatVerts()
    {
        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 vertWorld = transform.TransformPoint(verts[i]);
            Vector3 vertLocal = verts[i];
            GameObject hit;
            string name;
            Vector3 newPos = transformVertToWall(vertWorld, vertLocal, out hit, out name);
            hitGameObjects[i] = hit;
            hitNames[i] = name;
            newVerts[i] = newPos;
        }
    }
    Vector3 transformVertToWall(Vector3 worldPos, Vector3 localPos)
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

    Vector3 transformVertToWall(Vector3 worldPos, Vector3 localPos, out GameObject hitGameObject, out string hitName)
    {
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, worldPos - cam.transform.position, out hit, 99, splatOnto))
        {
            Vector3 newPos = transform.InverseTransformPoint(hit.point);
            newPos = Vector3.Lerp(localPos, newPos, .999f);
            hitGameObject = hit.transform.gameObject;
            hitName = hitGameObject.name;
            return newPos;
        }
        hitName = null;
        hitGameObject = null;
        return Vector3.negativeInfinity;
    }

    Vector3 transformVertToWall(Vector3 worldPos, Vector3 localPos, out Vector3 splatWorldPos)
    {
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, worldPos - cam.transform.position, out hit, 99, splatOnto))
        {
            splatWorldPos = hit.point;
            Vector3 newPos = transform.InverseTransformPoint(hit.point);
            newPos = Vector3.Lerp(localPos, newPos, .999f);
            return newPos;
        }
        splatWorldPos = Vector3.negativeInfinity;
        return Vector3.negativeInfinity;
    }

}

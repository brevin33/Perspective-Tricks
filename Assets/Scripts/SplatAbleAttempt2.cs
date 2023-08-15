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

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = meshFilter.mesh;
        verts = mesh.vertices;
        triangles = mesh.triangles;
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
            int[] triangle = new int[3];
            triangle[0] = triangles[i];
            triangle[1] = triangles[i + 1];
            triangle[2] = triangles[i + 2];

            int[,] addedEdgeVerts = new int[3, 3] { { -1, -1, -1 }, { -1, -1, -1 }, { -1, -1, -1 } };

            int[,] connections = new int[3, 2] { { 1, 2 }, { 1, 3 }, { 2, 3 } };

            // make new verts
            for (int l = 0; l < triangle.Length; l++)
            {
                int j = connections[l, 0];
                int k = connections[l, 1];

                Vector3 splatedVertJ = newVerts[triangle[j]];
                Vector3 splatedVertK = newVerts[triangle[k]];

                if (Physics.Linecast(splatedVertJ, splatedVertK))
                {
                    Vector3 splatEdgeVert1;
                    Vector3 splatEdgeVert2;
                    binaryFindEdge(triangle[j], triangle[k], out splatEdgeVert1, out splatEdgeVert2);
                    newVerts[newVertsLastAddedIndex] = splatEdgeVert1;
                    newVerts[newVertsLastAddedIndex + 1] = splatEdgeVert2;
                    addedEdgeVerts[j, k] = newVertsLastAddedIndex;
                    addedEdgeVerts[k, j] = newVertsLastAddedIndex + 1;
                    newVertsLastAddedIndex += 2;
                }
            }

            // get other verts inside triangle

            // make triangles with verts
            // new plan is to find all vert on projected area from triangle then stitch triangles fanning out from home triangle
        }
        return newVertsLastAddedIndex;
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
    float binaryFindEdge(int vert1Index, int vert2Index, out Vector3 splatEdgeVert1, out Vector3 splatEdgeVert2)
    {
        Vector3 vert1OriginalPos = verts[vert1Index];
        Vector3 vert2OriginalPos = verts[vert2Index];
        Vector3 vert1SplatWorld = transform.TransformPoint(newVerts[vert1Index]);
        Vector3 vert2SplatWorld = transform.TransformPoint(newVerts[vert2Index]);

        // loop setup
        float edgeBetween = .5f;
        Vector3 EdgeVert1 = Vector3.Lerp(vert1OriginalPos, vert2OriginalPos, edgeBetween - .01f);
        Vector3 EdgeVert2 = Vector3.Lerp(vert1OriginalPos, vert2OriginalPos, edgeBetween + .01f);
        Vector3 splatEdgeVert1World;
        Vector3 splatEdgeVert2World;
        splatEdgeVert1 = transformVertToWall(transform.TransformPoint(EdgeVert1), EdgeVert1, out splatEdgeVert1World);
        splatEdgeVert2 = transformVertToWall(transform.TransformPoint(EdgeVert2), EdgeVert2, out splatEdgeVert2World);
        float change = .25f;

        while (!Physics.Linecast(splatEdgeVert1World, splatEdgeVert2World))
        {
            if (Physics.Linecast(splatEdgeVert1World, vert1SplatWorld))
            {
                edgeBetween -= change;
            }
            else if (Physics.Linecast(splatEdgeVert2World, vert2SplatWorld))
            {
                edgeBetween += change;
            }
            else
            {
                Debug.LogError("surface to rough or to low vert desitiy in splatting mesh");
                break;
            }
            EdgeVert1 = Vector3.Lerp(vert1OriginalPos, vert2OriginalPos, edgeBetween - .01f);
            EdgeVert2 = Vector3.Lerp(vert1OriginalPos, vert2OriginalPos, edgeBetween + .01f);
            splatEdgeVert1 = transformVertToWall(transform.TransformPoint(EdgeVert1), EdgeVert1, out splatEdgeVert1World);
            splatEdgeVert2 = transformVertToWall(transform.TransformPoint(EdgeVert2), EdgeVert2, out splatEdgeVert2World);
            change *= .5f;
        }

        return edgeBetween;
    }

    void splatVerts()
    {
        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 vertWorld = transform.TransformPoint(verts[i]);
            Vector3 vertLocal = verts[i];
            Vector3 newPos = transformVertToWall(vertWorld, vertLocal);
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

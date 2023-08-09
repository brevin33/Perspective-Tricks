using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplatAble : MonoBehaviour
{
    
    MeshFilter meshFilter;
    Mesh mesh;
    Vector3[] verts;

    [SerializeField]
    Camera cam;

    [SerializeField]
    LayerMask splatOnto;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = meshFilter.mesh;
        verts = mesh.vertices;
    }
    private void OnMouseDown()
    {
        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 vert =  transform.TransformPoint( verts[i]);
            Vector3 newPos = transformVertToWall(vert);
            verts[i] = newPos;
        }

        mesh.vertices = verts;
    }

    public Vector3 transformVertToWall(Vector3 pos)
    {
        RaycastHit hit;
        Debug.Log("here");
        if (Physics.Raycast(cam.transform.position, pos - cam.transform.position, out hit, 99, splatOnto))
        {
            Vector3 newPos = transform.InverseTransformPoint( hit.point);
            newPos += (cam.transform.position - pos) * .00001f;
            return newPos;
        }
        return Vector3.negativeInfinity;
    }


}

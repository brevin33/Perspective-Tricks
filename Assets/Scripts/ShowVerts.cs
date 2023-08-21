using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class ShowVerts : MonoBehaviour
{
    [SerializeField]
    float size = .01f;
    private void OnDrawGizmos()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.sharedMesh;
        Vector3[] verts = mesh.vertices;
        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 vert = verts[i];
            Gizmos.DrawSphere(transform.TransformPoint( vert),size);
        }
    }
}

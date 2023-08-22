using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TwoSidedMeshCollider : MonoBehaviour
{
    void Start()
    {
        foreach (MeshCollider meshCollider in GameObject.FindObjectsOfType<MeshCollider>())
        {
            meshCollider.sharedMesh.SetIndices(meshCollider.sharedMesh.GetIndices(0).Concat(meshCollider.sharedMesh.GetIndices(0).Reverse()).ToArray(), MeshTopology.Triangles, 0);
        }
    }
}

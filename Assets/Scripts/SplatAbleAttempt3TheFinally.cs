using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplatAbleAttempt3TheFinally : MonoBehaviour
{
    // get sillouet planes
    Mesh mesh;

    Vector3 camPos;

    private void Awake()
    {
        
        mesh = GetComponent<MeshFilter>().mesh;
        camPos = Camera.main.transform.position;
    }

    private void OnMouseDown()
    {
        Silhouette silhouette = new Silhouette(mesh, camPos);

    }
}

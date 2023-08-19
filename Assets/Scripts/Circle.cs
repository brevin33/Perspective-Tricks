using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circle : MonoBehaviour
{
    [SerializeField]
    GameObject g;

    Vector3 camPos;
    // Start is called before the first frame update
    private void Awake()
    {
        camPos = Camera.main.transform.position;
        
    }

    private void OnDrawGizmos()
    {
        Silhouette silhouette = new Silhouette(g.name, camPos);
        if (silhouette.pointBehind(transform.position))
        {
            Gizmos.DrawSphere(transform.position, .2f);
        }
        else
        {
            Gizmos.DrawCube(transform.position, Vector3.one * .2f);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Silhouette
{

    string name;
    Vector3 viewPos;
    public Silhouette(string Name, Vector3 ViewPos)
    {
        name = Name;
        viewPos = ViewPos;
    }

    public bool pointBehind(Vector3 point)
    {
        RaycastHit hit;
        if (Physics.Raycast(viewPos, point - viewPos,out hit))
        {
            return hit.transform.gameObject.name == name;
        }
        return false;
    }

    public bool pointBehind(Vector3 point, out RaycastHit hit)
    {
        if (Physics.Raycast(viewPos, point - viewPos, out hit))
        {
            return hit.transform.gameObject.name == name;
        }
        return false;
    }

    public Vector3 findPointInSilhouetteAlongEdge(Vector3 inSilhouette, Vector3 outSilhouette, int accuracy)
    {
        float inBetween = 0.5f;
        float change = 0.25f;
        Vector3 point = Vector3.Lerp(inSilhouette,outSilhouette,inBetween);
        for (int i = 0;i < accuracy; i++) {
            if (pointBehind(point))
            {
                inBetween += change;
            }
            else
            {
                inBetween -= change;
            }
            change *= .5f;
            point = Vector3.Lerp(inSilhouette, outSilhouette, inBetween);
        }

        return point;
    }

    public VertInfo findVertInfoInSilhouetteAlongEdge(VertInfo inSilhouette, VertInfo outSilhouette, int accuracy)
    {
        float inBetween = 0.5f;
        float change = 0.25f;
        Vector3 inSilhouettePoint = inSilhouette.pos;
        Vector3 outSilhouettePoint = outSilhouette.pos;
        Vector3 point = Vector3.Lerp(inSilhouettePoint, outSilhouettePoint, inBetween);
        for (int i = 0; i < accuracy; i++)
        {
            if (pointBehind(point))
            {
                inBetween += change;
            }
            else
            {
                inBetween -= change;
            }
            change *= .5f;
            point = Vector3.Lerp(inSilhouettePoint, outSilhouettePoint, inBetween);
        }

        return new VertInfo
        {
            pos = point,
            normal = Vector3.Lerp(inSilhouette.normal, outSilhouette.normal, inBetween),
            uv = Vector2.Lerp(inSilhouette.uv, outSilhouette.uv, inBetween),
            uv2 = Vector2.Lerp(inSilhouette.uv2, outSilhouette.uv2, inBetween),
            tangent = Vector4.Lerp(inSilhouette.tangent, outSilhouette.tangent, inBetween)
        };
    }



}

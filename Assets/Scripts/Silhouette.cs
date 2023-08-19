using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Silhouette
{

    string n;
    Vector3 viewPos;
    public Silhouette(string Name, Vector3 ViewPos)
    {
        n = Name;
        viewPos = ViewPos;
    }

    public bool pointBehind(Vector3 point)
    {
        RaycastHit hit;
        if (Physics.Raycast(point, viewPos - point, out hit))
        {
            return hit.transform.gameObject.name == n;
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

    public void setVertInfoUVs(ref VertInfo vertInfo)
    {
        RaycastHit hit;
        if (Physics.Raycast(viewPos, vertInfo.pos - viewPos, out hit))
        {
            vertInfo.uv = hit.textureCoord;
            vertInfo.uv2 = hit.textureCoord2;
        }
    }

    public VertInfo findVertInfoInSilhouetteAlongEdge(VertInfo inSilhouette, VertInfo outSilhouette, int accuracy)
    {
        float inBetween = 0.5f;
        float change = 0.25f;
        Vector3 inSilhouettePoint = inSilhouette.pos;
        Vector3 outSilhouettePoint = outSilhouette.pos;
        Vector3 point = Vector3.Lerp(inSilhouettePoint, outSilhouettePoint, inBetween);
        Vector3 outPoint = point;
        for (int i = 0; i < accuracy; i++)
        {
            if (pointBehind(point))
            {
                inBetween += change;
                outPoint = point;
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
            pos = outPoint,
            normal = Vector3.Lerp(inSilhouette.normal, outSilhouette.normal, inBetween),
            tangent = Vector4.Lerp(inSilhouette.tangent, outSilhouette.tangent, inBetween)
        };
    }



}

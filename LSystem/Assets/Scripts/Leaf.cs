using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leaf{

    int colourDiv = 2;
    int positionMulti = 65;
    Vector3 Origin;
    int positionOff;
    public Leaf(TransformInfo transformInfo, ref Texture2D texture, Vector3 or, ref GameObject a_Leaves) {
        Origin = or;
        positionOff = (texture.width / 2) - (25);
        GameObject gm = new GameObject("Leaf");
        SetTransform(ref gm, transformInfo);
        gm.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        gm.tag = "Validate";
        gm.transform.SetParent(a_Leaves.transform);
        Vector3 v3 = MapFromWorldSpace(gm);
        DrawCircle(ref texture, new Color(ColourFloat(gm), ColourFloat(gm), ColourFloat(gm)), (int)v3.x, (int)v3.z);
        texture.Apply();
    }
    public void DrawCircle(ref Texture2D tex, Color color, int x, int y, int radius = 3)
    {
        float rSquared = radius * radius;
        for (int u = x - radius; u < x + radius + 1; u++)
            for (int v = y - radius; v < y + radius + 1; v++)
                if ((x - u) * (x - u) + (y - v) * (y - v) < rSquared)
                    tex.SetPixel(u, v, color);
    }
    private float ColourFloat(GameObject gm) { return gm.transform.position.y / colourDiv; }
    private void SetTransform(ref GameObject a, TransformInfo b)
    {
        a.transform.position = b.transform.position;
        a.transform.rotation = b.transform.rotation;
    }
    Vector3 MapFromWorldSpace(GameObject gm)
    {
        return new Vector3(
            positionOff + ((gm.transform.position.x - Origin.x) * positionMulti),
            0,
            positionOff + ((gm.transform.position.z - Origin.z) * positionMulti));
    }
}

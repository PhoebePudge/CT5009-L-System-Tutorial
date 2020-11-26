using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Plant.Utilities;

public class Leaf{
    TransformInfo transforminfo;
    GameObject gm; 
    int positionMulti = 65;
    Vector3 Origin;
    int positionOff;
    public Leaf(TransformInfo transInfo, ref Texture2D texture, Vector3 or, ref GameObject a_Leaves, PlantGen _instance) {
        transforminfo = transInfo;
        Origin = or;
        positionOff = (texture.width / 2) - (25);
        gm = new GameObject("Leaf");
        transforminfo.SetTransform(ref gm);
        gm.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        gm.tag = "Validate";
        gm.transform.SetParent(a_Leaves.transform);
        Vector3 v3 = MapFromWorldSpace(gm);
        TextureExstension.DrawCircle(ref texture, new Color(ColourFloat(gm), ColourFloat(gm), ColourFloat(gm)), (int)v3.x, (int)v3.z);
        texture.Apply();
    }
    private float ColourFloat(GameObject gm) { return gm.transform.position.y / 2; } 
    Vector3 MapFromWorldSpace(GameObject gm) {
        return new Vector3(
            positionOff + ((gm.transform.position.x - Origin.x) * positionMulti),
            0,
            positionOff + ((gm.transform.position.z - Origin.z) * positionMulti));
    }
    public void Draw() {
        gm.GetComponent<MeshFilter>().mesh = MeshExstension.PrimitiveShape(PrimitiveType.Sphere);
    } 
}

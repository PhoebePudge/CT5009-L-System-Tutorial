using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Plant.Utilities {
    public static class TextureExstension {
        public static void DrawCircle(ref Texture2D tex, Color color, int x, int y, int radius = 3) {
            float rSquared = radius * radius;
            for (int u = x - radius; u < x + radius + 1; u++)
                for (int v = y - radius; v < y + radius + 1; v++)
                    if ((x - u) * (x - u) + (y - v) * (y - v) < rSquared)
                        tex.SetPixel(u, v, color);
        }
    }
    public static class MeshExstension{
        private static Mesh Rectangle() {
            List<Vector3> vertices = new List<Vector3>{
                new Vector3 (-0.5f , -0.5f, -0.5f ),    new Vector3 ( 0.5f , -0.5f, -0.5f ),
                new Vector3 ( 0.5f ,  0.5f, -0.5f ),    new Vector3 (-0.5f ,  0.5f, -0.5f ),
                new Vector3 (-0.5f ,  0.5f,  0.5f ),    new Vector3 ( 0.5f ,  0.5f,  0.5f ),
                new Vector3 ( 0.5f , -0.5f,  0.5f ),    new Vector3 (-0.5f , -0.5f,  0.5f ),};
            List<int> triangles = new List<int>{
                2, 3, 4, 2, 4, 5,
                1, 2, 5, 1, 5, 6,
                0, 7, 4, 0, 4, 3,
                0, 6, 7, 0, 1, 6};
            Mesh msh = new Mesh();
            msh.vertices = vertices.ToArray();
            msh.triangles = triangles.ToArray();
            msh.RecalculateBounds();
            msh.RecalculateNormals();
            msh.name = "Rect";
            return msh;
        }
        public static Mesh PrimitiveShape(PrimitiveType type){
            GameObject gm = GameObject.CreatePrimitive(type);
            Mesh m = gm.GetComponent<MeshFilter>().mesh;
            gm.AddComponent<Destroy>();
            return m;
        }
    }
}

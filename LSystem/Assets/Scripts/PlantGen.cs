using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEditor;
using System;
public class Lsystem { 
    public Lsystem(ref string s, Dictionary<char, string> rule, char axiom, int appendmentChance) {
        StringBuilder sb = new StringBuilder();
        if (appendmentChance != 0 & s != axiom.ToString()) {
            foreach (char c in s) {
                if (rule.ContainsKey(c)) {
                    if (UnityEngine.Random.Range(0, 100) > appendmentChance) {
                        sb.Append(rule[c]);
                    }
                } else {
                    sb.Append(c.ToString());
                }
            }
        } else {
            foreach (char c in s) {
                sb.Append(rule.ContainsKey(c) ? rule[c] : c.ToString());
            }
        }
        s = sb.ToString();
    } 
}
public class PlantGen : MonoBehaviour {
    #region varibles
    //Pillar
    [HideInInspector] public bool pillarGeneration = true;
    [HideInInspector] [Range(1, 4)] public int pillarHeight = 3;

    //colour settings
    [HideInInspector] public Color Col1 = new Color(255 / 219, 255 / 116, 255 / 80, 1);
    [HideInInspector] public Color Col2 = new Color(255, 255, 255, 1);
    [HideInInspector] public bool FlipColour = false;
    [HideInInspector] public bool SolidColour = true;

    //Display
    [HideInInspector] public bool rotate = true;
    [HideInInspector] public int RotationAngle = 90;

    [Header("Plant Generation")]
    [SerializeField] private float angle = 25.000f;
    [SerializeField] private bool rotateLocal = true;

    [Header("Branch Generation Settings")]
    [SerializeField] [Range(0, 2.0f)] private float branchLength = 10.0f; private float _branchLength;
    [SerializeField] [Range(1, 15)] private float branchThickness = 8;
    [SerializeField] [Range(0, 15)] private float branchLengthDivider = 8;
    [SerializeField] [Range(1, 5)] private int generationHeight = 5;
    [SerializeField] [Range(1, 2)] private float branchMultiplier = 1.0f; private float _branchMultiplier;
    [SerializeField] [Range(0, 1)] private float lengthVariance = 0.10f;
    [SerializeField] [Range(0, 100)] private int BranchingChance = 50;
    [SerializeField] [Range(0f, 1f)] private float RandomMultiplier = 0.1f;
    private enum Shape { PrimitiveCube, PrimitiveCylinder }
    [SerializeField] private Shape shape = Shape.PrimitiveCube;

    [Header("Material")]
    [SerializeField] [Range(0.03f, 1)] float MaterialShinyness = 0.078125f;
    [SerializeField] [Range(0f, 1f)] float MaterialEmision = 1;

    //rules
    [SerializeField] public char axiom = 'F';
    private string currentString = string.Empty;
    [HideInInspector] public string dictionaryString = "F+[+F-F-F]-[--F+F+F]";
    private Dictionary<char, string> rules;

    //mesh
    private List<MeshFilter> meshObjectList;
    private GameObject MeshObject;
    private GameObject a_Leaves;
    private Sprite sprite;
    private Texture2D texture;

    //transform 
    [HideInInspector] public bool Validate = false;
    private Stack<TransformInfo> transformStack;
    private Vector3 Origin;
    GameObject TreeDrawer;

    //Leaf generation
    List<Leaf> leaves = new List<Leaf>();
    [SerializeField] private bool MakeLeaves = false;

    #endregion
    #region file data
    private string ExportIntString(params int[] input) {
        string output = "";
        foreach (int item in input) 
            output += item.ToString() + ","; 
        return output;
    }
    private string ExportFloatString(params float[] input) {
        string output = "";
        foreach (float item in input) 
            output += item.ToString() + ","; 
        return output;
    }
    private string ExportBoolString(params bool[] input) {
        string output = "";
        foreach (bool item in input)
            output += item.ToString() + ",";
        return output;
    }
    private string ExportStringString(params string[] input) {
        string output = "";
        foreach (string item in input)
            output += item.ToString() + ",";
        return output;
    }  
    public string ExportVariablesString(string path = "Assets/Resources/VariableExport.txt") {
            //colorus, shape, char
            string output = ExportIntString(pillarHeight, RotationAngle, generationHeight, BranchingChance) + ";" + 
            ExportFloatString(angle, branchLength, branchThickness, branchLengthDivider, branchMultiplier, lengthVariance, RandomMultiplier, MaterialShinyness, MaterialEmision) + ";" + 
            ExportBoolString(pillarGeneration, FlipColour, SolidColour, rotate, rotateLocal) + ";" + 
            ExportStringString(dictionaryString);
        Debug.Log(output); 

        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(ExportIntString(pillarHeight, RotationAngle, generationHeight, BranchingChance));
        writer.WriteLine(ExportFloatString(angle, branchLength, branchThickness, branchLengthDivider, branchMultiplier, lengthVariance, RandomMultiplier, MaterialShinyness, MaterialEmision));
        writer.WriteLine(ExportBoolString(pillarGeneration, FlipColour, SolidColour, rotate, rotateLocal));
        writer.WriteLine(ExportStringString(dictionaryString));
        writer.Close();

        //Re-import the file to update the reference in the editor
        AssetDatabase.ImportAsset(path);
        //TextAsset asset = Resources.Load("test");

        return output;
    }
    public void ImportVariablesSstring() {
        string path = "Assets/Resources/VariableExport.txt"; 
        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(path);
        int linecount = 0;
        
        for (string line = reader.ReadLine(); line != null; line = reader.ReadLine()) {
            string v = "";
            int wordcount = 0;
            foreach (char c in line) {
                if (c != ',') {
                    v += c.ToString();
                } else {
                    Debug.Log(v);
                    v = "";
                    wordcount++;
                }
            }
            linecount++;
        }
        reader.Close();
    }
    #endregion
    #region general
    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        DrawCube(gameObject.transform.position, gameObject.transform.rotation, gameObject.transform.localScale);
    }
    public static void DrawCube(Vector3 position, Quaternion rotation, Vector3 scale) {
        position = new Vector3(position.x , position.y + (scale.y / 2),position.z);
        Matrix4x4 cubeTransform = Matrix4x4.TRS(position , rotation, scale);
        Matrix4x4 oldGizmosMatrix = Gizmos.matrix; 
        Gizmos.matrix *= cubeTransform; 
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one); 
        Gizmos.matrix = oldGizmosMatrix;
    }
    public bool Validation() { if (branchMultiplier == 0) return false; else return true; }
    private void OnValidate() { if (Application.isPlaying & Validation()) { Validate = true; } }
    private void Start(){ 
        TreeDrawer = new GameObject("TreeDrawer");
        TreeDrawer.tag = "Validate";

        texture = new Texture2D(128,128);
        sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f),100f); 
        GameObject.Find("MapDisplay").GetComponent<SpriteRenderer>().sprite = sprite;

        for (int x = 0; x < texture.width; x++) 
            for (int y = 0; y < texture.height; y++) 
                texture.SetPixel(x,y,Color.white); 

        a_Leaves = new GameObject("Leaves");
        rules = new Dictionary<char, string> { { axiom, dictionaryString } };
        transform.Rotate(Vector3.right * -90.0f);
        TreeDrawer.transform.rotation = transform.rotation;
        Origin = transform.position;
    } 
    public void Rotation() { transform.eulerAngles = new Vector3(transform.eulerAngles.x, 0 + RotationAngle, transform.eulerAngles.z); }
    #endregion
    private void Make(){
        rules = new Dictionary<char, string> { { axiom, dictionaryString } };

        if (MeshObject != null)  
            Destroy(MeshObject); 

        meshObjectList = new List<MeshFilter>();
        _branchLength = branchLength;
        _branchMultiplier = 1;

        foreach (GameObject item in GameObject.FindGameObjectsWithTag("Validate")) 
            Destroy(item); 
        
        transformStack = new Stack<TransformInfo>();
        currentString = axiom.ToString(); 


        TreeDrawer = new GameObject("TreeDrawer");
        TreeDrawer.tag = "Validate";
        TreeDrawer.transform.rotation = transform.rotation;
        TreeDrawer.transform.position = Origin;

        if (pillarGeneration) {
            //perform the numbers of generation in a pillar
            for (int i = 0; i < generationHeight; i++)
                new Lsystem(ref currentString, rules, axiom, BranchingChance); 
            //add this pillar for the height
            for (int i = 0; i < pillarHeight; i++) 
                currentString += currentString; 
            Gen();
        } else { 
            for (int i = 0; i < generationHeight; i++)
                new Lsystem(ref currentString, rules, axiom, BranchingChance); 
            Gen();
        }
        CombineMeshes();
        foreach (Leaf item in leaves)
        {

        }
    }
    private void Update(){
        if (Validate){
            Validate = false;
            Make();
        }
        if (rotate)  
            transform.Rotate(Vector3.forward * (Time.deltaTime * 20f)); 
    } 
    //Branch
    private Vector3 Move(Vector3 direction, float length){
        TreeDrawer.transform.Translate(Vector3.forward * (length + (UnityEngine.Random.Range(0, lengthVariance * 100f) / 100f)));
        return TreeDrawer.transform.position;
    }

    //Branch Mesh
    private Material GetMaterial() {
        Material m = Resources.Load<Material>("Standard");
        m.SetFloat("_Emission", MaterialEmision);
        m.SetFloat("_Shininess", MaterialShinyness); 
        return m;
    }
    private void CombineMeshes(){
        MeshObject = new GameObject("MeshObject", typeof(MeshFilter), typeof(MeshRenderer));
        MeshObject.GetComponent<MeshRenderer>().material = GetMaterial();
        List<Color> colours = new List<Color>(); 
        CombineInstance[] combine = new CombineInstance[meshObjectList.Count];
        int i = 0;
        while (i < meshObjectList.Count){
            if (meshObjectList[i] != null){
                combine[i].mesh = meshObjectList[i].sharedMesh;
                combine[i].transform = meshObjectList[i].transform.localToWorldMatrix;
                for (int d = 0; d < combine[i].mesh.vertexCount; d++)  { 
                    colours.Add(meshObjectList[i].GetComponent<MeshRenderer>().material.color); 
                }; 
                Destroy(meshObjectList[i].gameObject);
            }
            i++;
        }
        
        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine);
        combinedMesh.RecalculateBounds();
        combinedMesh.RecalculateNormals();
        combinedMesh.colors = colours.ToArray();
        MeshObject.GetComponent<MeshFilter>().mesh = combinedMesh;
        MeshObject.transform.SetParent(transform); 
    }  

    private void Gen() {
        TransformInfo highestLeaf = new TransformInfo();
        foreach (char c in currentString) {
            TransformInfo current = new TransformInfo(new TransformHolder(TreeDrawer.transform), _branchLength * _branchMultiplier, null);
            float l = _branchLength + (UnityEngine.Random.Range(0, lengthVariance * 100f) / 100f);
            switch (c) {
                case 'F': 
                    _branchLength /= branchMultiplier; 
                    DrawBranch(current.transform.position, Move(Vector3.forward, l), _branchLength, out current);  
                    break; 
                case 'B':
                    _branchLength /= branchMultiplier; 
                    DrawBranch(current.transform.position, Move(-Vector3.forward, l), _branchLength, out current); 
                    break; 
                case '+': TreeDrawer.transform.Rotate(Vector3.right   *  angle); break; //pitch
                case '-': TreeDrawer.transform.Rotate(Vector3.right   * -angle); break; //pitch 
                case '{': TreeDrawer.transform.Rotate(Vector3.up      *  angle); break; //yaw
                case '}': TreeDrawer.transform.Rotate(Vector3.up      * -angle); break; //yaw 
                case '<': TreeDrawer.transform.Rotate(Vector3.forward *  angle); break; //roll
                case '>': TreeDrawer.transform.Rotate(Vector3.forward * -angle); break; //roll 
                case '[': 
                    transformStack.Push(new TransformInfo(current.transform, _branchLength, current.mesh));
                    if (MakeLeaves) 
                        CreateLeaf(highestLeaf);
                    break; 
                case ']': 
                    highestLeaf = current;
                    current = transformStack.Pop(); 
                    SetTransform(ref TreeDrawer, current); 
                    _branchLength = current.branchLength;
                    break; 
                default: throw new InvalidOperationException("Invalid L-tree operation");
            } 
        }
    }
    private Color ColourMaterial(Color Col1, Color Col2) {
        if (SolidColour)
            return Col1;
        else
            return Color.Lerp(Col2, Col1, (float)transformStack.Count / 3f);
    }
    private void DrawBranch(Vector3 pA, Vector3 pB, float length, out TransformInfo ti){
        GameObject gm = new GameObject("Branch", typeof(MeshFilter), typeof(MeshRenderer)); 
        Vector3 between = pB - pA;
        float thicknessMultiplier = (float)(UnityEngine.Random.Range(0f, 5f) * (float)RandomMultiplier) / 50f;

        if (shape == Shape.PrimitiveCube) { 
            gm.GetComponent<MeshFilter>().mesh = PrimitiveShape(PrimitiveType.Cube);
            gm.transform.localScale = new Vector3((length) / branchThickness + thicknessMultiplier, (between.magnitude) * 1f, (length) / branchThickness + thicknessMultiplier);
        } else {
            gm.GetComponent<MeshFilter>().mesh = PrimitiveShape(PrimitiveType.Cylinder);
            gm.transform.localScale = new Vector3((length) / branchThickness + thicknessMultiplier, (between.magnitude) * 0.5f, (length) / branchThickness + thicknessMultiplier);
        }

        if (FlipColour) gm.GetComponent<MeshRenderer>().material.color = ColourMaterial(Col2, Col1);
        else gm.GetComponent<MeshRenderer>().material.color = ColourMaterial(Col1, Col2); 
        
        gm.transform.localPosition = pA + (between / 2.0f);
        gm.transform.LookAt(pB);
        gm.transform.Rotate(90, 0, 0);
        gm.tag = "Validate"; 
        ti = new TransformInfo( new TransformHolder(gm.transform), length, gm.GetComponent<MeshFilter>().mesh); 
        meshObjectList.Add(gm.GetComponent<MeshFilter>());
    } 
    private Mesh Rectangle(){
        List<Vector3> vertices = new List<Vector3>{
        new Vector3 (-0.5f , -0.5f, -0.5f ),    new Vector3 ( 0.5f , -0.5f, -0.5f ),
        new Vector3 ( 0.5f ,  0.5f, -0.5f ),    new Vector3 (-0.5f ,  0.5f, -0.5f ),
        new Vector3 (-0.5f ,  0.5f,  0.5f ),    new Vector3 ( 0.5f ,  0.5f,  0.5f ),
        new Vector3 ( 0.5f , -0.5f,  0.5f ),    new Vector3 (-0.5f , -0.5f,  0.5f ),};
        List<int> triangles =  new List<int>{
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
    private void SetTransform(ref GameObject a, TransformInfo b) {
        a.transform.position = b.transform.position;
        a.transform.rotation = b.transform.rotation;
    }
    public Mesh PrimitiveShape(PrimitiveType type) {
        GameObject gm = GameObject.CreatePrimitive(type);
        Mesh m = gm.GetComponent<MeshFilter>().mesh;
        Destroy(gm);
        return m;
    } 
    GameObject DrawSphere(Vector3 pos, float size, Color c) {
        GameObject g = new GameObject("Giz " + c, typeof(MeshRenderer), typeof(MeshFilter));
        g.AddComponent<MeshFilter>().mesh = PrimitiveShape(PrimitiveType.Sphere);
        g.GetComponent<MeshRenderer>().material.color = c;
        g.transform.position = pos;
        g.transform.localScale = new Vector3(size, size, size);
        return g;
    } 
    //Leaf  
    private void CreateLeaf(TransformInfo ti) {
        leaves.Add(new Leaf(ti, ref texture, transform.position, ref a_Leaves));
    }  
}
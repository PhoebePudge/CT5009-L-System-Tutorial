using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEditor;
using System;
using Plant.Utilities;

public class PlantGen : MonoBehaviour {
    #region varibles
    public EditorVar EditorVariables;
    public struct EditorVar{ 
        public int[] _choiceIndex;
        public int _prevRotation;
    }
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
    [HideInInspector] public char[] axiom = new char[2];
    [HideInInspector] public bool doubleAxomAmmendment = true; 
    private string currentString = string.Empty;
    [HideInInspector] public string[] dictionaryString = new string[2];
    private Dictionary<char, string>[] rules = new Dictionary<char, string>[2];

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
 
    List<Leaf> leaves = new List<Leaf>();
    [SerializeField] private bool MakeLeaves = false;
    [SerializeField] private bool MeshLeaves = false;
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
        rules[0] = new Dictionary<char, string> { { axiom[0], dictionaryString[0]} };
        rules[1] = new Dictionary<char, string> { { axiom[1], dictionaryString[1]} };
        transform.rotation = new Quaternion();
        transform.Rotate(Vector3.right * -90.0f);
        transform.Rotate(Vector3.forward * -90.0f);
        TreeDrawer.transform.rotation = transform.rotation;
        Origin = transform.position;
    } 
    public void Rotation() { transform.eulerAngles = new Vector3(transform.eulerAngles.x, 0 + RotationAngle, transform.eulerAngles.z); }
    #endregion
    public void SaveToPrefab(string filepath) {
        bool success;
        PrefabUtility.SaveAsPrefabAsset(gameObject, filepath, out success);
        if (success)
        Debug.Log("Success in saving file to: " + filepath);
        else 
        Debug.Log("Failed in saving file to: " + filepath); 
    } 
    private void Make(){
        rules[0] = new Dictionary<char, string> { { axiom[0], dictionaryString[0] } };
        rules[1] = new Dictionary<char, string> { { axiom[1], dictionaryString[1] } };

        if (MeshObject != null)  
            Destroy(MeshObject); 

        meshObjectList = new List<MeshFilter>();
        _branchLength = branchLength;
        _branchMultiplier = 1;

        foreach (GameObject item in GameObject.FindGameObjectsWithTag("Validate")) 
            Destroy(item); 
        
        transformStack = new Stack<TransformInfo>();
        if (doubleAxomAmmendment)
            currentString = axiom[0].ToString() + axiom[1].ToString();
        else
        currentString = axiom[0].ToString();
         
        TreeDrawer = new GameObject("TreeDrawer");
        TreeDrawer.tag = "Validate";
        TreeDrawer.transform.rotation = transform.rotation;
        TreeDrawer.transform.position = Origin; 
        if (pillarGeneration) {
            //perform the numbers of generation in a pillar
            for (int i = 0; i < generationHeight; i++)
                if (doubleAxomAmmendment)
                    new Lsystem(ref currentString, rules, axiom, BranchingChance);
                else
                    new Lsystem(ref currentString, rules[0], axiom[0], BranchingChance); 
            //add this pillar for the height
            for (int i = 0; i < pillarHeight; i++) 
                currentString += currentString; 
            Gen();
        } else { 
            for (int i = 0; i < generationHeight; i++)
                if (doubleAxomAmmendment)
                    new Lsystem(ref currentString, rules, axiom, BranchingChance);
                else
                    new Lsystem(ref currentString, rules[0], axiom[0], BranchingChance);
            Gen();
        }
        CombineMeshes();
        if (MeshLeaves) {
            MeshifyLeaves();
        } else {
            foreach (Leaf item in leaves) {
                item.Draw();
            }
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
                case 'R':
                    break;
                case 'L':
                    break;
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
                    current.SetTransform(ref TreeDrawer); 
                    _branchLength = current.branchLength;
                    break; 
                default: throw new InvalidOperationException("Invalid L-tree operation with character: " + c);
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
            gm.GetComponent<MeshFilter>().mesh = MeshExstension.PrimitiveShape(PrimitiveType.Cube);
            gm.transform.localScale = new Vector3((length) / branchThickness + thicknessMultiplier, (between.magnitude) * 1f, (length) / branchThickness + thicknessMultiplier);
        } else {
            gm.GetComponent<MeshFilter>().mesh = MeshExstension.PrimitiveShape(PrimitiveType.Cylinder);
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
    GameObject DrawSphere(Vector3 pos, float size, Color c) {
        GameObject g = new GameObject("Giz " + c, typeof(MeshRenderer), typeof(MeshFilter));
        g.AddComponent<MeshFilter>().mesh = MeshExstension.PrimitiveShape(PrimitiveType.Sphere);
        g.GetComponent<MeshRenderer>().material.color = c;
        g.transform.position = pos;
        g.transform.localScale = new Vector3(size, size, size);
        return g;
    } 
    //Leaf  
    private void CreateLeaf(TransformInfo ti) {
        leaves.Add(new Leaf(ti, ref texture, transform.position, ref a_Leaves, this));
    }  
    private void MeshifyLeaves(){
    } 
}
class Utilities
{
    public void DoSomething()
    {

    }
}
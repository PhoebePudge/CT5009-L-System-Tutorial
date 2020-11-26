using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
public class PlantTools{ 
    [MenuItem("GameObject/CreatePlant")] private static void CreatePlant() {
        if (EditorUtility.DisplayDialog("Plant Generator", "Do you want to create a plant?", "Create", "Cancel")) {
            int Index = 1;
            foreach (GameObject gameObj in GameObject.FindObjectsOfType<GameObject>()) {
                if (gameObj.name.Contains("Plant")) 
                    Index++; 
            } 
            GameObject gm = new GameObject("Plant " + Index);
            gm.AddComponent<PlantGen>();
        }
    }  
} 
[CustomEditor(typeof(PlantGen))] public class PlantUI : Editor {
    static PlantGen myScript;
    int[] _choiceIndex = new int[2] { 0 , 0}; 
    int _prevRotation = 0; 
    string[] _choices = new[] { 
        "F+[+F-F-F]-[--F+F+F]", 
        "+<BF+<BFB{F+>>BFB-F}>>BFB{F>B{>", //should make a hillbert cube
        "FF-[-F+F+F]+[+F-F-F]", 
        "F+F--F+F",
        "+RF-LFL-FR+",
        "-LF+RFR+FL-", 
        "Custom String"}; // custom
    void OnEnable() {
        if (myScript == null) {
            myScript = (PlantGen)target;
        }
        _choiceIndex = myScript.EditorVariables._choiceIndex;
        _prevRotation = myScript.EditorVariables._prevRotation;

        if (_choiceIndex == null) {
            _choiceIndex = new int[2] { 0, 0 };
            _prevRotation = 0;
        }
    }
    private void OnDisable() {
        myScript.EditorVariables._choiceIndex = _choiceIndex;
        myScript.EditorVariables._prevRotation = _prevRotation;
    }
    public override void OnInspectorGUI() { 
        EditorGUI.BeginChangeCheck();
        
        #region Prefab Loading
        string path = "Assets/Resources/Prefabs/";  
        string fileExstension = "prefab";
        //Button save
        if (GUILayout.Button("Save to prefab")) {
            string filepath = EditorUtility.SaveFilePanel("Select location to save Prefab", path, "", fileExstension); 
            if (filepath.Length != 0) { 
                myScript.SaveToPrefab(filepath); 
            } 
            Validate(myScript);
        }
        //Button load
        if (GUILayout.Button("Load from prefab")) {
            string filepath = EditorUtility.OpenFilePanel("Select Prefab to Open", path, fileExstension);
            if (filepath.Length != 0) {
                GameObject gm = Instantiate<GameObject>(PrefabUtility.LoadPrefabContents(filepath));
                Debug.Log("Asset was found and loaded!");
                gm.transform.rotation = myScript.gameObject.transform.rotation;
                gm.name = myScript.gameObject.name;
                myScript.gameObject.AddComponent<Destroy>();
            } else {
                Debug.LogError("Failed loading prefab contents");
            } 
            Validate(myScript); 
        }
        #endregion

        #region Dictionary String 
        //1
        myScript.doubleAxomAmmendment = EditorGUILayout.Toggle("Use two Axioms", myScript.doubleAxomAmmendment);
        _choiceIndex[0] = EditorGUILayout.Popup(_choiceIndex[0], _choices);
        if (_choiceIndex[0] == _choices.Length - 1)
            myScript.dictionaryString[0] = EditorGUILayout.TextField("Custom Dictionary String", myScript.dictionaryString[0]);
        else
            myScript.dictionaryString[0] = _choices[_choiceIndex[0]];
        //2
        if (myScript.doubleAxomAmmendment) {
            _choiceIndex[1] = EditorGUILayout.Popup(_choiceIndex[1], _choices);
            if (_choiceIndex[1] == _choices.Length - 1)
                myScript.dictionaryString[1] = EditorGUILayout.TextField("Custom Dictionary String", myScript.dictionaryString[1]);
            else
                myScript.dictionaryString[1] = _choices[_choiceIndex[1]];
        }
        #endregion

        #region Axoim
        myScript.axiom[0] = EditorGUILayout.TextField("Axiom 1", myScript.axiom[0].ToString()).ToCharArray()[0];
        if (myScript.doubleAxomAmmendment) 
        myScript.axiom[1] = EditorGUILayout.TextField("Axiom 1", myScript.axiom[1].ToString()).ToCharArray()[0];
        #endregion

        #region Pillar Generation
        EditorGUILayout.LabelField("Pillar", EditorStyles.boldLabel);
        myScript.pillarGeneration = EditorGUILayout.Toggle("pillarGeneration", myScript.pillarGeneration);
        if (myScript.pillarGeneration)
            myScript.pillarHeight = EditorGUILayout.IntSlider("pillarHeight", myScript.pillarHeight, 1, 5);
        #endregion

        #region Colour
        //Toggle solid and flip
        EditorGUILayout.LabelField("Colour", EditorStyles.boldLabel);
        myScript.SolidColour = EditorGUILayout.Toggle("SolidColour", myScript.SolidColour);
        if (GUILayout.Button("FlipColour"))
            myScript.FlipColour = !myScript.FlipColour; 
        //Solid Colour
        if (myScript.SolidColour) {
            if (myScript.FlipColour)
                myScript.Col2 = EditorGUILayout.ColorField("Col2", myScript.Col2);
            else
                myScript.Col1 = EditorGUILayout.ColorField("Col1", myScript.Col1);
        } else {
            if (myScript.FlipColour) {
                myScript.Col2 = EditorGUILayout.ColorField("Col2", myScript.Col2);
                myScript.Col1 = EditorGUILayout.ColorField("Col1", myScript.Col1);
            } else {
                myScript.Col1 = EditorGUILayout.ColorField("Col1", myScript.Col1);
                myScript.Col2 = EditorGUILayout.ColorField("Col2", myScript.Col2);
            }
        }
        #endregion

        #region Rotation
        //Display rotate toggle
        EditorGUILayout.LabelField("Rotation", EditorStyles.boldLabel);
        myScript.rotate = EditorGUILayout.Toggle("rotate", myScript.rotate);

        //rotation
        if (!myScript.rotate) {
            EditorGUILayout.LabelField("Rotation Angle");
            myScript.RotationAngle = EditorGUILayout.IntSlider(myScript.RotationAngle, 0, 360);
        }
        #endregion

        DrawDefaultInspector();
        if (GUILayout.Button("Recalculate"))
            Validate(myScript);
        //Validate
        if (GUI.changed) 
            myScript.Rotation(); 
        _prevRotation = myScript.RotationAngle; 
    }

    private void Validate(PlantGen myScript) { 
        if (Application.isPlaying & myScript.Validation()) { myScript.Validate = true; }
    } 
} 
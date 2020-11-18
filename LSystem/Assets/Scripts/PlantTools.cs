using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor; 

public class PlantTools {
 
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
    [MenuItem("GameObject/LoadPlantFromFile")] private static void ImportPlantFromString() {

    }
} 
[CustomEditor(typeof(PlantGen))] public class PlantUI : Editor {
    PlantGen myScript;
    int _choiceIndex = 0; 
    int _prevRotation = 0; 
    string[] _choices = new[] { "F+[+F-F-F]-[--F+F+F]", "+<BF+<BFB{F+>>BFB-F}>>BFB{F>B{>", "FF-[-F+F+F]+[+F-F-F]", "F+F--F+F", "Custom String"};
    void OnEnable() { 
        myScript = (PlantGen)target;
        //mat = myScript.gameObject.GetComponentInChildren<MeshRenderer>().material;
        //if (mat != null) 
        //    _materialEditor = (MaterialEditor)CreateEditor(mat); 
    }
    public override void OnInspectorGUI() {
        EditorGUI.BeginChangeCheck();

        // Draw the material field of MyScript
        //EditorGUILayout.PropertyField(serializedObject.FindProperty("material"));

        //if (EditorGUI.EndChangeCheck()) {
        //    serializedObject.ApplyModifiedProperties();
//
        //    if (_materialEditor != null) { 
        //        DestroyImmediate(_materialEditor);
        //    }
    //
        //    if (myScript.gameObject.GetComponent<MeshRenderer>().material != null) { 
        //        _materialEditor = (MaterialEditor)CreateEditor(mat); 
        //    }
        //}


        //if (_materialEditor != null) {
        //    // Draw the material's foldout and the material shader field
            // Required to call _materialEditor.OnInspectorGUI ();
         //   _materialEditor.DrawHeader(); 
            //  We need to prevent the user to edit Unity default materials
      //      bool isDefaultMaterial = !AssetDatabase.GetAssetPath(mat).StartsWith("Assets");

     //       using (new EditorGUI.DisabledGroupScope(isDefaultMaterial)) {

                // Draw the material properties
    //            // Works only if the foldout of _materialEditor.DrawHeader () is open
   //             _materialEditor.OnInspectorGUI();
    //        }
   //     }



        //Button Obj
        if (GUILayout.Button("Export OBJ to file"))
            Validate(myScript);
        //Button Save
        if (GUILayout.Button("Save Settings to file"))
            Validate(myScript);
        //Button Load
        if (GUILayout.Button("Load Settings from file"))
            Validate(myScript);

        //Button choice
        _choiceIndex = EditorGUILayout.Popup(_choiceIndex, _choices);
        if (_choiceIndex == 4)
            myScript.dictionaryString = EditorGUILayout.TextField("Custom Dictionary String", myScript.dictionaryString);
        else
            myScript.dictionaryString = _choices[_choiceIndex];

        //Toggle Pillar
        EditorGUILayout.LabelField("Pillar", EditorStyles.boldLabel);
        myScript.pillarGeneration = EditorGUILayout.Toggle("pillarGeneration", myScript.pillarGeneration);
        if (myScript.pillarGeneration)
            myScript.pillarHeight = EditorGUILayout.IntSlider("pillarHeight", myScript.pillarHeight, 1, 5);

        //Toggle solid and flip
        EditorGUILayout.LabelField("Colour", EditorStyles.boldLabel);
        myScript.SolidColour = EditorGUILayout.Toggle("SolidColour", myScript.SolidColour);
        if (GUILayout.Button("FlipColour"))
            myScript.FlipColour = !myScript.FlipColour;

        //Solid Colour
        if (myScript.SolidColour)
        {
            if (myScript.FlipColour)
                myScript.Col2 = EditorGUILayout.ColorField("Col2", myScript.Col2);
            else
                myScript.Col1 = EditorGUILayout.ColorField("Col1", myScript.Col1);
        }
        else
        {
            if (myScript.FlipColour)
            {
                myScript.Col2 = EditorGUILayout.ColorField("Col2", myScript.Col2);
                myScript.Col1 = EditorGUILayout.ColorField("Col1", myScript.Col1);
            }
            else
            {
                myScript.Col1 = EditorGUILayout.ColorField("Col1", myScript.Col1);
                myScript.Col2 = EditorGUILayout.ColorField("Col2", myScript.Col2);
            }
        }
        //Display rotate toggle
        EditorGUILayout.LabelField("Rotation", EditorStyles.boldLabel);
        myScript.rotate = EditorGUILayout.Toggle("rotate", myScript.rotate);

        //rotation
        if (!myScript.rotate)
        {
            EditorGUILayout.LabelField("Rotation Angle");
            myScript.RotationAngle = EditorGUILayout.IntSlider(myScript.RotationAngle, 0, 360);
        }
        DrawDefaultInspector();
        if (GUILayout.Button("Recalculate"))
            Validate(myScript);
        //Validate
        if (GUI.changed)
        {
            myScript.Rotation();
        }
        _prevRotation = myScript.RotationAngle; 
    }
    private void Validate(PlantGen myScript) { 
        if (Application.isPlaying & myScript.Validation()) { myScript.Validate = true; }
    }
    void OnDisable()
    {
        //if (_materialEditor != null)
        //{
            // Free the memory used by default MaterialEditor
        //    DestroyImmediate(_materialEditor);
        //}
    }
} 
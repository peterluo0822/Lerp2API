﻿using UnityEngine;
using UnityEditor;
using System.IO;

namespace Lerp2API.Utility
{
    public class DummyEditor : EditorWindow
    {
        [MenuItem("Lerp2Dev Team Tools/Save Scene Skin...")]
        static public void SaveEditorSkin(EditorSkin es)
        {
            int c = Directory.GetFiles(Path.Combine(Application.dataPath, "Saved Skins/"), "*.guiskin", SearchOption.AllDirectories).Length;
            string n = "Assets/Saved Skins/SceneSkin - " + c + ".guiskin";
            GUISkin skin = Instantiate(EditorGUIUtility.GetBuiltinSkin(es)) as GUISkin;
            AssetDatabase.CreateAsset(skin, n); //There should be an dialog to set the name!
            EditorUtility.DisplayDialog("API Message", "GUI Skin saved in 'Saved Skins' folder with " + n + " name!", "Ok");
        }
    }
}
// Created by Dennis 'DeeFeS' Schmidt 2020
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System;
using System.Text.RegularExpressions;

namespace DFSSolutions.AssetVariables
{
    public class AssetVariableGeneratorWindow : EditorWindow
    {
        private class TemplateSettings
        {
            public enum Setting : int { DontGenerate = 0, GenerateIfNonexisting, OverwriteExisting };

            public Setting Generation;
            public string FullPath;
        }

        private const string FOLDER_NAME = "AssetVariables";
        private readonly string TEMPLATE_STRUCTURE = FOLDER_NAME + "/Generator/Templates";
        private readonly string TYPEDEF_STRUCTURE = FOLDER_NAME + "/TypeDefinitions";
        private readonly Color[] GENERATION_COLORS = { Color.grey, Color.white, Color.green, };
        private readonly Regex CLASSNAME_REGEX = new Regex("^([A-Za-z_][A-Za-z0-9_]*)$");
        private readonly Regex MULTIPLE_CLASSNAME_REGEX = new Regex("^([A-Za-z_][A-Za-z0-9_]*)(?:\\.([A-Za-z_][A-Za-z0-9_]*))*$");

        private string _TemplatePath = "";
        private string _OutputPath = "";
        private string _Namespace = "UnityEngine";
        private string _Type = "int";
        private string _TypeName = "Integer";
        private bool _ShowTemplates = false;
        private Dictionary<string, TemplateSettings> _TemplatesToShouldGenerate = new Dictionary<string, TemplateSettings>();

        [MenuItem("Tools/DFS Solutions/Asset Variable Generator")]
        public static void ShowWindow()
        {
            var window = GetWindow<AssetVariableGeneratorWindow>("Asset Generator");
            if (window.FindTemplatePath())
                window.FillTemplateDictionary();
        }

        private void OnGUI()
        {
            if (_TemplatePath.Equals(""))
            {
                EditorGUILayout.HelpBox($"Template folder not found!\nExactly this structure is needed:\n{TEMPLATE_STRUCTURE}", MessageType.Error);
                if (GUILayout.Button("Search") && FindTemplatePath())
                    FillTemplateDictionary();
                else
                    GUI.enabled = false;
            }
            else
            {
                GUI.enabled = false;
                _TemplatePath = EditorGUILayout.TextField("Template Path", _TemplatePath);
                GUI.enabled = true;
            }
            _OutputPath = EditorGUILayout.TextField("Output Path", _OutputPath);
            _Namespace = EditorGUILayout.TextField("Namespace", _Namespace);
            _Type = EditorGUILayout.TextField("Type", _Type);
            _TypeName = EditorGUILayout.TextField(new GUIContent("Type Name", "The name for the type that gets used in class names.\n" +
                "(Type: int | Type Name: Integer | Generated: IntegerVariable)"), _TypeName);

            _ShowTemplates = EditorGUILayout.Foldout(_ShowTemplates, "Templates", true);
            if (_ShowTemplates)
            {
                if (GUILayout.Button("Update Templates"))
                    FillTemplateDictionary();

                var keys = _TemplatesToShouldGenerate.Keys.ToList();
                if (keys.Count == 0)
                {
                    FillTemplateDictionary();
                    EditorGUILayout.HelpBox("No templates found.", MessageType.Warning);
                }

                for (int i = 0; i < keys.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal(GUI.skin.box);
                    var ogColor = GUI.contentColor;
                    GUI.contentColor = GENERATION_COLORS[(int)_TemplatesToShouldGenerate[keys[i]].Generation];
                    EditorGUILayout.LabelField(keys[i]);
                    GUI.contentColor = ogColor;

                    ogColor = GUI.backgroundColor;
                    GUI.backgroundColor = GENERATION_COLORS[(int)_TemplatesToShouldGenerate[keys[i]].Generation];
                    _TemplatesToShouldGenerate[keys[i]].Generation = (TemplateSettings.Setting)EditorGUILayout.EnumPopup(_TemplatesToShouldGenerate[keys[i]].Generation);
                    GUI.backgroundColor = ogColor;
                    EditorGUILayout.EndHorizontal();
                }

                GUI.enabled = keys.Count > 0;
                ShowGenerateButton();
            }
            GUI.enabled = true;
        }

        public bool CheckTemplateFolder()
        {
            string appPath = Application.dataPath;
            if (Directory.Exists(appPath + _TemplatePath))
                return true;
            return FindTemplatePath();
        }

        private bool FindTemplatePath()
        {
            string appPath = Application.dataPath;
            if (Directory.Exists(appPath + "/" + TEMPLATE_STRUCTURE))
                _TemplatePath = "/" + TEMPLATE_STRUCTURE;
            else
            {
                _TemplatePath = FindTemplateFolder(appPath);
                _TemplatePath = _TemplatePath.Remove(0, appPath.Length);
            }
            if (_OutputPath == "")
                _OutputPath = $"{_TemplatePath.Remove(_TemplatePath.Length - TEMPLATE_STRUCTURE.Length, TEMPLATE_STRUCTURE.Length)}{TYPEDEF_STRUCTURE}";
            return !_TemplatePath.Equals("");
        }

        private string FindTemplateFolder(string currentPath)
        {
            string[] directories = Directory.GetDirectories(currentPath);
            for (int i = 0; i < directories.Length; i++)
            {
                if (directories[i].Remove(0, currentPath.Length + 1).Equals(FOLDER_NAME))
                {
                    if (Directory.Exists(currentPath + "/" + TEMPLATE_STRUCTURE))
                        return currentPath + "/" + TEMPLATE_STRUCTURE;
                    break;
                }
            }
            for (int i = 0; i < directories.Length; i++)
            {
                string result = FindTemplateFolder(directories[i]);
                if (!result.Equals(""))
                    return result;
            }
            return "";
        }

        private void FillTemplateDictionary()
        {
            var buffer = new Dictionary<string, TemplateSettings>(_TemplatesToShouldGenerate.Count);
            foreach (var item in _TemplatesToShouldGenerate)
                buffer.Add(item.Key, item.Value);

            _TemplatesToShouldGenerate.Clear();
            string appPath = Application.dataPath;
            AddFiles(appPath + _TemplatePath);
            foreach (var item in buffer)
            {
                if (_TemplatesToShouldGenerate.ContainsKey(item.Key))
                    _TemplatesToShouldGenerate[item.Key] = item.Value;
            }
        }

        private void AddFiles(string path)
        {
            string[] files = Directory.GetFiles(path);
            for (int i = 0; i < files.Length; i++)
            {
                if (Path.GetExtension(files[i]).ToLower() != ".txt")
                    continue;

                string name = Path.GetFileNameWithoutExtension(files[i]);
                if (!_TemplatesToShouldGenerate.ContainsKey(name))
                {
                    var settings = new TemplateSettings()
                    {
                        Generation = TemplateSettings.Setting.GenerateIfNonexisting,
                        FullPath = files[i],
                    };
                    _TemplatesToShouldGenerate.Add(name, settings);
                }
            }
            string[] directories = Directory.GetDirectories(path);
            for (int i = 0; i < directories.Length; i++)
                AddFiles(directories[i]);
        }

        private void ShowGenerateButton()
        {
            if (!GUILayout.Button("Generate"))
                return;

            if (!CheckTemplateFolder())
            {
                EditorUtility.DisplayDialog("Error", "Template folder can not be found!\nIt must have been moved or renamed since opening this window.", "Quit");
                return;
            }
            if (!MULTIPLE_CLASSNAME_REGEX.IsMatch(_Namespace))
            {
                EditorUtility.DisplayDialog("Error", "Namespace not accepted!\nIt must follow the rules of a c# namespace.", "Quit");
                return;
            }
            if (!MULTIPLE_CLASSNAME_REGEX.IsMatch(_Type))
            {
                EditorUtility.DisplayDialog("Error", "Type not accepted!\nIt must follow the rules of a c# classname.", "Quit");
                return;
            }
            if (!CLASSNAME_REGEX.IsMatch(_TypeName))
            {
                EditorUtility.DisplayDialog("Error", "Type name not accepted!\nIt must follow the rules of a c# classname.", "Quit");
                return;
            }

            //if(!CheckOutputPath())
            //  return;
            List<string> files = FilterGenerationList();

            string dataPath = Application.dataPath;
            string outPath = dataPath + _OutputPath;
            for (int i = 0; i < files.Count; i += 2)
            {
                string templateText = File.ReadAllText(files[i]);
                templateText = templateText.Replace("$TIME$", DateTime.Now.ToString());
                templateText = templateText.Replace("$TYPE$", _Type);
                templateText = templateText.Replace("$TYPENAME$", _TypeName);
                templateText = templateText.Replace("$NAMESPACE$", _Namespace);
                Directory.CreateDirectory(Path.GetDirectoryName(files[i + 1]));
                File.WriteAllText(files[i + 1], templateText);
            }
            AssetDatabase.Refresh();
        }

        private List<string> FilterGenerationList()
        {
            string dataPath = Application.dataPath;
            string outPath = dataPath + _OutputPath;
            List<string> files = new List<string>();
            foreach (var item in _TemplatesToShouldGenerate)
            {
                if (item.Value.Generation == TemplateSettings.Setting.DontGenerate)
                    continue;

                string fullDir = Path.GetDirectoryName(item.Value.FullPath);
                string folders = fullDir.Remove(0, dataPath.Length + _TemplatePath.Length);
                string path = outPath + "/" + _TypeName + "/";
                if (folders.Length > 0)
                    path += folders + "/" + _TypeName + Path.GetFileNameWithoutExtension(item.Value.FullPath) + ".cs";
                else
                    path += _TypeName + Path.GetFileNameWithoutExtension(item.Value.FullPath) + ".cs";
                if (item.Value.Generation == TemplateSettings.Setting.GenerateIfNonexisting && File.Exists(path))
                {
                    Debug.LogFormat(LogType.Log, LogOption.None, null, "AV Generator: File ignored since it already exists.\n{0}", path);
                    continue;
                }

                files.Add(item.Value.FullPath);
                files.Add(path);
            }
            return files;
        }
    }
}
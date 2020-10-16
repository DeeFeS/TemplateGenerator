// Created by Dennis 'DeeFeS' Schmidt 2020
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DFSSolutions.TemplateGenerator
{
    public class TemplateGeneratorWindow : EditorWindow
    {
        private enum SelectionType : int
        {
            None = 0, Variable, File, Folder,
        }

        private const float INDENT_WIDTH = 10f;
        private const float LEFT_TAB_WIDTH = 500f;
        private const float SETTING_WIDTH = 150f;
        private static TemplateDatabase _Database = default;
        private static Dictionary<TemplateFolder, bool> _FolderToFoldOut = new Dictionary<TemplateFolder, bool>();
        private static List<string> _OutputPath = new List<string>();
        private static string _OutputFolder = "";

        private TemplateFolder _CurrentFolder = default;
        private TemplateFile _CurrentFile = default;
        private TemplateVariable _CurrentVariable = default;
        private SelectionType _CurrentSelection = SelectionType.None;

        [MenuItem("Tools/DFS Solutions/Template Generator")]
        public static void ShowWindow()
        {
            GetWindow<TemplateGeneratorWindow>("Template Generator");
        }

        private void OnEnable()
        {
            InitializeDatabase();
        }

        private void OnDisable()
        {
            _Database = null;
        }

        private static void InitializeDatabase()
        {
            if (_Database != null)
                return;

            _Database = new TemplateDatabase();
            _Database.FillDatabase(Application.dataPath);
            AddAllFolders();
        }

        private static void AddAllFolders()
        {
            Dictionary<TemplateFolder, bool> buffer = _FolderToFoldOut;
            _FolderToFoldOut = new Dictionary<TemplateFolder, bool>();
            foreach (TemplateFolder folder in _Database.Folders)
                AddFolder(folder);

            foreach (KeyValuePair<TemplateFolder, bool> kvp in buffer)
            {
                if (kvp.Value && _FolderToFoldOut.ContainsKey(kvp.Key))
                    _FolderToFoldOut[kvp.Key] = true;
            }
        }

        private static void AddFolder(TemplateFolder folder)
        {
            _FolderToFoldOut.Add(folder, false);
            foreach (TemplateFolder subfolder in folder.Subfolders)
                AddFolder(subfolder);
        }

        private void OnGUI()
        {
            if (_Database == null)
            {
                Debug.LogFormat(LogType.Error, LogOption.None, this, "Template Generator: Database not found.");
                return;
            }
            SetGeneralSettings();

            ShowOutputPath();

            GUIStyle leftTab = new GUIStyle(GUI.skin.box);
            leftTab.fixedWidth = LEFT_TAB_WIDTH;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(leftTab);
            EditorGUILayout.Space();
            ShowVariables();
            EditorGUILayout.Space();
            ShowFolders();
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical(GUI.skin.box);
            ShowSelection();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void ShowOutputPath()
        {
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            EditorGUILayout.LabelField("Output Path:");
            string path = Application.dataPath;
            for (int i = 0; i < _OutputPath.Count; i++)
            {
                if (!ShowDirectories(path, i))
                    break;
                path += "/" + _OutputPath[i];
            }
            ShowLastDirectory(path);
            _OutputFolder = EditorGUILayout.TextField(GUIContent.none, _OutputFolder);
            if (GUILayout.Button("Generate"))
                Generate();
            EditorGUILayout.EndHorizontal();
        }

        private void Generate()
        {

        }

        private bool ShowDirectories(string path, int idx)
        {
            string[] directories = Directory.GetDirectories(path);
            int selection = 0;
            for (int i = 0; i < directories.Length; i++)
            {
                directories[i] = directories[i].Remove(0, path.Length + 1);
                if (_OutputPath[idx].Equals(directories[i]))
                    selection = i;
            }
            selection = EditorGUILayout.Popup(selection, directories);
            if (idx != _OutputPath.Count && !_OutputPath[idx].Equals(directories[selection]))
            {
                _OutputPath[idx] = directories[selection];
                _OutputPath.RemoveRange(idx + 1, _OutputPath.Count - idx - 1);
                return false;
            }

            return true;
        }

        private void ShowLastDirectory(string path)
        {
            string[] directories = Directory.GetDirectories(path);
            if (directories.Length <= 0)
                return;
            for (int i = 0; i < directories.Length; i++)
            {
                directories[i] = directories[i].Remove(0, path.Length + 1);
            }
            int selection = -1;
            selection = EditorGUILayout.Popup(selection, directories);
            if (selection >= 0)
                _OutputPath.Add(directories[selection]);
        }

        private void SetGeneralSettings()
        {
        }

        private void ShowSelection()
        {
            switch (_CurrentSelection)
            {
                case SelectionType.None:
                    break;
                case SelectionType.Variable:
                    ShowSelectedVariable();
                    break;
                case SelectionType.File:
                    ShowSelectedFile();
                    break;
                case SelectionType.Folder:
                    ShowSelectedFolder();
                    break;
                default:
                    break;
            }
        }

        private void ShowVariables()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Variables:");
            foreach (KeyValuePair<string, TemplateVariable> kvp in _Database.VariableNameToData)
                ShowVariable(kvp.Key, kvp.Value);
            EditorGUILayout.EndVertical();
        }

        private void ShowVariable(string name, TemplateVariable variable)
        {
            EditorGUILayout.BeginHorizontal();
            GUIStyle button = new GUIStyle(GUI.skin.button);
            button.alignment = TextAnchor.MiddleLeft;
            button.fixedWidth = LEFT_TAB_WIDTH * 0.5f;
            if (GUILayout.Button(name, button))
                SetSelection(variable);
            variable.DefaultValue = EditorGUILayout.TextField(GUIContent.none, variable.DefaultValue);
            EditorGUILayout.EndHorizontal();
        }

        private void ShowFolders()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Templates:");
            foreach (TemplateFolder folder in _Database.Folders)
                ShowFolder(folder);
            EditorGUILayout.EndVertical();
        }

        private void ShowFolder(TemplateFolder folder)
        {
            ++EditorGUI.indentLevel;
            _FolderToFoldOut[folder] = EditorGUILayout.Foldout(_FolderToFoldOut[folder], new GUIContent(folder.Name), true);
            if (_FolderToFoldOut[folder])
            {
                foreach (TemplateFolder subfolder in folder.Subfolders)
                    ShowFolder(subfolder);
                ShowFiles(folder.Files);
            }
            --EditorGUI.indentLevel;
        }

        private void ShowFiles(List<TemplateFile> files)
        {
            foreach (TemplateFile file in files)
                ShowFile(file);
        }

        private void ShowFile(TemplateFile file)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(EditorGUI.indentLevel * INDENT_WIDTH);
            file.Mode = (TemplateFile.GenerationMode)EditorGUILayout.Popup((int)file.Mode, TemplateFile.SETTING_NAMES, GUILayout.MinWidth(SETTING_WIDTH));
            GUIStyle button = new GUIStyle(GUI.skin.button);
            button.alignment = TextAnchor.MiddleLeft;
            button.fixedWidth = LEFT_TAB_WIDTH - SETTING_WIDTH - (EditorGUI.indentLevel + 1f) * INDENT_WIDTH;
            if (GUILayout.Button($"\"{file.Name}\" >", button))
                SetSelection(file);
            EditorGUILayout.EndHorizontal();
        }

        private void SetSelection(TemplateFile file)
        {
            _CurrentSelection = SelectionType.File;
            _CurrentVariable = null;
            _CurrentFolder = null;
            _CurrentFile = file;
        }

        private void SetSelection(TemplateVariable variable)
        {
            _CurrentSelection = SelectionType.Variable;
            _CurrentVariable = variable;
            _CurrentFolder = null;
            _CurrentFile = null;
        }

        private void SetSelection(TemplateFolder folder)
        {
            _CurrentSelection = SelectionType.Folder;
            _CurrentVariable = null;
            _CurrentFolder = folder;
            _CurrentFile = null;
        }

        private void ShowSelectedVariable()
        {
            GUIContent notSetError = EditorGUIUtility.IconContent("error");
            notSetError.tooltip = "Value has not been set!";

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField($"Variable: {_CurrentVariable.Name}");
            EditorGUILayout.BeginHorizontal();
            _CurrentVariable.DefaultValue = EditorGUILayout.TextField("Value", _CurrentVariable.DefaultValue);
            if (_CurrentVariable.DefaultValue.Equals(null) || _CurrentVariable.DefaultValue.Equals(""))
                EditorGUILayout.LabelField(notSetError, GUIStyle.none, GUILayout.MaxWidth(EditorGUIUtility.singleLineHeight));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Used in:");
            EditorGUILayout.BeginHorizontal();
            foreach (TemplateFile file in _CurrentVariable.Users)
            {
                TemplateFile.VariableSettings settings = file.VariableToSettings[_CurrentVariable.Name];
                GUIContent templateContent = new GUIContent($"\"{file.Name}\"");
                templateContent.tooltip = settings.ToolTip;
                if (settings.Regex != null && !settings.Regex.IsMatch(_CurrentVariable.DefaultValue))
                {
                    templateContent.tooltip += "\r\n\r\nRegex mismatch!";
                    templateContent.image = EditorGUIUtility.IconContent("warning").image;
                }

                if (GUILayout.Button(templateContent, GUI.skin.button, GUILayout.MaxWidth(GUI.skin.label.CalcSize(templateContent).x)))
                    SetSelection(file);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void ShowSelectedFile()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField($"Template: {_CurrentFile.Name}");
            GUIStyle descriptionStyle = new GUIStyle(GUI.skin.GetStyle("label"))
            {
                wordWrap = true
            };
            EditorGUILayout.LabelField(_CurrentFile.Description, descriptionStyle);
            EditorGUILayout.Space();
            string[] keys = _CurrentFile.VariableToSettings.Keys.ToArray();
            for (int i = 0; i < keys.Length; i++)
            {
                EditorGUILayout.LabelField(keys[i]);
                TemplateFile.VariableSettings settings = _CurrentFile.VariableToSettings[keys[i]];
                ShowVariableSettings(keys[i], ref settings);
                EditorGUILayout.Space();

                _CurrentFile.UpdateVariableSettings(keys[i], settings);
            }

            string dependencyAmount = _CurrentFile.DependenciesNameToFile.Count > 0 ? _CurrentFile.DependenciesNameToFile.Count.ToString() : "NONE";
            EditorGUILayout.LabelField($"Dependencies: {dependencyAmount}");
            EditorGUILayout.BeginHorizontal();
            foreach (KeyValuePair<string, TemplateFile> kvp in _CurrentFile.DependenciesNameToFile)
            {
                Color ogColor = GUI.backgroundColor;

                if (kvp.Value == null)
                {
                    GUI.enabled = false;
                    GUI.backgroundColor = Color.red;
                }
                if (GUILayout.Button($"\"{kvp.Key}\"", GUI.skin.button, GUILayout.MaxWidth(GUI.skin.label.CalcSize(new GUIContent($" \"{kvp.Key}\" ")).x)))
                    SetSelection(kvp.Value);

                GUI.backgroundColor = ogColor;
                GUI.enabled = true;
            }
            EditorGUILayout.EndHorizontal();

            GUIStyle previewStyle = new GUIStyle(GUI.skin.box);
            previewStyle.alignment = TextAnchor.MiddleLeft;
            EditorGUILayout.LabelField(_CurrentFile.Text, previewStyle);

            EditorGUILayout.EndVertical();
        }

        private void ShowSelectedFolder()
        {

        }

        private void ShowVariableSettings(string name, ref TemplateFile.VariableSettings settings)
        {
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField(settings.ToolTip);

            settings.IsUnique = EditorGUILayout.Toggle("Is Unique?", settings.IsUnique);

            GUIContent notSetError = EditorGUIUtility.IconContent("error");
            notSetError.tooltip = "Value has not been set!";
            GUIContent regexMismatchWarning = EditorGUIUtility.IconContent("warning");
            regexMismatchWarning.tooltip = "Regex mismatch!";

            // Default Value
            GUI.enabled = !settings.IsUnique;
            EditorGUILayout.BeginHorizontal();
            TemplateVariable variable = _Database.VariableNameToData[name];
            variable.DefaultValue = EditorGUILayout.TextField(new GUIContent("Default"), variable.DefaultValue);
            if (variable.DefaultValue.Equals(null) || variable.DefaultValue.Equals(""))
                EditorGUILayout.LabelField(notSetError, GUIStyle.none, GUILayout.MaxWidth(EditorGUIUtility.singleLineHeight));
            else if (settings.Regex != null && !settings.Regex.IsMatch(variable.DefaultValue))
                EditorGUILayout.LabelField(regexMismatchWarning, GUIStyle.none, GUILayout.MaxWidth(EditorGUIUtility.singleLineHeight));
            EditorGUILayout.EndHorizontal();

            GUI.enabled = true;

            // Unique Value
            if (settings.IsUnique)
            {
                EditorGUILayout.BeginHorizontal();
                settings.Value = EditorGUILayout.TextField(new GUIContent("Unique"), settings.Value);
                if (settings.Value.Equals(null) || settings.Value.Equals(""))
                    EditorGUILayout.LabelField(notSetError, GUIStyle.none, GUILayout.MaxWidth(EditorGUIUtility.singleLineHeight));
                else if (settings.Regex != null && !settings.Regex.IsMatch(settings.Value))
                    EditorGUILayout.LabelField(regexMismatchWarning, GUIStyle.none, GUILayout.MaxWidth(EditorGUIUtility.singleLineHeight));
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }
    }
}
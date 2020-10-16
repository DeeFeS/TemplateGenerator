// Created by Dennis 'DeeFeS' Schmidt 2020
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DFSSolutions.AssetVariables
{

    public class AssetDictionaryEditor : Editor
    {
        private SerializedProperty _Keys = default;
        private SerializedProperty _Values = default;

        private void OnEnable()
        {
            _Keys = serializedObject.FindProperty(nameof(_Keys));
            _Values = serializedObject.FindProperty(nameof(_Values));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField($"{_Keys.arraySize} Entries");

            ShowHeader();

            for (int i = 0; i < _Keys.arraySize; i++)
                ShowEntry(i);

            serializedObject.ApplyModifiedProperties();
        }

        private void ShowHeader()
        {
            EditorGUILayout.BeginHorizontal();

            //Rect r = GUILayoutUtility.GetRect(new GUIContent("+"), GUI.skin.button);
            EditorGUILayout.LabelField($"Keys", GUILayout.Width((Screen.width - 116f) / 2f));
            EditorGUILayout.LabelField($"Entries", GUILayout.Width((Screen.width - 116f) / 2f));
            if (GUILayout.Button("+"))
                AddEntrySlot(0);


            EditorGUILayout.EndHorizontal();
        }

        private void ShowEntry(int idx)
        {
            EditorGUILayout.BeginHorizontal();
            var key = _Keys.GetArrayElementAtIndex(idx);
            var value = _Values.GetArrayElementAtIndex(idx);

            if (key != null)
                EditorGUILayout.PropertyField(key, new GUIContent(""));
            if (value != null)
                EditorGUILayout.PropertyField(value, new GUIContent(""));

            if (idx == 0)
                GUI.enabled = false;
            if (GUILayout.Button("<"))
                MoveEntry(idx, true);
            GUI.enabled = true;

            if (idx == _Keys.arraySize - 1)
                GUI.enabled = false;
            if (GUILayout.Button(">"))
                MoveEntry(idx, false);
            GUI.enabled = true;

            if (GUILayout.Button("+"))
                AddEntrySlot(idx);
            if (GUILayout.Button("-"))
                RemoveEntrySlot(idx);
            EditorGUILayout.EndHorizontal();
        }

        private void AddEntrySlot(int idx)
        {
            _Keys.InsertArrayElementAtIndex(idx);
            _Values.InsertArrayElementAtIndex(idx);
        }

        private void RemoveEntrySlot(int idx)
        {
            _Keys.DeleteArrayElementAtIndex(idx);
            _Values.DeleteArrayElementAtIndex(idx);
        }

        private void MoveEntry(int idx, bool moveLeft)
        {
            if (moveLeft)
                --idx;

            _Keys.InsertArrayElementAtIndex(idx);
            _Keys.MoveArrayElement(idx + 2, idx);
            _Keys.DeleteArrayElementAtIndex(idx + 2);

            _Values.InsertArrayElementAtIndex(idx);
            _Values.MoveArrayElement(idx + 2, idx);
            _Values.DeleteArrayElementAtIndex(idx + 2);
        }
    }
}
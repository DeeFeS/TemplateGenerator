/// ---------------------------------------------------------------
/// Source: https://github.com/roboryantron/Unite2017/blob/master/Assets/Code/Variables/Editor/FloatReferenceDrawer.cs
///	Edited by Dennis 'DeeFeS' Schmidt
/// Copyright 2020
/// ---------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DeeFeS
{
    public abstract class VariableReferenceDrawer : PropertyDrawer
    {
        protected readonly string[] _PopupOptions = { "Use Asset", "Use Local" };

        protected GUIStyle _PopupStyle;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_PopupStyle == null)
            {
                _PopupStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions"));
                _PopupStyle.imagePosition = ImagePosition.ImageOnly;
            }

            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            EditorGUI.BeginChangeCheck();

            // Get properties
            SerializedProperty useLocal = property.FindPropertyRelative("_UseLocal");
            SerializedProperty local = property.FindPropertyRelative("_Local");
            SerializedProperty variable = property.FindPropertyRelative("_Asset");

            // Calculate rect for configuration button
            Rect buttonRect = new Rect(position);
            buttonRect.yMin += _PopupStyle.margin.top;
            buttonRect.width = _PopupStyle.fixedWidth + _PopupStyle.margin.right;
            position.xMin = buttonRect.xMax;

            // Store old indent level and set it to 0, the PrefixLabel takes care of it
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            int result = EditorGUI.Popup(buttonRect, useLocal.boolValue ? 1 : 0, _PopupOptions, _PopupStyle);

            useLocal.boolValue = result == 1;

            if (!useLocal.boolValue || local != null)
                EditorGUI.PropertyField(position, useLocal.boolValue ? local : variable, GUIContent.none);

            if (EditorGUI.EndChangeCheck())
                property.serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
}
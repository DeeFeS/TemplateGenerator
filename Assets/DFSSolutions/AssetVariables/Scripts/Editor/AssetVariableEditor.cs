// Created by Dennis 'DeeFeS' Schmidt 2020
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DFSSolutions.AssetVariables
{
    public class AssetVariableEditor<TVariable, TAsset> : Editor
        where TAsset : AssetVariable<TVariable>
    {
        private AssetVariable<TVariable> _Entity = default;
        private SerializedProperty _Value = default;
        private const float BUTTON_HEIGHT = 40f;

        private void OnEnable()
        {
            _Entity = (AssetVariable<TVariable>)target;
            _Value = serializedObject.FindProperty(nameof(_Value));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUI.enabled = Application.isPlaying;
            if (GUILayout.Button("Push Value", GUILayout.Height(BUTTON_HEIGHT)))
                _Entity.InvokeCallback();
            GUI.enabled = true;

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_Value);

            serializedObject.ApplyModifiedProperties();
        }
    }

}
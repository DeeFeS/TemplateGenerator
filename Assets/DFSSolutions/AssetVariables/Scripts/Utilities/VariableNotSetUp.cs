// Created by Dennis 'DeeFeS' Schmidt 2020
using UnityEngine;

namespace DFSSolutions.AssetVariables
{
    public static class VariableNotSetUp
    {
        public static void Check(Object variable, string variableName, Object context, LogType severity = LogType.Warning)
        {
            if (!variable)
                Debug.LogFormat(severity, LogOption.None, context, "Variable '{0}' was not setup correctly!", variableName);
        }
    }
}

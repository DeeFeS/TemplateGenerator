// Created by Dennis 'DeeFeS' Schmidt 2020
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DFSSolutions.TemplateGenerator
{
    [System.Serializable]
    public class TemplateFile
    {
        public static readonly string[] SETTING_NAMES = new string[] { "Disabled", "Don't Override", "Override", };
        public enum GenerationMode : int { DontGenerate = 0, GenerateIfNonexisting, OverwriteExisting };

        public static readonly Dictionary<string, GenerationMode> SETTING_NAME_TO_ENUM = new Dictionary<string, GenerationMode>()
        {
            { SETTING_NAMES[(int)GenerationMode.DontGenerate], GenerationMode.DontGenerate},
            { SETTING_NAMES[(int)GenerationMode.GenerateIfNonexisting], GenerationMode.GenerateIfNonexisting},
            { SETTING_NAMES[(int)GenerationMode.OverwriteExisting], GenerationMode.OverwriteExisting},
        };
        public static readonly string TEMPLATE_FILE_EXTENSION = ".txt";

        private static readonly string[] HEADER_SEPERATOR = new string[] { "<header>", "</header>" };
        private static readonly string[] TOP_LEVEL_SEPERATOR = new string[] { "<description>", "</description>", "<variables>", "</variables>", "<dependencies>", "</dependencies>"};
        private static readonly string[] DESCRIPTION_SEPERATOR = new string[] { "<description>", "</description>" };
        private static readonly string[] VARIABLES_SEPERATOR = new string[] { "<variables>", "</variables>" };
        private static readonly string[] DEPENDENCY_SEPERATOR = new string[] { "<dependencies>", "</dependencies>" };
        private static readonly string[] VARIABLE_SEPERATOR = new string[] { "<variable>", "</variable>" };
        private static readonly string[] FIELD_SEPERATOR = new string[] { "<br>" };

        public struct VariableSettings
        {
            public bool IsUnique;
            public string Value;
            public string ToolTip;
            public Regex Regex;
        }

        private TemplateFolder _Folder = default;
        private string _Name = "";
        private string _Description = "";
        private string _Text = "";
        private GenerationMode _Mode = GenerationMode.GenerateIfNonexisting;
        private Dictionary<string, VariableSettings> _VariableToSettings = new Dictionary<string, VariableSettings>();
        private Dictionary<string, TemplateFile> _DependenciesNameToFile = new Dictionary<string, TemplateFile>();

        public string Name => _Name;
        public string Description => _Description;
        public string Text => _Text;
        public GenerationMode Mode { get => _Mode; set => _Mode = value; }
        public Dictionary<string, VariableSettings> VariableToSettings => _VariableToSettings;
        public Dictionary<string, TemplateFile> DependenciesNameToFile => _DependenciesNameToFile;

        public TemplateFile(TemplateFolder folder) => _Folder = folder;

        public bool ReadFile(string path)
        {
            if (!File.Exists(path))
                return false;
            _Name = Path.GetFileNameWithoutExtension(path);
            string fullText = File.ReadAllText(path);
            string[] splitText = fullText.Split(HEADER_SEPERATOR, StringSplitOptions.RemoveEmptyEntries);
            if (splitText.Length != 2)
                return false;
            if (!ReadHeader(splitText[0]))
                return false;

            _Text = splitText[1];
            return true;
        }

        public bool InjectDependency(string name, TemplateFile file)
        {
            if (!_DependenciesNameToFile.ContainsKey(name))
                return false;
            _DependenciesNameToFile[name] = file;
            return true;
        }

        public void UpdateVariableSettings(string variable, VariableSettings settings)
        {
            if (!_VariableToSettings.ContainsKey(variable))
                return;

            _VariableToSettings[variable] = settings;
        }

        private bool ReadHeader(string header)
        {
            header = header.Replace("\r\n", "");
            string[] splitText = header.Split(TOP_LEVEL_SEPERATOR, StringSplitOptions.RemoveEmptyEntries);
            if (splitText.Length < 3)
                return false;
            _Description = splitText[0];

            string[] variables = splitText[1].Split(VARIABLE_SEPERATOR, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < variables.Length; i++)
            {
                string[] fields = variables[i].Split(FIELD_SEPERATOR, StringSplitOptions.RemoveEmptyEntries);
                if (fields.Length < 1)
                    return false;
                VariableSettings settings = new VariableSettings();
                settings.IsUnique = false;
                settings.Value = "";
                settings.ToolTip = fields.Length >= 2 ? fields[1] : "";
                if (fields.Length >= 3)
                    settings.Regex = new Regex(fields[2]);

                _VariableToSettings.Add(fields[0], settings);
            }

            string[] dependencies = splitText[2].Split(FIELD_SEPERATOR, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < dependencies.Length; i++)
                _DependenciesNameToFile.Add(dependencies[i], null);

            return true;
        }
    }
}
// Created by Dennis 'DeeFeS' Schmidt 2020
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DFSSolutions.TemplateGenerator
{
    [System.Serializable]
    public class TemplateDatabase
    {
        private Dictionary<string, TemplateVariable> _VariableNameToData = new Dictionary<string, TemplateVariable>();
        private List<TemplateFile> _Files = new List<TemplateFile>();
        private List<TemplateFolder> _Folders = new List<TemplateFolder>();

        public Dictionary<string, TemplateVariable> VariableNameToData => _VariableNameToData;
        public List<TemplateFile> Files => _Files;
        public List<TemplateFolder> Folders => _Folders;

        public bool FillDatabase(string path)
        {
            CheckFolder(path);
            if (_Folders.Count <= 0)
                return false;
            RegisterFiles();
            RegisterDependencies();
            GatherVariables();
            return true;
        }

        private void CheckFolder(string path)
        {
            string[] directories = Directory.GetDirectories(path);
            if (directories.Length <= 0)
                return;

            for (int i = 0; i < directories.Length; i++)
            {
                string directory = directories[i].Remove(0, path.Length + 1);
                if (directory.Equals("Templates"))
                {
                    TemplateFolder folder = new TemplateFolder(null);
                    if (folder.ReadFolder(directories[i]))
                        _Folders.Add(folder);
                }
                else
                    CheckFolder(directories[i]);
            }
        }

        private void RegisterFiles()
        {
            foreach (TemplateFolder folder in _Folders)
                RegisterFilesInFolder(folder);
        }

        private void RegisterFilesInFolder(TemplateFolder folder)
        {
            foreach (TemplateFolder subfolder in folder.Subfolders)
                RegisterFilesInFolder(subfolder);

            List<TemplateFile> files = folder.Files;
            _Files.AddRange(files);
        }

        private void RegisterDependencies()
        {
            for (int i = 0; i < _Files.Count; i++)
            {
                foreach (TemplateFile file in _Files)
                    _Files[i].InjectDependency(file.Name, file);
            }
        }

        private void GatherVariables()
        {
            foreach (TemplateFile file in _Files)
            {
                foreach (KeyValuePair<string, TemplateFile.VariableSettings> kvp in file.VariableToSettings)
                {
                    TemplateVariable var = null;
                    if (!_VariableNameToData.TryGetValue(kvp.Key, out var))
                    {
                        var = new TemplateVariable(kvp.Key);
                        _VariableNameToData.Add(kvp.Key, var);
                    }
                    var.AddUser(file);
                }
            }
        }
    }
}

// Created by Dennis 'DeeFeS' Schmidt 2020
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DFSSolutions.TemplateGenerator
{
    [System.Serializable]
    public class TemplateFolder
    {
        private bool _InUse = true;
        private string _Name = default;
        private TemplateFolder _Parent = default;
        private List<TemplateFolder> _SubFolders = new List<TemplateFolder>();
        private List<TemplateFile> _Files = new List<TemplateFile>();

        public string Name => _Name;
        public TemplateFolder Parent => _Parent;
        public List<TemplateFolder> Subfolders => _SubFolders;
        public List<TemplateFile> Files => _Files;
        public bool InUse { get => _InUse; set => _InUse = value; }

        public TemplateFolder(TemplateFolder parent) => _Parent = parent;

        public bool ReadFolder(string path)
        {
            if (!Directory.Exists(path))
                return false;

            _Name = path.Remove(0, Path.GetDirectoryName(path).Length + 1);
            ReadDirectories(path);
            ReadFiles(path);

            return true;
        }

        private void ReadDirectories(string path)
        {
            Debug.Log($"Directory: {path}");
            string[] directoryPaths = Directory.GetDirectories(path);
            for (int i = 0; i < directoryPaths.Length; i++)
            {
                TemplateFolder folder = new TemplateFolder(this);
                if (folder.ReadFolder(directoryPaths[i]))
                    _SubFolders.Add(folder);
            }
        }

        private void ReadFiles(string path)
        {
            string[] filePaths = Directory.GetFiles(path);
            for (int i = 0; i < filePaths.Length; i++)
            {
                string extension = Path.GetExtension(filePaths[i]).ToLower();
                Debug.Log($"Files: {filePaths[i]}");
                if (extension != TemplateFile.TEMPLATE_FILE_EXTENSION.ToLower())
                    continue;
                TemplateFile file = new TemplateFile(this);
                if (file.ReadFile(filePaths[i]))
                    _Files.Add(file);
            }
        }
    }
}
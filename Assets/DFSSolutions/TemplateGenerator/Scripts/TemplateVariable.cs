// Created by Dennis 'DeeFeS' Schmidt 2020
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DFSSolutions.TemplateGenerator
{
    [System.Serializable]
    public class TemplateVariable
	{
        private string _Name = "";
        private string _DefaultValue = "";
        private List<TemplateFile> _Users = new List<TemplateFile>();

        public string Name => _Name;

        public string DefaultValue
        {
            get => _DefaultValue;
            set => _DefaultValue = value;
        }

        public List<TemplateFile> Users => _Users;

        public TemplateVariable(string name) => _Name = name;

        public void AddUser(TemplateFile file)
        {
            if (!_Users.Contains(file))
                _Users.Add(file);
        }
	}
}
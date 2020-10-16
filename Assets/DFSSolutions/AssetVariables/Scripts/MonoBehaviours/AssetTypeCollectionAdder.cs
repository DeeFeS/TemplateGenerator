// Created by Dennis 'DeeFeS' Schmidt 2020
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DFSSolutions.AssetVariables
{
    public class AssetTypeCollectionAdder<TVariable, TAsset> : MonoBehaviour
        where TAsset : AssetTypeCollection<TVariable>
    {
        public enum AddMode : int
        {
            Manual = 0,
            Awake, Start, OnEnable, OnChange,
        }

        public enum RemoveMode : int
        {
            Manual = 0,
            OnDisable, OnDestroy, OnChange
        }

        [SerializeField] private TVariable _Value = default;
        [SerializeField] private TAsset _Asset = default;
        [Space]
        [SerializeField] private AddMode _AddMode = AddMode.Awake;
        [SerializeField] private RemoveMode _RemoveMode = RemoveMode.OnDestroy;
        [Space]
        [SerializeField] private int _Index = -1;

        public TVariable Value
        {
            get { return _Value; }
            set
            {
                if (_Value.Equals(value))
                    return;

                Remove(RemoveMode.OnChange);

                _Value = value;

                Add(AddMode.OnChange);
            }
        }

        private void Awake()
        {
            VariableNotSetUp.Check(_Asset, nameof(_Asset), this);

            Add(AddMode.Awake);
        }

        private void Start() => Add(AddMode.Start);
        private void OnEnable() => Add(AddMode.OnEnable);
        private void OnDisable() => Remove(RemoveMode.OnDisable);
        private void OnDestroy() => Remove(RemoveMode.OnDestroy);

        public void Add() => Add(AddMode.Manual);
        public void Remove() => Remove(RemoveMode.Manual);

        private void Add(AddMode current)
        {
            if (current != _AddMode)
                return;

            if (_Index < 0)
                _Asset.Add(_Value);
            else
                _Asset.AddAt(_Value, _Index);
        }

        private void Remove(RemoveMode current)
        {
            if (current != _RemoveMode)
                return;

            if (_Index < 0)
                _Asset.Remove(_Value);
            else
                _Asset.RemoveAt(_Value, _Index);
        }
    }
}

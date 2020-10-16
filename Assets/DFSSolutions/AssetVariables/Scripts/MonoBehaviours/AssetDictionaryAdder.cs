﻿// Created by Dennis 'DeeFeS' Schmidt 2020
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DFSSolutions.AssetVariables
{
    public class AssetDictionaryAdder<TKey, TValue, TAsset> : MonoBehaviour
        where TAsset : AssetDictionary<TKey, TValue>
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

        [SerializeField] private TKey _Key = default;
        [SerializeField] private TValue _Value = default;
        [SerializeField] private TAsset _Asset = default;
        [Space]
        [SerializeField] private AddMode _AddMode = AddMode.Awake;
        [SerializeField] private RemoveMode _RemoveMode = RemoveMode.OnDestroy;
        [SerializeField, ReadOnly] private bool _IsAdded = false;

        public TKey Key
        {
            get => _Key;
            set
            {
                if (_Key.Equals(value))
                    return;

                Remove(RemoveMode.OnChange);

                _Key = value;

                Add(AddMode.OnChange);
            }
        }

        public TValue Value
        {
            get => _Value;
            set
            {
                if (_Value.Equals(value))
                    return;

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

            if (!_IsAdded)
                _Asset.Add(_Key, _Value);
            else
                _Asset[_Key] = _Value;

            _IsAdded = true;
        }

        private void Remove(RemoveMode current)
        {
            if (current != _RemoveMode)
                return;

            if (_IsAdded)
                _Asset.Remove(_Key);
            _IsAdded = false;
        }
    }
}

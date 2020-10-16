// Created by Dennis 'DeeFeS' Schmidt 2020
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DFSSolutions.AssetVariables
{
    public class AssetVariableSetter<TVariable, TAsset> : MonoBehaviour
        where TAsset : AssetVariable<TVariable>
    {
        public enum SetMode : int
        {
            Manual = 0,
            Awake, Start, OnEnable, OnChange,
        }

        public enum UnsetMode : int
        {
            Manual = 0,
            OnDisable, OnDestroy, OnChange
        }

        [SerializeField] private TVariable _Value = default;
        [SerializeField] private TAsset _Asset = default;
        [Space]
        [SerializeField] private SetMode _SetMode = default;
        [SerializeField] private bool _OnlySetIfUnsetValue = false;
        [Space]
        [SerializeField] private UnsetMode _UnsetMode = default;
        [SerializeField] private TVariable _UnsetValue = default;
        [SerializeField] private bool _OnlyUnsetIfSetValue = true;

        public TVariable Value
        {
            get { return _Value; }
            set
            {
                if (_Value.Equals(value))
                    return;

                Unset(UnsetMode.OnChange);

                _Value = value;

                Set(SetMode.OnChange);
            }
        }

        private void Awake()
        {
            VariableNotSetUp.Check(_Asset, nameof(_Asset), this);

            Set(SetMode.Awake);
        }

        private void Start() => Set(SetMode.Start);
        private void OnEnable() => Set(SetMode.OnEnable);
        private void OnDisable() => Unset(UnsetMode.OnDisable);
        private void OnDestroy() => Unset(UnsetMode.OnDestroy);

        public void Set() => Set(SetMode.Manual);
        public void Unset() => Unset(UnsetMode.Manual);

        private void Set(SetMode current)
        {
            if (current != _SetMode)
                return;

            if (_OnlySetIfUnsetValue && !_Asset.Value.Equals(_UnsetValue))
                return;

            _Asset.Value = _Value;
        }

        private void Unset(UnsetMode current)
        {
            if (current != _UnsetMode)
                return;

            if (_OnlyUnsetIfSetValue && !_Asset.Value.Equals(_Value))
                return;

            _Asset.Value = _UnsetValue;
        }
    }
}
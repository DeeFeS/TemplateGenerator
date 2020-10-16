// Created by Dennis 'DeeFeS' Schmidt 2020
using UnityEngine;

namespace DFSSolutions.AssetVariables
{
    public class AssetVariable<T> : ScriptableObject
    {
        public delegate void ValueChangeCallback(T newValue);

        [SerializeField] private T _Value = default;
        private event ValueChangeCallback _Callback = default;

        public T Value
        {
            get { return _Value; }
            set
            {
                if (_Value.Equals(value))
                    return;

                _Value = value;
                _Callback?.Invoke(value);
            }
        }

        private void OnValidate() => InvokeCallback();

        /// <summary>
        /// Manual callback trigger. Only use if necessary.
        /// </summary>
        public void InvokeCallback() => _Callback?.Invoke(_Value);
        public void SubscribeToValueChange(ValueChangeCallback callback) => _Callback += callback;
        public void UnsubscribeFromValueChange(ValueChangeCallback callback) => _Callback -= callback;
    }
}
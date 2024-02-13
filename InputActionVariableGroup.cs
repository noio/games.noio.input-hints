using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.SmartFormat.Core.Extensions;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.Serialization;

namespace  games.noio.InputHints
{
    [Serializable]
    public class InputActionVariableGroup : IVariableValueChanged, IVariableGroup
    {
        public event Action<IVariable> ValueChanged;

        #region PUBLIC AND SERIALIZED FIELDS

        [SerializeField]
        InputHintsConfig _config;

        #endregion

        Dictionary<string, InputActionVariable> _cachedVariables;

        #region INTERFACE IMPLEMENTATIONS

        public bool TryGetValue(string key, out IVariable value)
        {
            if (_cachedVariables == null)
            {
                InputHints.UsedDeviceChanged += OnValueChanged;
                _config.Changed += OnValueChanged;
                
                _cachedVariables = new Dictionary<string, InputActionVariable>();
            }

            if (_cachedVariables.TryGetValue(key, out var cachedVariable))
            {
                value = cachedVariable;
                return true;
            }

            var action = _config.InputActions.FindAction(key);
            if (action != null)
            {
                value = _cachedVariables[key] = new InputActionVariable(action, _config);
                return true;
            }

            /*
             * Try to find action by replacing underscores or dashes with spaces:
             * (In case the action name has spaces in it..)
             * We first try the exact match above in case the action
             * ACTUALLY has a dash or underscore in the name.
             */
            key = key.Replace("-", " ");
            key = key.Replace("_", " ");
            action = _config.InputActions.FindAction(key);
            if (action != null)
            {
                value = _cachedVariables[key] = new InputActionVariable(action, _config);
                return true;
            }

            value = default;
            return false;
        }

        public object GetSourceValue(ISelectorInfo selector)
        {
            return this;
        }

        #endregion

        void OnValueChanged()
        {
            ValueChanged?.Invoke(this);
        }
    }
}
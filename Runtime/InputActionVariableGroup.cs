// (C)2024 @noio_games
// Thomas van den Berg

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.Core.Extensions;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

namespace games.noio.InputHints
{
    [Serializable]
    public class InputActionVariableGroup : IVariableValueChanged, IVariableGroup
    {
        public event Action<IVariable> ValueChanged;

        #region SERIALIZED FIELDS

        [SerializeField] InputHintsConfig _config;

        #endregion

        Dictionary<string, InputActionVariable> _cachedVariables;

        #region PROPERTIES

        public InputHintsConfig Config
        {
            get => _config;
            set => _config = value;
        }

        #endregion

        #region INTERFACE IMPLEMENTATIONS

        public bool TryGetValue(string key, out IVariable value)
        {
            // Debug.Log($"F{Time.frameCount} InputActionVariableGroup.TryGetValue({key})");
                
            if (_cachedVariables == null)
            {
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
            // Debug.Log($"F{Time.frameCount} InputActionVariableGroup.GetSourceValue({selector.SelectorOperator}{selector.SelectorText})");
            // Debug.Log($"F{Time.frameCount} GetSourceVal");
            // Debug.Log(
                // $"F{Time.frameCount} GetSourceVal {selector.SelectorText} {selector.CurrentValue} {selector.SelectorIndex} {selector.Result} {selector.SelectorOperator}");
            return this;
        }

        #endregion

        void OnValueChanged()
        {
            ValueChanged?.Invoke(this);
        }
    }
}
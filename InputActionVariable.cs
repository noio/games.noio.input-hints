using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.SmartFormat.Core.Extensions;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

// ReSharper disable once CheckNamespace
namespace  games.noio.InputHints
{
    internal struct InputActionVariable : IVariableValueChanged
    {
        public event Action<IVariable> ValueChanged;
        readonly InputAction _action;
        readonly InputHintsConfig _source;

        public InputActionVariable(InputAction action, InputHintsConfig source)
        {
            _action = action;
            _source = source;
            ValueChanged = null;
        }

        public override string ToString()
        {
            return $"InputActionVariable({_action.name}, {_action.GetBindingDisplayString()})";
        }

        public object GetSourceValue(ISelectorInfo selector)
        {
            return _source.GetSprite(_action);
        }

        public void OnValueChanged()
        {
            Debug.Log($"F{Time.frameCount} Sending OnValueChanged to {this} Has Listeners: {ValueChanged != null}");
            ValueChanged?.Invoke(this);
        }
    }
}
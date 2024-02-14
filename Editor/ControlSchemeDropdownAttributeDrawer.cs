// (C)2024 @noio_games
// Thomas van den Berg

using Packages.games.noio.input_hints;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace games.noio.InputHints.Editor
{
    [CustomPropertyDrawer(typeof(ControlSchemeDropdownAttribute))]
    public class ControlSchemeDropdownAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, label);
            if (EditorGUI.DropdownButton(position, new GUIContent(property.stringValue), FocusType.Keyboard))
            {
                var config = property.serializedObject.targetObject as InputHintsConfig;
                var menu = new GenericMenu();
                foreach (var controlScheme in config.GetInputControlSchemes())
                {
                    menu.AddItem(new GUIContent(controlScheme.name), false, () =>
                    {
                        property.stringValue = controlScheme.name;
                        property.serializedObject.ApplyModifiedProperties();
                    });
                }

                menu.DropDown(position);
            }
        }

        
    }
}
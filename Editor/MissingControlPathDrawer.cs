using UnityEditor;
using UnityEngine;

namespace games.noio.InputHints.Editor
{
    [CustomPropertyDrawer(typeof(MissingControlPath))]
    public class MissingControlPathDrawer : PropertyDrawer
    {
        #region MONOBEHAVIOUR METHODS

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var controlPathProp = property.FindPropertyRelative("_controlPath");
            var controlSchemeProp = property.FindPropertyRelative("_controlScheme");

            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var iconRect = position;
            iconRect.width = 18;

            var clearButtonRect = position;
            clearButtonRect.xMin = clearButtonRect.xMax - 60;

            var addButtonRect = position;
            addButtonRect.xMax = clearButtonRect.xMin - 5;
            addButtonRect.xMin = addButtonRect.xMax - 60;

            var textRect = position;
            textRect.xMin = iconRect.xMax;
            textRect.xMax = addButtonRect.xMin - 5;

            EditorGUI.LabelField(iconRect, EditorGUIUtility.IconContent("d_console.warnicon.sml"));

            EditorGUI.LabelField(textRect,
                $"No mapping found for \"{controlPathProp.stringValue}\" " +
                $"in Control Scheme \"{controlSchemeProp.stringValue}\"");

            if (GUI.Button(addButtonRect, "Add"))
            {
                var config = controlPathProp.serializedObject.targetObject as InputHintsConfig;

                EditorApplication.delayCall += () =>
                {
                    var spriteName = SpriteSearchWindow.ShowWindow();

                    config.AddSprite(controlPathProp.stringValue, spriteName, controlSchemeProp.stringValue);
                    config.ClearMissingControlPath(controlPathProp.stringValue,
                        controlSchemeProp.stringValue);
                    controlPathProp.serializedObject.Update();
                };
            }

            if (GUI.Button(clearButtonRect, "Clear"))
            {
                var config = controlPathProp.serializedObject.targetObject as InputHintsConfig;
                config.ClearMissingControlPath(controlPathProp.stringValue, controlSchemeProp.stringValue);
                controlPathProp.serializedObject.Update();
            }

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }

        #endregion
    }
}
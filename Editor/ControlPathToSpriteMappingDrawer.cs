using UnityEditor;
using UnityEngine;

namespace games.noio.InputHints.Editor
{
    [CustomPropertyDrawer(typeof(ControlPathToSpriteMapping))]
    public class ControlPathToSpriteMappingDrawer : PropertyDrawer
    {
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var controlPathProp = property.FindPropertyRelative("_controlPath");
            var spriteNameProp = property.FindPropertyRelative("_spriteName");
            var spriteCategoryProp = property.FindPropertyRelative("_spriteCategory");

            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects
            var controlPathRect = position;
            controlPathRect.width = 80;

            /*
             * Sprite sheet rect on the right
             */
            var spriteCategoryRect = position;
            spriteCategoryRect.xMin = spriteCategoryRect.xMax - 90;

            var searchButtonRect = position;
            searchButtonRect.xMax = spriteCategoryRect.xMin - 5;
            searchButtonRect.xMin = searchButtonRect.xMax - 40;

            /*
             * Sprite name fills up the remaining space in the middle:
             */
            var spriteNameRect = position;
            spriteNameRect.xMin = controlPathRect.xMax + 5;
            spriteNameRect.xMax = searchButtonRect.xMin - 5;

            // Draw fields - passs GUIContent.none to each so they are drawn without labels
            EditorGUI.PropertyField(controlPathRect, controlPathProp,
                GUIContent.none);
            EditorGUI.PropertyField(spriteNameRect, spriteNameProp,
                GUIContent.none);

            if (GUI.Button(searchButtonRect, EditorGUIUtility.IconContent("d_SearchWindow")))
            {
                EditorApplication.delayCall += () =>
                {
                    var pickedSprite = SpriteSearchWindow.ShowWindow();
                    if (pickedSprite != null)
                    {
                        spriteNameProp.stringValue = pickedSprite;
                        spriteNameProp.serializedObject.ApplyModifiedProperties();
                    }
                };
            }

            if (EditorGUI.DropdownButton(spriteCategoryRect,
                    new GUIContent(spriteCategoryProp.stringValue),
                    FocusType.Keyboard))
            {
                var menu = new GenericMenu();
                var config = property.serializedObject.targetObject as InputHintsConfig;
                foreach (var sheetKey in config.GetSpriteCategories())
                {
                    menu.AddItem(new GUIContent(sheetKey), false, () =>
                    {
                        spriteCategoryProp.stringValue = sheetKey;
                        spriteCategoryProp.serializedObject.ApplyModifiedProperties();
                    });
                }

                menu.DropDown(spriteCategoryRect);
            }

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}
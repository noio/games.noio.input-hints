using UnityEditor;
using UnityEngine;

namespace games.noio.InputHints.Editor
{
    [CustomPropertyDrawer(typeof(SpriteCategoryToAssetMapping))]
    public class SpriteCategoryToAssetMappingDrawer : PropertyDrawer
    {
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var spriteCategoryProp = property.FindPropertyRelative("_spriteCategory");
            var spriteAssetProp = property.FindPropertyRelative("_spriteAsset");

            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            position.yMin += 1;
            
            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects
            var leftRect = position;
            leftRect.width = 80;

            // var midRect = position;
            // midRect.xMin = leftRect.xMax + 5;
            // midRect.width = 16;

            var rightRect = position;
            rightRect.xMin = leftRect.xMax + 5;
            rightRect.height -= 2;

            var config = property.serializedObject.targetObject as InputHintsConfig;

            /*
             * Draw Dropdown with all SpriteCategories
             */
            if (EditorGUI.DropdownButton(leftRect,
                    new GUIContent(spriteCategoryProp.stringValue),
                    FocusType.Keyboard))
            {
                var menu = new GenericMenu();

                foreach (var sheetKey in config.GetSpriteCategories())
                {
                    menu.AddItem(new GUIContent(sheetKey), false, () =>
                    {
                        spriteCategoryProp.stringValue = sheetKey;
                        spriteCategoryProp.serializedObject.ApplyModifiedProperties();
                        config.OnChanged();
                    });
                }

                menu.DropDown(leftRect);
            }

            // EditorGUI.LabelField(midRect, "\u27a1");

            /*
             * Draw Dropdown with all SpriteSheets in project:
             */

            using (var changeScope = new EditorGUI.ChangeCheckScope())
            {
                EditorGUI.PropertyField(rightRect, spriteAssetProp, GUIContent.none);
                if (changeScope.changed)
                {
                    config.OnChanged();
                }
            }

            // if (EditorGUI.DropdownButton(rightRect,
            //         new GUIContent(spriteAssetNameProp.stringValue),
            //         FocusType.Keyboard))
            // {
            //     var menu = new GenericMenu();
            //
            //     foreach (var asset in InputHintsConfig.GetSpriteAssetsInProject())
            //     {
            //         menu.AddItem(new GUIContent(asset.name), false, () =>
            //         {
            //             spriteAssetNameProp.stringValue = asset.name;
            //             spriteAssetNameProp.serializedObject.ApplyModifiedProperties();
            //             InputHints.OnBindingsChanged();
            //         });
            //     }
            //
            //     menu.DropDown(rightRect);
            // }

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}
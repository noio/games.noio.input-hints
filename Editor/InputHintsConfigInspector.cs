// (C)2024 @noio_games
// Thomas van den Berg

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat.Extensions;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

namespace games.noio.InputHints.Editor
{
    [CustomEditor(typeof(InputHintsConfig))]
    public class InputHintsConfigInspector : UnityEditor.Editor
    {
        static readonly Lazy<GUIStyle> ExplanationTextStyle = new(() => new GUIStyle(EditorStyles.label)
        {
            wordWrap = true,
            fontSize = 11
        });

        SerializedProperty _scriptProp;
        SerializedProperty _inputActionsProp;
        SerializedProperty _spriteFormatProp;
        SerializedProperty _spriteCategoriesProp;
        SerializedProperty _controlTypesProp;
        SerializedProperty _spritesProp;
        SerializedProperty _missingControlPathsProp;
        ReorderableList _spriteCategoriesList;
        ReorderableList _controlTypesList;
        bool _categoriesOpen = true;
        bool _controlTypesOpen = true;
        bool _spritesOpen = true;
        bool _missingControlPathsOpen = true;
        bool _localizationVariablesAdded;
        string _localizationFormatString;

        #region MONOBEHAVIOUR METHODS

        void OnEnable()
        {
            _scriptProp = serializedObject.FindProperty("m_Script");
            _inputActionsProp = serializedObject.FindProperty("_inputActions");
            _spriteFormatProp = serializedObject.FindProperty("_spriteFormat");
            _spriteCategoriesProp = serializedObject.FindProperty("_spriteCategories");
            _controlTypesProp = serializedObject.FindProperty("_controlTypes");
            _spritesProp = serializedObject.FindProperty("_sprites");
            _missingControlPathsProp = serializedObject.FindProperty("_missingControlPaths");

            CheckLocalizationVariablesAdded();

            // Setup the ReorderableList for _spriteCategoriesProp
            _spriteCategoriesList =
                new ReorderableList(serializedObject, _spriteCategoriesProp, true, true, true, true)
                {
                    drawHeaderCallback = rect => { EditorGUI.LabelField(rect, "Categories"); },
                    drawElementCallback = (rect, index, isActive, isFocused) =>
                    {
                        var element = _spriteCategoriesProp.GetArrayElementAtIndex(index);

                        // rect.y += 2;
                        var name = element.FindPropertyRelative("_name");
                        GUI.Label(rect, name.stringValue);

                        // EditorGUI.PropertyField(
                        // new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        // element, GUIContent.none);
                    },
                    onAddCallback = list =>
                    {
                        EditorApplication.delayCall += () =>
                        {
                            var newCategory = TextInputDialog.Show("Add Sprite Category",
                                "Enter name for new category:", "Gamepad");
                            if ((target as InputHintsConfig).GetSpriteCategories().Contains(newCategory) ==
                                false)
                            {
                                // This is where you can customize the add operation
                                var index = list.serializedProperty.arraySize;
                                list.serializedProperty.arraySize++;
                                list.index = index; // Automatically select the new item

                                var newElement = list.serializedProperty.GetArrayElementAtIndex(index);
                                newElement.FindPropertyRelative("_name").stringValue = newCategory;
                                serializedObject.ApplyModifiedProperties();
                            }
                        };
                    }
                };

            _controlTypesList =
                new ReorderableList(serializedObject, _controlTypesProp, true, false, true, true)
                {
                    drawElementCallback = (rect, index, isActive, isFocused) =>
                    {
                        rect.xMin += 20;
                        var element = _controlTypesProp.GetArrayElementAtIndex(index);
                        EditorGUI.PropertyField(rect, element, includeChildren: true);
                    },
                    elementHeightCallback = (index) =>
                    {
                        var element = _controlTypesProp.GetArrayElementAtIndex(index);
                        return EditorGUI.GetPropertyHeight(element, true);
                    }
                };
        }

        void CheckLocalizationVariablesAdded()
        {
            foreach (var source in LocalizationSettings.StringDatabase.SmartFormatter.SourceExtensions)
            {
                if (source is PersistentVariablesSource persistentVariablesSource)
                {
                    foreach (KeyValuePair<string, VariablesGroupAsset> group in persistentVariablesSource)
                    {
                        if (group.Value != null)
                        {
                            foreach (KeyValuePair<string, IVariable> variable in group.Value)
                            {
                                if (variable.Value is InputActionVariableGroup)
                                {
                                    _localizationVariablesAdded = true;
                                    _localizationFormatString = $"{group.Key}.{variable.Key}";
                                }
                            }
                        }
                     
                    }
                }
            }
        }

        #endregion

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var config = target as InputHintsConfig;

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(_scriptProp);
            }

            EditorGUILayout.Space();

            // Draw the ReorderableList instead of the default property field for _spriteCategoriesProp
            EditorGUILayout.PropertyField(_inputActionsProp);
            EditorGUILayout.PropertyField(_spriteFormatProp);

            EditorGUILayout.Space();
            if (_localizationVariablesAdded)
            {
                EditorGUILayout.HelpBox($"You can insert Input Hints into Localized Strings using {{{_localizationFormatString}.ActionName}}", MessageType.Info);
            }
            else
            {
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.HelpBox($"The Localization System is not set up to format Input Hints.", MessageType.Error);
                    if (GUILayout.Button("Configure Now", GUILayout.Height(38)))
                    {
                        AddLocalizationVariables();
                        
                    }
                }
            }

            if (DrawHeader(ref _categoriesOpen, new GUIContent("Categories"),
                    "Categories are mapped to specific TMP_SpriteAssets by the Control Type."))
            {
                _spriteCategoriesList.DoLayoutList();
            }

            // EditorGUILayout.HelpBox(
            //     "Sprite Sheets are divided into categories. " +
            //     "For example, \"Gamepad\" can be \"Xbox\" or \"DualSense\", " +
            //     "but both have a sprite named \"Stick-L\"",
            //     MessageType.None);

            if (DrawHeader(ref _controlTypesOpen, new GUIContent("Control Types"),
                    "Control Types take a connected controller and map specific Sprite Assets " +
                    "to each relevant category."))
            {
                EditorGUI.indentLevel++;

                // EditorGUILayout.PropertyField(_controlTypesProp, GUIContent.none, true);
                _controlTypesList.DoLayoutList();

                EditorGUI.indentLevel--;
            }

            if (DrawHeader(ref _spritesOpen, new GUIContent("Sprites"),
                    "These are the mappings from the Input System's 'Control Path' to " +
                    "character names defined on the Sprite Asset."))
            {
                using (var changeScope = new EditorGUI.ChangeCheckScope())
                {
                    // _spritesList.DoLayoutList();
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_spritesProp, GUIContent.none, true);
                    EditorGUI.indentLevel--;

                    if (changeScope.changed)
                    {
                        serializedObject.ApplyModifiedProperties();

                        EditorApplication.delayCall += () => { config.OnChanged(); };
                    }
                }
            }

            if (DrawHeader(ref _missingControlPathsOpen, new GUIContent("Missing Control Paths"),
                    "In the Editor, when a sprite is requested for a control path that has no mapping, " +
                    "it is logged here, and you can add it directly."))
            {
                for (var i = 0; i < _missingControlPathsProp.arraySize; i++)
                {
                    var item = _missingControlPathsProp.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(item, GUIContent.none);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        void AddLocalizationVariables()
        {
            // var globalLocalizationVariables = AssetDatabase.FindAssets()
            
            
            CheckLocalizationVariablesAdded();
        }

        static bool DrawHeader(ref bool isOpen, GUIContent title, string description)
        {
            EditorGUILayout.Space();

            // isOpen = EditorGUILayout.Foldout(isOpen, title, EditorStyles.foldoutHeader);

            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

            if (isOpen)
            {
                EditorGUILayout.Space(2);
                GUI.color = new Color(1f, 1f, 1f, 0.8f);
                EditorGUILayout.LabelField(description,
                    ExplanationTextStyle.Value);
                GUI.color = Color.white;
            }

            EditorGUILayout.Space(9);

            return isOpen;
        }
    }
}
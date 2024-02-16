// (C)2024 @noio_games
// Thomas van den Berg

using System;
using System.Collections.Generic;
using System.IO;
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
        const string DefaultVariablesGroupAssetName = "global";
        const string DefaultVariableGroup = "input";

        static readonly Lazy<GUIStyle> ExplanationTextStyle = new(() => new GUIStyle(EditorStyles.label)
        {
            wordWrap = true,
            fontSize = 11
        });

        InputHintsConfig _config;
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
            _config = target as InputHintsConfig;
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
                            if (_config.GetSpriteCategories().Contains(newCategory) ==
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
                        var element = _controlTypesProp.GetArrayElementAtIndex(index);
                        var devicesString = element.FindPropertyRelative("_devices").stringValue;

                        /*
                         * Draw PREVIEW button
                         */
                        var buttonRect = rect;
                        buttonRect.yMin += 1;

                        // buttonRect.height = EditorGUIUtility.singleLineHeight;
                        buttonRect.width = 60;
                        buttonRect.yMax -= 1;
                        if (GUI.Button(buttonRect, "Preview"))
                        {
                            _config.SetControlTypeFromDevicesString(devicesString);
                        }

                        /*
                         * DRAW REST OF FIELDS
                         */
                        var fieldsRect = rect;
                        fieldsRect.xMin += 75;

                        // fieldsRect.yMin += 2 + EditorGUIUtility.singleLineHeight;
                        var label = string.IsNullOrEmpty(devicesString) ? "Default" : devicesString;
                        EditorGUI.PropertyField(fieldsRect, element, new GUIContent(label), true);
                    },
                    elementHeightCallback = index =>
                    {
                        var element = _controlTypesProp.GetArrayElementAtIndex(index);
                        return EditorGUI.GetPropertyHeight(element, true);

                        // +
                        // EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    }
                };
        }

        #endregion

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(_scriptProp);
            }

            EditorGUILayout.Space();

            // Draw the ReorderableList instead of the default property field for _spriteCategoriesProp

            bool inputActionsLinked = _inputActionsProp.objectReferenceValue != null;
            var localizationSettings = LocalizationSettings.Instance;

            if (inputActionsLinked == false)
            {
                EditorGUILayout.HelpBox("Please create and link an Input Action Asset", MessageType.Error);
            }
            else
            {
                if (_localizationVariablesAdded)
                {
                    EditorGUILayout.HelpBox(
                        $"You can insert Input Hints into Localized Strings using {{{_localizationFormatString}.ActionName}}",
                        MessageType.Info);
                }
                else
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUILayout.HelpBox(
                            "The Localization System is not set up to format Input Hints.",
                            MessageType.Error);
                        if (GUILayout.Button("Configure Now", GUILayout.Height(38)))
                        {
                            AutoConfigureSetup();
                        }
                    }
                }
            }

            EditorGUILayout.PropertyField(_inputActionsProp);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField(new GUIContent("Localization Settings"), localizationSettings,
                    typeof(LocalizationSettings));
            }

            EditorGUILayout.PropertyField(_spriteFormatProp);

            EditorGUILayout.Space();
            using var scope =
                new EditorGUI.DisabledScope(inputActionsLinked == false ||
                                            _localizationVariablesAdded == false);

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
                    "to each relevant category. Leave the last \"Devices\" string empty to use it as the " +
                    "default."))
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

                        EditorApplication.delayCall += () => { _config.OnChanged(); };
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

        void CheckLocalizationVariablesAdded()
        {
            foreach (var source in LocalizationSettings.StringDatabase.SmartFormatter.SourceExtensions)
            {
                if (source is PersistentVariablesSource persistentVariablesSource)
                {
                    /*
                     * Go through all the "Variables Group Assets"
                     */
                    foreach (KeyValuePair<string, VariablesGroupAsset> group in persistentVariablesSource)
                    {
                        if (group.Value != null)
                        {
                            /*
                             * Go through each Variables Group
                             */
                            foreach (KeyValuePair<string, IVariable> variable in group.Value)
                            {
                                /*
                                 * See if an InputActionVariable group is added,
                                 * then we'll just assume it's this one
                                 */
                                if (variable.Value is InputActionVariableGroup inputActionVariableGroup)
                                {
                                    if (inputActionVariableGroup.Config == _config)
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
        }

        void AutoConfigureSetup()
        {
            VariablesGroupAsset defaultGroup = null;
            PersistentVariablesSource variablesSource = null;
            foreach (var source in LocalizationSettings.StringDatabase.SmartFormatter.SourceExtensions)
            {
                if (source is PersistentVariablesSource persistentVariablesSource)
                {
                    variablesSource = persistentVariablesSource;
                    foreach (KeyValuePair<string, VariablesGroupAsset> group in persistentVariablesSource)
                    {
                        if (group.Key == DefaultVariablesGroupAssetName)
                        {
                            defaultGroup = group.Value;
                        }
                    }
                }
            }

            /*
             * Create a Variables Group Asset if it doesn't exist
             */
            if (defaultGroup == null)
            {
                defaultGroup = CreateInstance<VariablesGroupAsset>();
                defaultGroup.name = "Global Localization Variables";
                var folder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(_config));
                AssetDatabase.CreateAsset(defaultGroup, Path.Combine(folder, defaultGroup.name + ".asset"));
            }

            /*
             * Add Variables Group Asset to the Localization Settings "Variables Source"
             */
            variablesSource.Add(DefaultVariablesGroupAssetName, defaultGroup);

            /*
             * Now we need to create & add our InputActionVariableGroup (to the Asset)
             */
            var variableGroup = new InputActionVariableGroup() { Config = _config };

            Undo.RecordObject(defaultGroup, "Set Up Input Hints");

            /*
             * Need to do some hacking to add a managed Reference to the GroupsAsset
             * (just calling "add" is not enough because it uses SerializeReference)
             */
            var groupSerializedOb = new SerializedObject(defaultGroup);
            var variablesList = groupSerializedOb.FindProperty("m_Variables");
            var index = variablesList.arraySize;
            variablesList.InsertArrayElementAtIndex(index);
            var element = variablesList.GetArrayElementAtIndex(index);
            var variable = element.FindPropertyRelative("variable");
            variable.managedReferenceValue = variableGroup;

            var name = element.FindPropertyRelative("name");
            name.stringValue = DefaultVariableGroup;
            groupSerializedOb.ApplyModifiedProperties();

            // defaultGroup.Add(DefaultVariableGroup, variableGroup);

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
// (C)2024 @noio_games
// Thomas van den Berg

using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;

namespace games.noio.InputHints
{
    [CreateAssetMenu(menuName = "Noio/Input Hints Config")]
    public class InputHintsConfig : ScriptableObject
    {
        public event Action Changed;

        #region PUBLIC AND SERIALIZED FIELDS

        [SerializeField] InputActionAsset _inputActions;
        [SerializeField] string _spriteFormat = "<sprite=\"{0}\" name=\"{1}\" tint=1>";
        [SerializeField] List<SpriteCategory> _spriteCategories;
        [SerializeField] List<ControlType> _controlTypes;
        [SerializeField] List<ControlPathToSpriteMapping> _sprites;
        [SerializeField] List<MissingControlPath> _missingControlPaths;

        #endregion

        Dictionary<Guid, InputActionVariable> _generatedVariables;
        ControlType _usedControlType;

        #region PROPERTIES

        public InputActionAsset InputActions => _inputActions;

        #endregion

        #region MONOBEHAVIOUR METHODS

        void OnEnable()
        {
            InputHints.UsedDeviceChanged += HandleUsedDeviceChanged;
        }

        void OnDisable()
        {
            InputHints.UsedDeviceChanged -= HandleUsedDeviceChanged;
        }

        #endregion

        public string GetSprite(InputAction action)
        {
            if (_usedControlType == null || _usedControlType.IsEmpty)
            {
                _usedControlType = GetControlType(InputHints.UsedDevice);
            }

            var bindingIndex = action.GetBindingIndex(_usedControlType.InputControlScheme);

            // if (action.bindings[bindingIndex].isPartOfComposite)
            // {
            //     PrintBinding(action, bindingIndex - 1);
            // }

            if (bindingIndex <= -1)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"No binding found for \"{action.name}\" " +
                                 $"with Control Scheme \"{_usedControlType.InputControlScheme}\"",this);
#endif
                return $"[{action.name}]";
            }

            var path = action.bindings[bindingIndex].path;

            InputControlPath.ToHumanReadableString(path, out _, out var controlPath);

            // PrintBinding(action, bindingIndex);

            // var bindingString = action.GetBindingDisplayString(bindingIndex,
            // InputBinding.DisplayStringOptions.DontIncludeInteractions);

            foreach (var sprite in _sprites)
            {
                if (sprite.ControlPath == controlPath)
                {
                    /*
                     * See if there is a sprite mapping that matches any of the sprite
                     * sheets defined by the current ControlType
                     * The ControlType is set by the current used device.
                     *
                     * So:
                     *
                     * 1. If an Xbox controller is used, that determines the Control Type "Gamepad".
                     * 2. The Gamepad control type defines a "gamepad" sprite sheet so
                     * 3. We look for a sprite mapping that uses the "gamepad" sprite sheet
                     *
                     * ControlTypes can define MULTIPLE sprite sheets, e.g. "mouse" and "keyboard",
                     * and we just look for a sprite that matches ANY of the sprite sheets defined
                     * by the current ControlType
                     */
                    var asset =
                        _usedControlType.SpriteAssets.FirstOrDefault(
                            m => m.SpriteCategory == sprite.SpriteCategory);

                    if (asset == null)
                    {
                        continue;
                    }

                    return string.Format(_spriteFormat, asset.SpriteAsset.name, sprite.SpriteName);
                }
            }

#if UNITY_EDITOR
            Debug.LogWarning($"[No sprite found for \"{controlPath}\"]", this);
            if (_missingControlPaths.Any(mcp => mcp.Matches(controlPath, _usedControlType)) == false)
            {
                Debug.Log("Adding missing control path entry");
                _missingControlPaths.Add(new MissingControlPath(controlPath, _usedControlType));
                EditorUtility.SetDirty(this);
            }
#endif

            return $"[{controlPath}]";
        }

        void PrintBinding(InputAction action, int bindingIndex)
        {
            var binding = action.bindings[bindingIndex];
            var bindingPath = binding.path;
            var displayString = action.GetBindingDisplayString(bindingIndex, out _, out string controlPath);
            var humanReadable =
                InputControlPath.ToHumanReadableString(bindingPath, out _, out var humanReadablePath);
            var composite = binding.isPartOfComposite
                ? "(Part of Composite)"
                : binding.isComposite
                    ? "(Composite)"
                    : "";

            Debug.Log($"[{action.name}] " +
                      $"Path: \"{bindingPath}\" " +
                      $"DisplayString: \"{displayString}\"+\"{controlPath}\" " +
                      $"HumanReadable: \"{humanReadable}\"+\"{humanReadablePath}\" {composite}");
        }

        public void OnChanged()
        {
            Changed?.Invoke();
        }

        public void SetControlTypeFromDevicesString(string devicesString)
        {
            var index = _controlTypes.FindIndex(ct => ct.Devices == devicesString);

            if (index > -1)
            {
                _usedControlType = _controlTypes[index];
                Changed?.Invoke();
            }
        }

        void HandleUsedDeviceChanged(InputDevice inputDevice)
        {
            _usedControlType = GetControlType(inputDevice);
            Changed?.Invoke();
        }

        ControlType GetControlType(InputDevice usedDevice)
        {
            if (usedDevice != null)
            {
                foreach (var controlType in _controlTypes)
                {
                    var usedDevicePath = usedDevice.path;
                    if (controlType.DeviceMatcher.IsMatch(usedDevicePath))
                    {
                        return controlType;
                    }
                }
            }

            /*
             * If no controltypes match (or UsedDevice is not set if e.g. the game is not playing),
             * then use the LAST control type, because the last position in the list is also where you
             * would place the default 'fall-through' option that matches any device that
             * wasn't matched by the earlier control types.
             */
            return _controlTypes.Count > 0 ? _controlTypes[^1] : null;
        }

#if UNITY_EDITOR

        #region EDITOR

        void Reset()
        {
            _inputActions = AssetDatabase.FindAssets("t:InputActionAsset")
                                         .Select(AssetDatabase.GUIDToAssetPath)
                                         .Select(AssetDatabase.LoadAssetAtPath<InputActionAsset>)
                                         .FirstOrDefault();

            if (_inputActions == null)
            {
                Debug.LogError("No Input Actions Asset Found.");
            }

            _spriteCategories = new List<SpriteCategory>
            {
                new("Keyboard"),
                new("Mouse"),
                new("Gamepad")
            };

            var allSpriteAssets = GetSpriteAssetsInProject();

            _controlTypes = new List<ControlType>
            {
                new("(Mouse|Keyboard)", "Keyboard&Mouse", new List<SpriteCategoryToAssetMapping>
                {
                    new("Keyboard", allSpriteAssets.Length > 0 ? allSpriteAssets[0] : null),
                    new("Mouse", allSpriteAssets.Length > 0 ? allSpriteAssets[0] : null)
                })
            };
        }

        public IEnumerable<string> GetSpriteCategories()
        {
            return _spriteCategories.Select(s => s.Name);
        }

        static TMP_SpriteAsset[] GetSpriteAssetsInProject()
        {
            if (TMP_Settings.instance != null)
            {
                var spriteAssetsPath = TMP_Settings.defaultSpriteAssetPath;
                var spriteAssets = Resources.LoadAll<TMP_SpriteAsset>(spriteAssetsPath);
                return spriteAssets;
            }
            else
            {
                return Array.Empty<TMP_SpriteAsset>();
            }
        }

        public IEnumerable<InputControlScheme> GetInputControlSchemes()
        {
            return _inputActions == null
                ? Enumerable.Empty<InputControlScheme>()
                : _inputActions.controlSchemes;
        }

        public void AddSprite(string controlPath, string spriteName, string controlScheme)
        {
            Undo.RecordObject(this, "Add Sprite Mapping");

            var category = FindCategoryFor(spriteName, controlScheme);

            var mapping = _sprites.FirstOrDefault(m => m.ControlPath == controlPath);
            if (mapping == null)
            {
                mapping = new ControlPathToSpriteMapping(controlPath, spriteName, category);
                _sprites.Add(mapping);
            }
            else
            {
                mapping.SpriteName = spriteName;
            }

            LocalizationSettings.SelectedLocale = LocalizationSettings.SelectedLocale;

            OnChanged();
        }

        /// <summary>
        ///     Starting from the ControlType that matches the given controlScheme,
        ///     finds the first Category that has a sprite with the given name.
        /// </summary>
        /// <param name="spriteName"></param>
        /// <param name="controlScheme"></param>
        /// <returns></returns>
        string FindCategoryFor(string spriteName, string controlScheme)
        {
            var allSpriteAssets = GetSpriteAssetsInProject().ToDictionary(asset => asset.name);

            foreach (var controlType in _controlTypes)
            {
                if (controlType.InputControlScheme != controlScheme)
                {
                    continue;
                }

                foreach (var mapping in controlType.SpriteAssets)
                {
                    if (mapping.SpriteAsset != null &&
                        allSpriteAssets.TryGetValue(mapping.SpriteAsset.name, out var asset))
                    {
                        if (asset.spriteCharacterTable.Any(sc => sc.name == spriteName))
                        {
                            return mapping.SpriteCategory;
                        }
                    }
                }
            }

            return null;
        }

        public void ClearMissingControlPath(string controlPath, string controlScheme)
        {
            var index = _missingControlPaths.FindIndex(missing =>
                missing.ControlPath == controlPath && missing.ControlScheme == controlScheme);
            if (index > -1)
            {
                Undo.RecordObject(this, $"Clear Missing Control Path Entry for {controlPath}");
                _missingControlPaths.RemoveAt(index);
            }
        }

        #endregion

#endif
    }
}
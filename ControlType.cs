// (C)2024 @noio_games
// Thomas van den Berg

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace games.noio.InputHints
{
    [Serializable]
    public class ControlType
    {
        #region PUBLIC AND SERIALIZED FIELDS

        [Tooltip("A regular expression that is matched against the current Input Device's path.")]
        [SerializeField]
        string _devices = "";

        [SerializeField] string _inputControlScheme;
        [SerializeField] List<SpriteCategoryToAssetMapping> _spriteAssets;

        #endregion

        Regex _deviceMatcher;

        public ControlType(string devices, string inputControlScheme, List<SpriteCategoryToAssetMapping> spriteAssets)
        {
            _devices = devices;
            _inputControlScheme = inputControlScheme;
            _spriteAssets = spriteAssets;
        }

        #region PROPERTIES

        public string InputControlScheme
        {
            get => _inputControlScheme;
            set => _inputControlScheme = value;
        }

        public Regex DeviceMatcher
        {
            get
            {
                if (_deviceMatcher == null)
                {
                    try
                    {
                        _deviceMatcher = new Regex(_devices);
                    }
                    catch (Exception)
                    {
                        _deviceMatcher = new Regex(@"^\b$"); // Match nothing 
                    }
                }

                return _deviceMatcher;
            }
        }

        public List<SpriteCategoryToAssetMapping> SpriteAssets => _spriteAssets;

        #endregion
    }
}
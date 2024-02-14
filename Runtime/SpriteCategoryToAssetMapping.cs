// (C)2024 @noio_games
// Thomas van den Berg

using System;
using TMPro;
using UnityEngine;

namespace games.noio.InputHints
{
    /// <summary>
    ///     Maps a Sprite Category to a specific Sprite Sheet for a particular Control type.
    ///     For example, the control type that matches "(DualSense)" could
    ///     map the category "Gamepad" to the SpriteSheet "DualSense-Filled"
    /// </summary>
    [Serializable]
    public class SpriteCategoryToAssetMapping
    {
        #region PUBLIC AND SERIALIZED FIELDS

        [Tooltip("Sprite Category as defined in Input Hints Config")]
        [SerializeField]
        string _spriteCategory;

        [SerializeField]
        TMP_SpriteAsset _spriteAsset;

        public SpriteCategoryToAssetMapping(string spriteCategory, TMP_SpriteAsset spriteAsset)
        {
            _spriteCategory = spriteCategory;
            _spriteAsset = spriteAsset;
        }

        #endregion

        #region PROPERTIES

        public string SpriteCategory
        {
            get => _spriteCategory;
            set => _spriteCategory = value;
        }

        public TMP_SpriteAsset SpriteAsset => _spriteAsset;

        #endregion
    }
}
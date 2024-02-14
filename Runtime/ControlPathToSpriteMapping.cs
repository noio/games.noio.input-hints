// (C)2024 @noio_games
// Thomas van den Berg

using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace games.noio.InputHints
{
    [Serializable]
    public class ControlPathToSpriteMapping
    {
        #region PUBLIC AND SERIALIZED FIELDS

        [SerializeField]
        string _controlPath;

        [SerializeField]
        string _spriteName;

        [SerializeField]
        string _spriteCategory;

        #endregion

        public ControlPathToSpriteMapping(string controlPath, string spriteName, string spriteCategory)
        {
            _controlPath = controlPath;
            _spriteName = spriteName;
            _spriteCategory = spriteCategory;
        }

        #region PROPERTIES

        public string ControlPath => _controlPath;
        public string SpriteName
        {
            get => _spriteName;
            set => _spriteName = value;
        }

        public string SpriteCategory => _spriteCategory;

        #endregion
    }
}
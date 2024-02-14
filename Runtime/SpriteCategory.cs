// (C)2024 @noio_games
// Thomas van den Berg

using System;
using UnityEngine;

namespace games.noio.InputHints
{
    [Serializable]
    internal class SpriteCategory
    {
        #region PUBLIC AND SERIALIZED FIELDS

        [SerializeField] string _name;

        public SpriteCategory(string name)
        {
            _name = name;
        }

        #endregion

        #region PROPERTIES

        public string Name => _name;

        #endregion
    }
}
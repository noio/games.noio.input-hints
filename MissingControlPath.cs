// (C)2024 @noio_games
// Thomas van den Berg

using System;
using UnityEngine;

namespace games.noio.InputHints
{
    [Serializable]
    public class MissingControlPath
    {
        #region PUBLIC AND SERIALIZED FIELDS

        [SerializeField] string _controlPath;
        [SerializeField] string _controlScheme;

        #endregion

        public MissingControlPath(string controlPath, ControlType controlType)
        {
            _controlPath = controlPath;
            _controlScheme = controlType.InputControlScheme;
        }

        #region PROPERTIES

        public string ControlPath => _controlPath;
        public string ControlScheme => _controlScheme;

        #endregion

        public bool Matches(string controlPath, ControlType controlType)
        {
            return controlPath == _controlPath && controlType.InputControlScheme == _controlScheme;
        }
    }
}
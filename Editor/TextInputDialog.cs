// (C)2024 @noio_games
// Thomas van den Berg

using System;
using UnityEditor;
using UnityEngine;

namespace games.noio.InputHints.Editor
{
    internal class TextInputDialog : EditorWindow
    {
        string _description;
        string _inputText;
        string _okButton;
        string _cancelButton;
        bool _initializedPosition;
        Action _onOKButton;
        bool _shouldClose;
        Vector2 _maxScreenPos;

        #region MONOBEHAVIOUR METHODS

        #region OnGUI()

        void OnGUI()
        {
            // Check if Esc/Return have been pressed
            var e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                switch (e.keyCode)
                {
                    // Escape pressed
                    case KeyCode.Escape:
                        _shouldClose = true;
                        e.Use();
                        break;

                    // Enter pressed
                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                        _onOKButton?.Invoke();
                        _shouldClose = true;
                        e.Use();
                        break;
                }
            }

            if (_shouldClose)
            {
                // Close this dialog
                Close();

                //return;
            }

            // Draw our control
            var rect = EditorGUILayout.BeginVertical();

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField(_description);

            EditorGUILayout.Space(8);
            GUI.SetNextControlName("inText");
            _inputText = EditorGUILayout.TextField("", _inputText);
            GUI.FocusControl("inText"); // Focus text field
            EditorGUILayout.Space(12);

            // Draw OK / Cancel buttons
            var r = EditorGUILayout.GetControlRect();
            r.width /= 2;
            if (GUI.Button(r, _okButton))
            {
                _onOKButton?.Invoke();
                _shouldClose = true;
            }

            r.x += r.width;
            if (GUI.Button(r, _cancelButton))
            {
                _inputText = null; // Cancel - delete inputText
                _shouldClose = true;
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.EndVertical();

            // Force change size of the window
            if (rect.width != 0 && minSize != rect.size)
            {
                minSize = maxSize = rect.size;
            }

            // Set dialog position next to mouse position
            if (!_initializedPosition && e.type == EventType.Layout)
            {
                _initializedPosition = true;

                // Move window to a new position. Make sure we're inside visible window
                var mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                mousePos.x += 32;
                if (mousePos.x + position.width > _maxScreenPos.x)
                {
                    mousePos.x -= position.width + 64; // Display on left side of mouse
                }

                if (mousePos.y + position.height > _maxScreenPos.y)
                {
                    mousePos.y = _maxScreenPos.y - position.height;
                }

                position = new Rect(mousePos.x, mousePos.y, position.width, position.height);

                // Focus current window
                Focus();
            }
        }

        #endregion OnGUI()

        #endregion

        #region Show()

        /// <summary>
        ///     Returns text player entered, or null if player cancelled the dialog.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="inputText"></param>
        /// <param name="okButton"></param>
        /// <param name="cancelButton"></param>
        /// <returns></returns>
        public static string Show(
            string title,
            string description,
            string inputText,
            string okButton     = "OK",
            string cancelButton = "Cancel")
        {
            // Make sure our popup is always inside parent window, and never offscreen
            // So get caller's window size
            var maxPos = GUIUtility.GUIToScreenPoint(new Vector2(Screen.width, Screen.height));

            string ret = null;

            //var window = EditorWindow.GetWindow<InputDialog>();
            var window = CreateInstance<TextInputDialog>();
            window._maxScreenPos = maxPos;
            window.titleContent = new GUIContent(title);
            window._description = description;
            window._inputText = inputText;
            window._okButton = okButton;
            window._cancelButton = cancelButton;
            window._onOKButton += () => ret = window._inputText;

            //window.ShowPopup();
            window.ShowModal();

            return ret;
        }

        #endregion Show()
    }
}
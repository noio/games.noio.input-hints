// (C)2024 @noio_games
// Thomas van den Berg

using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace games.noio.InputHints.Editor
{
    public class SpriteSearchWindow : EditorWindow
    {
        public event Action<string> SpritePicked;
        List<(TMP_SpriteAsset spriteAsset, TMP_SpriteCharacter spriteCharacter)> _sprites;
        Vector2 _scrollPosition;

        Dictionary<string, List<(TMP_SpriteAsset spriteAsset, TMP_SpriteCharacter spriteCharacter)>>
            _sprites2;

        List<string> _spriteNames;
        string _searchString;
        bool _initializedPosition;
        bool _shouldClose;
        Vector2 MaxScreenPos { get; set; }

        #region MONOBEHAVIOUR METHODS

        void OnEnable()
        {
            _sprites2 =
                new Dictionary<string,
                    List<(TMP_SpriteAsset spriteAsset, TMP_SpriteCharacter spriteCharacter)>>();

            var spriteSheets = AssetDatabase.FindAssets("t:TMP_SpriteAsset")
                                            .Select(AssetDatabase.GUIDToAssetPath)
                                            .Select(AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>)
                                            .Where(sheet => sheet != null)
                                            .ToList();

            foreach (var spriteAsset in spriteSheets)
            {
                foreach (var character in spriteAsset.spriteCharacterTable)
                {
                    if (_sprites2.TryGetValue(character.name, out var sprites) == false)
                    {
                        _sprites2[character.name] = sprites =
                            new List<(TMP_SpriteAsset spriteAsset, TMP_SpriteCharacter spriteCharacter)>();
                    }

                    sprites.Add((spriteAsset, character));
                }
            }

            _spriteNames = _sprites2.Keys.ToList();
            _spriteNames.Sort();

            spriteSheets.SelectMany(spriteAsset =>
                            spriteAsset.spriteCharacterTable.Select(spriteCharacter =>
                                (spriteAsset, spriteCharacter)))
                        .ToList();
        }

        void OnGUI()
        {
            
            
            if (_shouldClose)
            {
                Close();
            }

            /*
             * SEARCH BAR
             */
            using (new GUILayout.HorizontalScope(GUI.skin.FindStyle("Toolbar")))
            {
                GUILayout.FlexibleSpace();
                GUI.SetNextControlName("spriteSearch");
                _searchString = GUILayout.TextField(_searchString,
                    GUI.skin.FindStyle("ToolbarSearchTextField"),
                    GUILayout.Width(150));
                if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSearchCancelButton")))
                {
                    // Remove focus if cleared
                    _searchString = "";
                    GUI.FocusControl(null);
                }
            }

            /*
             * SPRITE LISTING
             */
            using (var scrollView = new EditorGUILayout.ScrollViewScope(_scrollPosition))
            {
                foreach (var spriteName in _spriteNames)
                {
                    if (string.IsNullOrEmpty(_searchString) == false &&
                        spriteName.ToLower().Contains(_searchString.ToLower()) == false)
                    {
                        continue;
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Pick", GUILayout.Width(40)))
                        {
                            SpritePicked?.Invoke(spriteName);
                            _shouldClose = true;
                        }

                        EditorGUILayout.LabelField(spriteName, EditorStyles.boldLabel);
                    }

                    GUILayout.Space(5);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Space(16);
                        foreach (var (spriteAsset, spriteCharacter) in _sprites2[spriteName])
                        {
                            using (new EditorGUILayout.VerticalScope(GUILayout.Width(50)))
                            {
                                var rect = EditorGUILayout.GetControlRect(false, 40,
                                    GUILayout.ExpandWidth(true));
                                rect.width = 40;
                                DrawSpriteGlyph(rect, spriteCharacter, spriteAsset);
                                GUI.Label(rect, new GUIContent("", spriteAsset.name));
                            }
                        }
                    }

                    GUILayout.Space(10);
                }

                _scrollPosition = scrollView.scrollPosition;
            }
            
            /*
             * INITIALIZATION
             */
            var e = Event.current;

            // Set dialog position next to mouse position
            if (!_initializedPosition && e.type == EventType.Layout)
            {
                _initializedPosition = true;

                // Move window to a new position. Make sure we're inside visible window
                var mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                mousePos.x += 32;
                if (mousePos.x + position.width > MaxScreenPos.x)
                {
                    mousePos.x -= position.width + 64; // Display on left side of mouse
                }

                if (mousePos.y + position.height > MaxScreenPos.y)
                {
                    mousePos.y = MaxScreenPos.y - position.height;
                }

                position = new Rect(mousePos.x, mousePos.y, position.width, position.height);

                // Focus current window
                GUI.FocusControl("spriteSearch");
                Focus();
            }
        }

        #endregion

        public static string ShowWindow()
        {
            var window = CreateInstance<SpriteSearchWindow>();
            window.titleContent = new GUIContent("Sprite Listing");
            window.minSize = window.maxSize = new Vector2(600, 700);
            window.MaxScreenPos = GUIUtility.GUIToScreenPoint(new Vector2(Screen.width, Screen.height));
            string pickedSprite = null;
            window.SpritePicked += spriteName => pickedSprite = spriteName; 
            
            window.ShowModal();
            
            return pickedSprite;
        }

        void DrawSpriteGlyph(Rect position, TMP_SpriteCharacter character, TMP_SpriteAsset spriteAsset)
        {
            // Get a reference to the sprite glyph table
            // TMP_SpriteAsset spriteAsset = property.serializedObject.targetObject as TMP_SpriteAsset;

            if (spriteAsset == null)
            {
                return;
            }

            // int glyphIndex = property.FindPropertyRelative("m_GlyphIndex").intValue;
            var glyphIndex = (int)character.glyphIndex;

            // Lookup glyph and draw glyph (if available)
            var elementIndex = spriteAsset.spriteGlyphTable.FindIndex(item => item.index == glyphIndex);

            if (elementIndex != -1)
            {
                var glyphRect = spriteAsset.spriteGlyphTable[elementIndex].glyphRect;

                // Get a reference to the sprite texture
                var tex = spriteAsset.spriteSheet;

                // Return if we don't have a texture assigned to the sprite asset.
                if (tex == null)
                {
                    Debug.LogWarning(
                        "Please assign a valid Sprite Atlas texture to the [" + spriteAsset.name +
                        "] Sprite Asset.", spriteAsset);
                    return;
                }

                var spriteTexPosition = new Vector2(position.x, position.y);
                var spriteSize = new Vector2(48, 48);
                var alignmentOffset = new Vector2((58 - spriteSize.x) / 2, (58 - spriteSize.y) / 2);

                float x = glyphRect.x;
                float y = glyphRect.y;
                float spriteWidth = glyphRect.width;
                float spriteHeight = glyphRect.height;

                if (spriteWidth >= spriteHeight)
                {
                    spriteSize.y = spriteHeight * spriteSize.x / spriteWidth;
                    spriteTexPosition.y += (spriteSize.x - spriteSize.y) / 2;
                }
                else
                {
                    spriteSize.x = spriteWidth * spriteSize.y / spriteHeight;
                    spriteTexPosition.x += (spriteSize.y - spriteSize.x) / 2;
                }

                // Compute the normalized texture coordinates
                var texCoords = new Rect(x / tex.width, y / tex.height, spriteWidth / tex.width,
                    spriteHeight / tex.height);
              
                GUI.DrawTextureWithTexCoords(position, tex, texCoords, true);
            }
        }
    }
}
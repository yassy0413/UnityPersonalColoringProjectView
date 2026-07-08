using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ColoringProjectView
{
    public sealed class PersonalColoringProjectViewWindow : EditorWindow
    {
        private const string Title = "Personal Coloring Project View";

        private ReorderableList _reorderableList;
        private GUIStyle _sectionGuiStyle;

        private static PersonalColoringProjectViewSettings _settings;

        internal static PersonalColoringProjectViewSettings Settings =>
            _settings ??= new PersonalColoringProjectViewSettings();

        [MenuItem("Window/" + Title, false)]
        internal static void ShowWindow()
        {
            var window = GetWindow<PersonalColoringProjectViewWindow>(Title);
            window.minSize = new Vector2(480, 320);
            window.Show();
        }

        internal static void SaveAndRepaint()
        {
            Settings.Save();
            EditorApplication.RepaintProjectWindow();

            foreach (var window in Resources.FindObjectsOfTypeAll<PersonalColoringProjectViewWindow>())
            {
                window.Repaint();
            }
        }

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            EditorApplication.projectWindowItemOnGUI += ProjectWindowItemOnGUI;
        }

        private static void ProjectWindowItemOnGUI(string guid, Rect rect)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var unit = Settings.Units.FirstOrDefault(x => x.IsMatch(path));

            if (unit == null)
            {
                return;
            }

            EditorGUI.DrawRect(rect, unit.Color);
        }

        private void OnGUI()
        {
            _sectionGuiStyle ??= new GUIStyle
            {
                normal = new GUIStyleState { textColor = Color.white },
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            EditorGUILayout.Space(8);
            GUILayout.Label("Personal Coloring Project View", _sectionGuiStyle, GUILayout.ExpandWidth(true));
            EditorGUILayout.Space(4);
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(2));
            EditorGUILayout.Space(4);

            GUILayout.Label("examples)");
            GUILayout.Label("  Assets/Project/Chapter[0-9]+/?$");
            GUILayout.Label("  Assets/Project/Chapter.*/.*.unity");

            EditorGUILayout.Space(4);
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(2));
            EditorGUILayout.Space(4);

            SetupReorderableList();
            _reorderableList?.DoLayoutList();
        }

        private void SetupReorderableList()
        {
            if (_reorderableList != null)
            {
                return;
            }

            var elementType = typeof(PersonalColoringProjectViewSettings.Unit);

            _reorderableList = new ReorderableList(Settings.Units, elementType)
            {
                draggable = true,
                multiSelect = true,
                elementHeightCallback = index => EditorGUIUtility.singleLineHeight,
                headerHeight = 0,

                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    rect.height = EditorGUIUtility.singleLineHeight;

                    var unit = Settings.Units[index];
                    var modified = false;

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        const float ColorPickerWidth = 48;
                        var pathFieldRect = new Rect(rect.x, rect.y, rect.width - ColorPickerWidth, rect.height);
                        var colorFieldRect = new Rect(pathFieldRect.xMax, rect.y, ColorPickerWidth, rect.height);

                        var pathPattern = EditorGUI.TextField(pathFieldRect, unit.PathPattern);
                        var color = EditorGUI.ColorField(colorFieldRect, unit.Color);

                        if (pathPattern != unit.PathPattern)
                        {
                            unit.SetPathPattern(pathPattern);
                            modified = true;
                        }

                        if (color != unit.Color)
                        {
                            unit.Color = color;
                            modified = true;
                        }
                    }

                    if (modified)
                    {
                        Settings.Save();
                    }
                },

                onAddCallback = list =>
                {
                    PersonalColoringProjectViewUndo.Record("Add Coloring Project View Item");
                    Settings.Units.Add(new PersonalColoringProjectViewSettings.Unit
                    {
                        Color = new Color(Color.cyan.r, Color.cyan.g, Color.cyan.b, 0.1f)
                    });
                    PersonalColoringProjectViewUndo.Commit();
                },

                onRemoveCallback = list =>
                {
                    if (Settings.Units.Count <= 0)
                    {
                        return;
                    }

                    if (list.selectedIndices.Count > 0)
                    {
                        PersonalColoringProjectViewUndo.Record("Remove Coloring Project View Item");

                        foreach (var index in list.selectedIndices.Reverse())
                        {
                            Settings.Units.RemoveAt(index);
                        }
                    }
                    else
                    {
                        PersonalColoringProjectViewUndo.Record("Remove Coloring Project View Item");
                        Settings.Units.RemoveAt(Settings.Units.Count - 1);
                    }

                    PersonalColoringProjectViewUndo.Commit();
                }
            };
        }
    }
}

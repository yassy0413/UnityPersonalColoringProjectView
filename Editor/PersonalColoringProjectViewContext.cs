using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ColoringProjectView
{
    public static class PersonalColoringProjectViewContext
    {
        private const string OpenSettingsWindowMenuPath =
            "Assets/PersonalColoringProjectView/Open Settings Window";

        private const string AddFolderMenuPath =
            "Assets/PersonalColoringProjectView/Add this folder";

        private const string AddFolderHierarchyMenuPath =
            "Assets/PersonalColoringProjectView/Add this folder hierarchy";

        private const string RemoveFolderMenuPath =
            "Assets/PersonalColoringProjectView/Remove this folder";

        private static readonly Color DefaultColor = new Color(Color.cyan.r, Color.cyan.g, Color.cyan.b, 0.1f);

        [MenuItem(OpenSettingsWindowMenuPath, false)]
        private static void OpenSettingsWindow()
        {
            PersonalColoringProjectViewWindow.ShowWindow();
        }

        [MenuItem(AddFolderMenuPath, false)]
        private static void AddFolder()
        {
            if (!TryGetSelectedFolderPath(out var path))
            {
                return;
            }

            AddPathPattern(ToFolderPattern(path));
        }

        [MenuItem(AddFolderMenuPath, true)]
        private static bool ValidateAddFolder() => TryGetSelectedFolderPath(out _);

        [MenuItem(AddFolderHierarchyMenuPath, false)]
        private static void AddFolderHierarchy()
        {
            if (!TryGetSelectedFolderPath(out var path))
            {
                return;
            }

            AddPathPattern(ToFolderHierarchyPattern(path));
        }

        [MenuItem(AddFolderHierarchyMenuPath, true)]
        private static bool ValidateAddFolderHierarchy() => TryGetSelectedFolderPath(out _);

        [MenuItem(RemoveFolderMenuPath, false)]
        private static void RemoveFolder()
        {
            if (!TryGetSelectedFolderPath(out var path))
            {
                return;
            }

            var folderPattern = ToFolderPattern(path);
            var folderHierarchyPattern = ToFolderHierarchyPattern(path);
            var settings = PersonalColoringProjectViewWindow.Settings;

            if (!settings.Units.Any(x => x.PathPattern == folderPattern || x.PathPattern == folderHierarchyPattern))
            {
                return;
            }

            PersonalColoringProjectViewUndo.Record("Remove Coloring Project View Folder");
            settings.Units.RemoveAll(x => x.PathPattern == folderPattern || x.PathPattern == folderHierarchyPattern);
            PersonalColoringProjectViewUndo.Commit();
        }

        [MenuItem(RemoveFolderMenuPath, true)]
        private static bool ValidateRemoveFolder()
        {
            if (!TryGetSelectedFolderPath(out var path))
            {
                return false;
            }

            var folderPattern = ToFolderPattern(path);
            var folderHierarchyPattern = ToFolderHierarchyPattern(path);
            return PersonalColoringProjectViewWindow.Settings.Units.Any(x =>
                x.PathPattern == folderPattern || x.PathPattern == folderHierarchyPattern);
        }

        private static void AddPathPattern(string pathPattern)
        {
            var settings = PersonalColoringProjectViewWindow.Settings;
            if (settings.Units.Any(x => x.PathPattern == pathPattern))
            {
                return;
            }

            PersonalColoringProjectViewUndo.Record("Add Coloring Project View Folder");

            settings.Units.Add(new PersonalColoringProjectViewSettings.Unit
            {
                PathPattern = pathPattern,
                Color = DefaultColor
            });

            PersonalColoringProjectViewUndo.Commit();
        }

        private static bool TryGetSelectedFolderPath(out string path)
        {
            path = string.Empty;

            var activeObjects = Selection.GetFiltered<Object>(SelectionMode.Assets);
            if (activeObjects.Length != 1)
            {
                return false;
            }

            path = AssetDatabase.GetAssetPath(activeObjects[0]);
            return !string.IsNullOrEmpty(path) && AssetDatabase.IsValidFolder(path);
        }

        private static string ToFolderPattern(string path) => $"^{Regex.Escape(path)}/?$";

        private static string ToFolderHierarchyPattern(string path) => $"^{Regex.Escape(path)}(/.*)?$";
    }
}

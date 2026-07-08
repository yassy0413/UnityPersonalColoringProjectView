using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ColoringProjectView
{
    [Serializable]
    public sealed class PersonalColoringProjectViewSettings
    {
        private static string SettingsDataPath =>
            Path.Join(Application.persistentDataPath, "PersonalColoringProjectView.json");

        [SerializeField]
        private List<Unit> _units = new();

        public List<Unit> Units => _units;

        [Serializable]
        public sealed partial class Unit
        {
            public string PathPattern;
            public Color Color;

            [NonSerialized]
            private Regex PathPatternRegex;

            public void SetPathPattern(string pathPattern)
            {
                PathPattern = pathPattern;
                PathPatternRegex = null;
            }

            public bool IsMatch(string path)
            {
                if (string.IsNullOrEmpty(PathPattern))
                {
                    return false;
                }

                PathPatternRegex ??= new Regex(PathPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                return PathPatternRegex.IsMatch(path);
            }
        }

        public PersonalColoringProjectViewSettings()
        {
            Load();
        }

        private void Load()
        {
            if (!File.Exists(SettingsDataPath))
            {
                return;
            }

            try
            {
                var json = File.ReadAllText(SettingsDataPath);
                JsonUtility.FromJsonOverwrite(json, this);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public void Save()
        {
            try
            {
                File.WriteAllText(SettingsDataPath, ToJson());
            }
            catch (Exception)
            {
                // ignored
            }
        }

        internal string ToJson() => JsonUtility.ToJson(this);

        internal void RestoreJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return;
            }

            JsonUtility.FromJsonOverwrite(json, this);
            InvalidateCaches();
        }

        private void InvalidateCaches()
        {
            foreach (var unit in Units)
            {
                unit.InvalidateCache();
            }
        }

        public sealed partial class Unit
        {
            internal void InvalidateCache()
            {
                PathPatternRegex = null;
            }
        }
    }
}

using UnityEditor;
using UnityEngine;

namespace ColoringProjectView
{
    [InitializeOnLoad]
    internal static class PersonalColoringProjectViewUndo
    {
        private sealed class UndoState : ScriptableObject
        {
            [SerializeField]
            private string _settingsJson = string.Empty;

            public string SettingsJson
            {
                get => _settingsJson;
                set => _settingsJson = value;
            }
        }

        private static UndoState _state;

        private static UndoState State
        {
            get
            {
                if (_state != null)
                {
                    return _state;
                }

                _state = ScriptableObject.CreateInstance<UndoState>();
                _state.hideFlags = HideFlags.HideAndDontSave;
                _state.SettingsJson = PersonalColoringProjectViewWindow.Settings.ToJson();
                return _state;
            }
        }

        static PersonalColoringProjectViewUndo()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        public static void Record(string undoName)
        {
            State.SettingsJson = PersonalColoringProjectViewWindow.Settings.ToJson();
            Undo.RecordObject(State, undoName);
        }

        public static void Commit()
        {
            State.SettingsJson = PersonalColoringProjectViewWindow.Settings.ToJson();
            EditorUtility.SetDirty(State);
            PersonalColoringProjectViewWindow.SaveAndRepaint();
        }

        private static void OnUndoRedoPerformed()
        {
            PersonalColoringProjectViewWindow.Settings.RestoreJson(State.SettingsJson);
            PersonalColoringProjectViewWindow.SaveAndRepaint();
        }
    }
}

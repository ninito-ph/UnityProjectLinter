using Ninito.UnityProjectLinter.Editor.Settings;
using UnityEditor;
using UnityEngine;

namespace Ninito.UnityProjectLinter.Editor.Settings
{
    /// <summary>
    ///     A custom inspector for Asset Linting Settings
    /// </summary>
    [CustomEditor(typeof(AssetLintingSettings))]
    public sealed class AssetLintingSettingsEditor : UnityEditor.Editor
    {
        #region Unity Callbacks

        public override void OnInspectorGUI()
        {
            if (!GUILayout.Button("Open Unity Project Linter Settings")) return;
            SettingsService.OpenProjectSettings("Project/Unity Project Linter");
        }

        #endregion
    }
}
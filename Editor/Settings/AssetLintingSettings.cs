using System.Collections.Generic;
using System.IO;
using Ninito.UnityProjectLinter.Editor.Rules;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ninito.UnityProjectLinter.Editor.Settings
{
    public sealed class AssetLintingSettings : ScriptableObject
    {
        #region Private Fields

        [Header("General Settings")]
        [SerializeField]
        [Tooltip("Whether to send a warning on the console when an asset is named incorrectly.")]
        public bool WarnOnIncorrect = true;

        [Header("Default Rules")]
        [SerializeField]
        [Tooltip("Whether script assets should be ignored from the linting process.")]
        public bool IgnoreScriptAssets = true;

        [SerializeField]
        [Tooltip("Custom naming rules to lint assets with.")]
        public List<NamingRule> NamingRules = new List<NamingRule>();

        [SerializeField]
        [Tooltip("The ignored paths (folders). Assets inside ignored folders and their subfolders won't be linted.")]
        public List<string> IgnoredPaths = new List<string>();

        [SerializeField]
        [Tooltip("The ignored assets. These assets will not be linted.")]
        public List<Object> IgnoredAssets = new List<Object>();

        #endregion

        #region Internal Methods

        /// <summary>
        /// Gets or creates event import settings.
        /// </summary>
        internal static AssetLintingSettings GetOrCreateSettings()
        {
            AssetLintingSettings settings =
                Resources.Load("Editor/ALS_UnityProjectLinterSettings") as AssetLintingSettings;

            settings ??= CreateSettings();

            return settings;
        }

        /// <summary>
        /// Gets or creates event import settings as a serialized object.
        /// </summary>
        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Ignores a new path
        /// </summary>
        /// <param name="path">The path to ignore</param>
        public void IgnorePath(string path)
        {
            if (IgnoredPaths.Contains(path)) return;
            IgnoredPaths.Add(path);
        }

        /// <summary>
        ///     Ignores a new asset
        /// </summary>
        /// <param name="assetPath">The path of the asset to ignore</param>
        public void IgnoreAsset(string assetPath)
        {
            Object loadedAsset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (IgnoredAssets.Contains(loadedAsset)) return;
            IgnoredAssets.Add(loadedAsset);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates a new settings object.
        /// </summary>
        /// <returns>The newly created settings object</returns>
        private static AssetLintingSettings CreateSettings()
        {
            if (!Directory.Exists("Assets/Resources"))
            {
                Directory.CreateDirectory("Assets/Resources");
            }

            if (!Directory.Exists("Assets/Resources/Editor"))
            {
                Directory.CreateDirectory("Assets/Resources/Editor");
            }

            AssetLintingSettings settings = CreateInstance<AssetLintingSettings>();
            AssetDatabase.CreateAsset(settings, "Assets/Resources/Editor/ALS_UnityProjectLinterSettings.asset");
            AssetDatabase.SaveAssets();
            return settings;
        }

        #endregion
    }
}
using System.Collections.Generic;
using System.Linq;
using Ninito.UnityProjectLinter.Editor.Linter;
using Ninito.UnityProjectLinter.Editor.Logging;
using Ninito.UnityProjectLinter.Editor.Settings;
using Ninito.UnityProjectLinter.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace Ninito.UnityProjectLinter.Editor.AssetPostprocessors
{
    /// <summary>
    ///     An asset PostProcessor that ensures all assets are named correctly
    /// </summary>
    public sealed class AssetNameReceiver : AssetPostprocessor
    {
        #region Private Fields

        private static AssetLintingSettings _lintingSettings;

        #endregion

        #region Properties

        private static AssetLintingSettings LintingSettings
        {
            get
            {
                if (_lintingSettings == null)
                {
                    _lintingSettings = AssetLintingSettings.GetOrCreateSettings();
                }

                return _lintingSettings;
            }
        }

        #endregion

        #region AssetPostprocessor Implementation

        public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            HashSet<string> lintedAssets = new HashSet<string>();
            int violatingAssets = 0;

            violatingAssets += LintAssets(importedAssets, ref lintedAssets);
            violatingAssets += LintAssets(movedAssets, ref lintedAssets);

            if (violatingAssets > 1 && LintingSettings.WarnOnIncorrect)
            {
                Debug.LogWarning($"<b>{violatingAssets.ToString()} assets</b> are not following naming conventions!");
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Gets all asset paths in the project
        /// </summary>
        /// <returns>All asset paths in the project</returns>
        public static IEnumerable<string> GetAllAssetPaths()
        {
            return AssetDatabase.FindAssets("", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath).Where(assetPath => assetPath.Contains("."));
        }

        /// <summary>
        ///     Lints a collection of AssetPaths, and outputs how many assets were violating the rules
        /// </summary>
        /// <param name="assetPaths">The asset paths to lint</param>
        /// <param name="examinedAssets">The hashset of already examined assets</param>
        /// <returns>The number of assets violating the rules</returns>
        public static int LintAssets(IEnumerable<string> assetPaths, ref HashSet<string> examinedAssets)
        {
            int violatingAssets = 0;

            foreach (string movedAssetPath in assetPaths)
            {
                if (HasAssetBeenLinted(examinedAssets, movedAssetPath)) continue;
                if (AssetNameLinter.IsAssetNamedAccordingToRule(movedAssetPath)) continue;

                if (LintingSettings.WarnOnIncorrect)
                {
                    WarnAssetNameViolatesRules(movedAssetPath);
                }

                violatingAssets++;
            }

            return violatingAssets;
        }

        /// <summary>
        ///     Logs all violating assets with a rule violation logger
        /// </summary>
        /// <param name="logger">The logger to log violations with</param>
        public static void LogViolatingAssets(IRuleViolationLogger logger)
        {
            foreach (string path in GetAllAssetPaths())
            {
                if (AssetNameLinter.IsAssetNamedAccordingToRule(path)) continue;
                logger.LogViolation(path);
            }
        }

        /// <summary>
        ///     Gets all violating assets
        /// </summary>
        public static IEnumerable<string> GetAllViolatingAssetPaths()
        {
            return (from path in GetAllAssetPaths()
                where !AssetNameLinter.IsAssetNamedAccordingToRule(path)
                select path).ToList();
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Checks whether an asset has already been linted
        /// </summary>
        /// <param name="lintedAssets">The set of already-linted assets</param>
        /// <param name="assetPath">The asset path of the asset to check</param>
        /// <returns>Whether the asset has already been linted</returns>
        private static bool HasAssetBeenLinted(ISet<string> lintedAssets, string assetPath)
        {
            if (lintedAssets.Contains(assetPath))
            {
                return true;
            }

            lintedAssets.Add(assetPath);

            return false;
        }

        /// <summary>
        ///     Warns that an asset's name violates rules
        /// </summary>
        /// <param name="assetPath">The name of the imported asset</param>
        private static void WarnAssetNameViolatesRules(string assetPath)
        {
            Debug.LogWarning(
                $"<color=white><b>{AssetNameUtility.GetAssetNameByPath(assetPath)}</b></color> has naming " +
                "inconsistencies! Suggested name is: " +
                $"<color=white><b>{AssetNameLinter.GetSuggestedNameFor(assetPath)}</b></color>");
        }

        #endregion
    }
}
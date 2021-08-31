using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Ninito.UnityProjectLinter.LintingRules;
using Ninito.UnityProjectLinter.Other;
using Ninito.UnityProjectLinter.Utilities;
using UnityEditor;
using UnityEngine;

namespace Ninito.UnityProjectLinter
{
    /// <summary>
    ///     An asset PostProcessor that ensures all assets are named correctly
    /// </summary>
    public class AssetNameLinter : AssetPostprocessor
    {
        #region AssetPostprocessor Implementation

        public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            AssetLintingSettings settings = GetActiveLintingSettings();

            if (settings == null)
            {
                return;
            }

            var lintedAssets = new HashSet<string>();
            int violatingAssets = 0;

            violatingAssets += LintAssets(settings, importedAssets, ref lintedAssets);
            violatingAssets += LintAssets(settings, movedAssets, ref lintedAssets);

            if (violatingAssets > 1 && settings.WarnOnIncorrect)
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
            return AssetDatabase.FindAssets("", new[] { "Assets/" })
                .Select(AssetDatabase.GUIDToAssetPath).Where(assetPath => assetPath.Contains("."));
        }

        /// <summary>
        ///     Lints a collection of AssetPaths, and outputs how many assets were violating the rules
        /// </summary>
        /// <param name="lintingSettings">The settings to lint the assets with</param>
        /// <param name="assetPaths">The asset paths to lint</param>
        /// <param name="examinedAssets">The hashset of already examined assets</param>
        /// <returns>The number of assets violating the rules</returns>
        public static int LintAssets(AssetLintingSettings lintingSettings, IEnumerable<string> assetPaths,
            ref HashSet<string> examinedAssets)
        {
            int violatingAssets = 0;

            foreach (string movedAssetPath in assetPaths)
            {
                if (HasAssetBeenLinted(examinedAssets, movedAssetPath)) continue;
                if (IsAssetNamedAccordingToRule(lintingSettings, movedAssetPath)) continue;

                if (lintingSettings.WarnOnIncorrect)
                {
                    WarnAssetNameViolatesRules(lintingSettings, movedAssetPath);
                }

                violatingAssets++;
            }

            return violatingAssets;
        }

        /// <summary>
        ///     Logs all violating assets with a rule violation logger
        /// </summary>
        /// <param name="lintingSettings">The settings to lint the assets with</param>
        /// <param name="logger">The logger to log violations with</param>
        public static void LogViolatingAssets(AssetLintingSettings lintingSettings, IRuleViolationLogger logger)
        {
            foreach (string assetPath in GetAllAssetPaths())
            {
                if (IsAssetNamedAccordingToRule(lintingSettings, assetPath)) continue;
                logger.LogViolation(assetPath);
            }
        }

        /// <summary>
        ///     Gets all violating assets
        /// </summary>
        public static IEnumerable<string> GetAllViolatingAssetPaths(AssetLintingSettings lintingSettings)
        {
            return (from path in GetAllAssetPaths()
                where !IsAssetNamedAccordingToRule(lintingSettings, path)
                select path).ToList();
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Gets the active asset linting settings for the project
        /// </summary>
        /// <returns>The currently active asset linting settings</returns>
        private static AssetLintingSettings GetActiveLintingSettings()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(AssetLintingSettings)}");

            var lintingSettingsList = guids.Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<AssetLintingSettings>).ToList();

            int activeLintingSettings = lintingSettingsList.Count(settings => settings.Enabled);

            if (activeLintingSettings <= 1)
            {
                return lintingSettingsList.FirstOrDefault(settings => settings.Enabled);
            }

            Debug.LogError(
                "There are more than one enabled linting settings in your project! Make sure only one is enabled!");
            return null;
        }

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
        /// <param name="lintingSettings">The settings of the failed linted assets</param>
        /// <param name="assetPath">The name of the imported asset</param>
        private static void WarnAssetNameViolatesRules(AssetLintingSettings lintingSettings, string assetPath)
        {
            Debug.LogWarning(
                $"<color=white><b>{AssetNameUtility.GetAssetNameByPath(assetPath)}</b></color> has naming " +
                "inconsistencies! Suggested name is: " +
                $"<color=white><b>{lintingSettings.GetSuggestedNameFor(assetPath)}</b></color>");
        }

        /// <summary>
        ///     Gets whether an asset is named according to the defined rules
        /// </summary>
        /// <param name="lintingSettings">The settings to lint the asset with</param>
        /// <param name="assetPath">The asset path to check for</param>
        /// <returns>Whether the asset is named according to the rules</returns>
        private static bool IsAssetNamedAccordingToRule(AssetLintingSettings lintingSettings, string assetPath)
        {
            if (lintingSettings.AllowSpaces == false && AssetNameUtility.AssetNameContainsSpaces(assetPath))
            {
                return false;
            }

            if (!lintingSettings.IsThereRuleForAsset(assetPath)) return true;

            return DoesAssetHaveExpectedPrefix(lintingSettings, assetPath) &&
                   DoesAssetHaveExpectedSuffix(lintingSettings, assetPath);
        }

        /// <summary>
        ///     Gets whether the given asset name starts with the expected prefix, according to the given linting settings
        /// </summary>
        /// <param name="lintingSettings">The settings to lint with</param>
        /// <param name="assetPath">The path of the asset to lint</param>
        /// <returns>Whether the given asset name starts with the expected prefix, according to the given linting settings</returns>
        private static bool DoesAssetHaveExpectedPrefix(AssetLintingSettings lintingSettings, string assetPath)
        {
            string prefix = lintingSettings.GetPrefixOfAsset(assetPath);
            string assetName = AssetNameUtility.GetAssetNameByPath(assetPath);

            return String.IsNullOrEmpty(prefix) || assetName.StartsWith(prefix);
        }

        /// <summary>
        ///     Gets whether the given asset name ends with the expected suffix, according to the given linting settings
        /// </summary>
        /// <param name="lintingSettings">The settings to lint with</param>
        /// <param name="assetPath">The path of the asset to lint</param>
        /// <returns>Whether the given asset name ends with the expected suffix, according to the given linting settings</returns>
        private static bool DoesAssetHaveExpectedSuffix(AssetLintingSettings lintingSettings, string assetPath)
        {
            string suffix = lintingSettings.GetSuffixOfAsset(assetPath);
            string assetName = AssetNameUtility.GetAssetNameByPath(assetPath);

            return String.IsNullOrEmpty(suffix) || Regex.IsMatch(assetName, $@"([^_]+)({suffix})($|_.*$)");
        }

        #endregion
    }
}
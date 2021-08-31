using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Ninito.UnityProjectLinter.Utilities;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ninito.UnityProjectLinter.LintingRules
{
    [CreateAssetMenu(fileName = CreateAssetMenus.AssetLintingSettingsFileName,
        menuName = CreateAssetMenus.AssetLintingSettingsMenuName, order = CreateAssetMenus.AssetLintingSettingsOrder)]
    public class AssetLintingSettings : ScriptableObject
    {
        #region Private Methods

        [Header("General Settings")]
        [SerializeField]
        private bool enabled = true;

        [SerializeField]
        [Tooltip("Whether to send a warning on the console when an asset is named incorrectly.")]
        private bool warnOnIncorrect = true;

        [Header("Default Rules")]
        [SerializeField]
        [Tooltip("Whether non-ignored assets should have prefixes, generated using the default prefix Regex.")]
        private bool defaultPrefixesEnabled = true;

        [SerializeField]
        [Tooltip(
            "The Regex that generates default prefixes. It will receive the asset's type as an input, and output all captures as the prefix, followed by an underscore. Refer to the preview box below.")]
        private string defaultPrefixRegex = @"[A-Z0-9]";

        [SerializeField]
        [Tooltip("Whether to require the _Variant suffix on variant prefabs.")]
        private bool requireVariantSuffix = true;

        [SerializeField]
        [Tooltip("Whether spaces should be allowed in asset names.")]
        private bool allowSpaces;

        [SerializeField]
        [Tooltip("Whether script assets should be ignored from the linting process.")]
        private bool ignoreScriptAssets = true;

        [SerializeField]
        [Tooltip("Custom naming rules to lint assets with.")]
        private List<NamingRule> namingRules = new List<NamingRule>();

        [SerializeField]
        [Tooltip("The ignored paths (folders). Assets inside ignored folders and their subfolders won't be linted.")]
        private List<string> ignoredPaths = new List<string>();

        [SerializeField]
        [Tooltip("The ignored assets. These assets will not be linted.")]
        private List<Object> ignoredAssets = new List<Object>();

        #endregion

        #region Properties

        public IEnumerable<NamingRule> NamingRules => namingRules;

        public bool WarnOnIncorrect => warnOnIncorrect;

        public bool Enabled => enabled;

        public bool AllowSpaces => allowSpaces;

        public List<string> IgnoredPaths => ignoredPaths;

        public List<Object> IgnoredAssets => ignoredAssets;

        #endregion

        #region Public Methods

        /// <summary>
        ///     Gets the suggested name of an asset
        /// </summary>
        /// <param name="assetPath">The path of the asset to get a suggested name for</param>
        /// <returns>The suggested name for the asset</returns>
        public string GetSuggestedNameFor(string assetPath)
        {
            string assetName = AssetNameUtility.GetAssetNameByPath(assetPath);

            if (!AllowSpaces)
            {
                assetName = assetName.Replace(" ", "");
            }

            assetName = assetName.Replace("_", "");

            if (IsTherePrefixRuleForAsset(assetPath))
            {
                assetName = assetName.Insert(0, GetPrefixOfAsset(assetPath));
            }

            if (IsThereSuffixRuleForAsset(assetPath))
            {
                assetName += GetSuffixOfAsset(assetName);
            }

            return assetName;
        }

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

        /// <summary>
        ///     Whether a prefix rule exists for the asset at the given path
        /// </summary>
        /// <param name="assetPath">The path of the asset to check for</param>
        /// <returns>Whether a prefix rule for the asset at the given path exists</returns>
        public bool IsTherePrefixRuleForAsset(string assetPath)
        {
            if (ignoreScriptAssets && AssetDatabaseUtilities.IsScriptAsset(assetPath))
            {
                return false;
            }

            return NamingRules.Any(rule =>
                       rule.AppliesToAsset(assetPath) && rule.Context == NamingRule.RuleContext.Prefix) ||
                   defaultPrefixesEnabled;
        }

        /// <summary>
        ///     Whether a suffix rule exists for the asset at the given path
        /// </summary>
        /// <param name="assetPath">The path of the asset to check for</param>
        /// <returns>Whether a suffix rule for the asset at the given path exists</returns>
        public bool IsThereSuffixRuleForAsset(string assetPath)
        {
            if (requireVariantSuffix && AssetDatabaseUtilities.IsAssetPrefab(assetPath)) return true;
            return NamingRules.Any(rule =>
                rule.AppliesToAsset(assetPath) && rule.Context == NamingRule.RuleContext.Suffix);
        }

        /// <summary>
        ///     Checks whether a rule for the asset at the given path exists
        /// </summary>
        /// <param name="assetPath">The path of the asset to check for</param>
        /// <returns>Whether a rule for the asset at the given path exists</returns>
        public bool IsThereRuleForAsset(string assetPath)
        {
            if (IsIgnored(assetPath)) return false;
            return IsTherePrefixRuleForAsset(assetPath) || IsThereSuffixRuleForAsset(assetPath);
        }

        /// <summary>
        ///     Gets the prefix of an asset
        /// </summary>
        /// <param name="assetPath">The path of the asset to get the prefix of</param>
        /// <returns>The prefix of the asset at the given path</returns>
        public string GetPrefixOfAsset(string assetPath)
        {
            string customRulePrefix = GetFixForAsset(assetPath, NamingRule.RuleContext.Prefix);

            if (!String.IsNullOrEmpty(customRulePrefix)) return customRulePrefix;

            string assetTypeName = AssetDatabase.GetMainAssetTypeAtPath(assetPath).Name;

            Regex defaultRegex = new Regex(defaultPrefixRegex);
            string defaultPrefix = defaultRegex.GetAllMatchesAsString(assetTypeName);

            return defaultPrefix + "_";
        }

        /// <summary>
        ///     Gets the suffix of an asset
        /// </summary>
        /// <param name="assetPath">The path of the asset to get the suffix of</param>
        /// <returns>The suffix of the asset at the given path</returns>
        public string GetSuffixOfAsset(string assetPath)
        {
            string suffix = String.Empty;

            if (requireVariantSuffix && AssetDatabaseUtilities.IsAssetPrefab(assetPath, out GameObject prefab))
            {
                suffix += PrefabUtility.IsPartOfVariantPrefab(prefab) ? "_Variant" : "";
            }

            string customSuffix = GetFixForAsset(assetPath, NamingRule.RuleContext.Suffix);

            if (customSuffix != null)
            {
                suffix += customSuffix;
            }

            return suffix;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Gets the prefix or suffix for an asset
        /// </summary>
        /// <param name="assetPath">The path for the desired asset</param>
        /// <param name="fixContext">Whether the fix should be a prefix or suffix</param>
        /// <returns>The desired fix for the desired asset</returns>
        private string GetFixForAsset(string assetPath, NamingRule.RuleContext fixContext)
        {
            return NamingRules
                .FirstOrDefault(namingRule =>
                    namingRule.AppliesToAsset(assetPath) && namingRule.Context == fixContext)
                ?.GetFixForAsset(assetPath);
        }

        /// <summary>
        ///     Checks whether an asset path is ignored
        /// </summary>
        /// <param name="assetPath">The asset path to check for</param>
        /// <returns>Whether the asset path is ignored</returns>
        private bool IsIgnored(string assetPath)
        {
            return ignoredPaths.Any(assetPath.Contains) ||
                   IgnoredAssets.Contains(AssetDatabase.LoadAssetAtPath<Object>(assetPath));
        }

        #endregion
    }
}
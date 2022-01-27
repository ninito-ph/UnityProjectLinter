using System;
using System.Linq;
using System.Text.RegularExpressions;
using Ninito.UnityProjectLinter.LintingRules;
using Ninito.UnityProjectLinter.Utilities;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ninito.UnityProjectLinter
{
	/// <summary>
	/// A class that reads and enforces naming rules
	/// </summary>
	public static class AssetNameLinter
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

		#region Public Methods

		/// <summary>
		///     Gets the suggested name of an asset
		/// </summary>
		/// <param name="assetPath">The path of the asset to get a suggested name for</param>
		/// <returns>The suggested name for the asset</returns>
		public static string GetSuggestedNameFor(string assetPath)
		{
			string assetName = AssetNameUtility.GetAssetNameByPath(assetPath).Replace("_", "");

			if (IsThereInfixRuleForAsset(assetPath))
			{
				assetName = GetSuggestedInfixFor(assetPath);
			}

			if (IsTherePrefixRuleForAsset(assetPath))
			{
				assetName = assetName.Insert(0, GetSuggestedPrefixFor(assetPath));
			}

			if (IsThereSuffixRuleForAsset(assetPath))
			{
				assetName += GetSuggestedSuffixFor(assetPath);
			}

			return assetName;
		}

		/// <summary>
		///     Whether a prefix rule exists for the asset at the given path
		/// </summary>
		/// <param name="assetPath">The path of the asset to check for</param>
		/// <returns>Whether a prefix rule for the asset at the given path exists</returns>
		public static bool IsTherePrefixRuleForAsset(string assetPath)
		{
			if (LintingSettings.IgnoreScriptAssets && AssetDatabaseUtilities.IsScriptAsset(assetPath))
			{
				return false;
			}

			return LintingSettings.NamingRules.Where(rule => rule != null).Any(rule =>
				rule.Applies(assetPath) &&
				rule.Context == NamingRule.RuleContext.Prefix);
		}

		/// <summary>
		///     Whether an infix rule exists for the asset at the given path
		/// </summary>
		/// <param name="assetPath">The path of the asset to check for</param>
		/// <returns>Whether an infix rule for the asset at the given path exists</returns>
		public static bool IsThereInfixRuleForAsset(string assetPath)
		{
			if (LintingSettings.IgnoreScriptAssets && AssetDatabaseUtilities.IsScriptAsset(assetPath))
			{
				return false;
			}

			return LintingSettings.NamingRules.Where(rule => rule != null).Any(rule =>
				rule.Applies(assetPath) &&
				rule.Context == NamingRule.RuleContext.Infix);
		}

		/// <summary>
		///     Whether a suffix rule exists for the asset at the given path
		/// </summary>
		/// <param name="assetPath">The path of the asset to check for</param>
		/// <returns>Whether a suffix rule for the asset at the given path exists</returns>
		public static bool IsThereSuffixRuleForAsset(string assetPath)
		{
			if (LintingSettings.IgnoreScriptAssets && AssetDatabaseUtilities.IsScriptAsset(assetPath))
			{
				return false;
			}

			return LintingSettings.NamingRules.Where(rule => rule != null).Any(rule =>
				rule.Applies(assetPath) && rule.Context == NamingRule.RuleContext.Suffix);
		}

		/// <summary>
		///     Checks whether a rule for the asset at the given path exists
		/// </summary>
		/// <param name="assetPath">The path of the asset to check for</param>
		/// <returns>Whether a rule for the asset at the given path exists</returns>
		public static bool IsThereRuleForAsset(string assetPath)
		{
			if (IsIgnored(assetPath)) return false;
			return IsTherePrefixRuleForAsset(assetPath) || IsThereSuffixRuleForAsset(assetPath);
		}

		/// <summary>
		///     Gets the prefix of an asset
		/// </summary>
		/// <param name="assetPath">The path of the asset to get the prefix of</param>
		/// <returns>The prefix of the asset at the given path</returns>
		public static string GetSuggestedPrefixFor(string assetPath)
		{
			string customPrefix = GetFixForAsset(assetPath, NamingRule.RuleContext.Prefix);

			return !String.IsNullOrEmpty(customPrefix) ? customPrefix : String.Empty;
		}

		/// <summary>
		///     Gets the infix of an asset
		/// </summary>
		/// <param name="assetPath">The path of the asset to get the infix of</param>
		/// <returns>The infix of the asset at the given path</returns>
		public static string GetSuggestedInfixFor(string assetPath)
		{
			string customInfix = GetFixForAsset(assetPath, NamingRule.RuleContext.Infix);

			return !String.IsNullOrEmpty(customInfix) ? customInfix : String.Empty;
		}

		/// <summary>
		///     Gets the suffix of an asset
		/// </summary>
		/// <param name="assetPath">The path of the asset to get the suffix of</param>
		/// <returns>The suffix of the asset at the given path</returns>
		public static string GetSuggestedSuffixFor(string assetPath)
		{
			string customSuffix = GetFixForAsset(assetPath, NamingRule.RuleContext.Suffix);

			return String.IsNullOrEmpty(customSuffix) ? String.Empty : customSuffix;
		}

		/// <summary>
		///     Gets whether an asset is named according to the defined rules
		/// </summary>
		/// <param name="assetPath">The asset path to check for</param>
		/// <returns>Whether the asset is named according to the rules</returns>
		public static bool IsAssetNamedAccordingToRule(string assetPath)
		{
			if (!IsThereRuleForAsset(assetPath)) return true;

			return DoesAssetHaveExpectedPrefix(assetPath) &&
			       DoesAssetHaveExpectedInfix(assetPath) &&
			       DoesAssetHaveExpectedSuffix(assetPath);
		}

		#endregion

		#region Private Methods

		/// <summary>
		///     Gets the prefix or suffix for an asset
		/// </summary>
		/// <param name="assetPath">The path for the desired asset</param>
		/// <param name="fixContext">Whether the fix should be a prefix or suffix</param>
		/// <returns>The desired fix for the desired asset</returns>
		private static string GetFixForAsset(string assetPath, NamingRule.RuleContext fixContext)
		{
			NamingRule rule = GetRuleFor(assetPath, fixContext);

			if (rule == null) return String.Empty;

			return fixContext switch
			{
				NamingRule.RuleContext.Prefix => rule.GetPrefix(assetPath),
				NamingRule.RuleContext.Suffix => rule.GetSuffix(assetPath),
				NamingRule.RuleContext.Infix => rule.GetInfix(assetPath),
				_ => throw new ArgumentOutOfRangeException(nameof(fixContext), fixContext,
					"Unknown naming rule context")
			};
		}

		/// <summary>
		/// Gets a rule for the asset at the specified path
		/// </summary>
		/// <param name="assetPath">The path of the asset to get a rule for</param>
		/// <param name="fixContext">The context of the rule</param>
		/// <returns>A rule for the asset with the specified context, if any exists</returns>
		private static NamingRule GetRuleFor(string assetPath, NamingRule.RuleContext fixContext)
		{
			return LintingSettings.NamingRules.OrderByDescending(rule => rule.Priority)
			                      .FirstOrDefault(rule =>
				                      rule != null && rule.Applies(assetPath) && rule.Context == fixContext);
		}

		/// <summary>
		///     Checks whether an asset path is ignored
		/// </summary>
		/// <param name="assetPath">The asset path to check for</param>
		/// <returns>Whether the asset path is ignored</returns>
		private static bool IsIgnored(string assetPath)
		{
			return LintingSettings.IgnoredPaths
			                      .Where(path => !String.IsNullOrEmpty(path) && !String.IsNullOrWhiteSpace(path))
			                      .Any(assetPath.Contains) ||
			       LintingSettings.IgnoredAssets.Where(asset => asset != null)
			                      .Contains(AssetDatabase.LoadAssetAtPath<Object>(assetPath));
		}

		/// <summary>
		///     Gets whether the given asset name starts with the expected prefix, according to the given linting
		///		settings
		/// </summary>
		/// <param name="assetPath">The path of the asset to lint</param>
		/// <returns>Whether the given asset name starts with the expected prefix, according to the given linting
		/// settings</returns>
		private static bool DoesAssetHaveExpectedPrefix(string assetPath)
		{
			string prefix = GetSuggestedPrefixFor(assetPath);
			string assetName = AssetNameUtility.GetAssetNameByPath(assetPath);

			return String.IsNullOrEmpty(prefix) || assetName.StartsWith(prefix);
		}

		/// <summary>
		///     Gets whether the asset's name meets the infix requirements
		/// </summary>
		/// <param name="assetPath">The path of the asset to lint</param>
		/// <returns>Whether the given asset name meets all infix rules</returns>
		private static bool DoesAssetHaveExpectedInfix(string assetPath)
		{
			string infix = GetSuggestedInfixFor(assetPath);

			if (String.IsNullOrEmpty(infix)) return true;

			string assetName = AssetNameUtility.GetAssetNameByPath(assetPath);

			return String.IsNullOrEmpty(infix) || assetName == infix;
		}

		/// <summary>
		///     Gets whether the given asset name ends with the expected suffix, according to the given linting
		/// settings
		/// </summary>
		/// <param name="assetPath">The path of the asset to lint</param>
		/// <returns>Whether the given asset name ends with the expected suffix, according to the given linting
		/// settings</returns>
		private static bool DoesAssetHaveExpectedSuffix(string assetPath)
		{
			string suffix = GetSuggestedSuffixFor(assetPath);
			string assetName = AssetNameUtility.GetAssetNameByPath(assetPath);

			return String.IsNullOrEmpty(suffix) || Regex.IsMatch(assetName, $@"([^_]+)({suffix})($|_.*$)");
		}

		#endregion
	}
}
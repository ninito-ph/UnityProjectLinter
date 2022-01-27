using System;
using Ninito.UnityProjectLinter.Editor.Rules;
using Ninito.UnityProjectLinter.Editor.Samples;
using Ninito.UnityProjectLinter.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace Ninito.UnityProjectLinter.Editor.Samples
{
	[CreateAssetMenu(fileName = CreateAssetMenus.VariantSuffixNamingRuleFileName,
		menuName = CreateAssetMenus.VariantSuffixNamingRuleMenuName, order = CreateAssetMenus.VariantSuffixNamingRuleOrder)]
	public sealed class VariantSuffixNamingRule : NamingRule
	{
		#region Properties

		public override RuleContext Context => RuleContext.Suffix;
		
		#endregion

		#region Public Methods

		public override bool Applies(string assetPath)
		{
			AssetDatabaseUtilities.IsAssetPrefab(assetPath, out GameObject prefab);
			return prefab != null && PrefabUtility.IsPartOfVariantPrefab(prefab);			
		}

		public override string GetSuffix(string assetPath)
		{
			return "_Variant";
		}

		#endregion
	}
}
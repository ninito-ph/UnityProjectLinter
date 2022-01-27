using System.Text.RegularExpressions;
using Ninito.UnityProjectLinter.Editor.Extensions;
using Ninito.UnityProjectLinter.Editor.Rules;
using UnityEditor;
using UnityEngine;

namespace Ninito.UnityProjectLinter.Editor.Samples
{
	/// <summary>
	/// A prefix naming rule that generates a prefix based on a regex match
	/// </summary>
	[CreateAssetMenu(fileName = CreateAssetMenus.RegexNamingRuleFileName,
		menuName = CreateAssetMenus.RegexNamingRuleMenuName, order = CreateAssetMenus.RegexNamingRuleOrder)]
	public sealed class RegexNamingRule : NamingRule
	{
		#region Fields

		[SerializeField]
		private RuleContext context = RuleContext.Prefix;
		
		[Header("Regex")]
		[SerializeField]
		private string regexPattern = @"[A-Z]";

		#endregion
		
		#region Public Methods

		public override RuleContext Context => context;

		public override bool Applies(string assetPath)
		{
			return true;
		}

		public override string GetPrefix(string assetPath)
		{
			string assetTypeName = AssetDatabase.GetMainAssetTypeAtPath(assetPath)?.Name;
			Regex regex = new Regex(regexPattern);
			string prefix = regex.GetAllMatchesAsString(assetTypeName);

			return prefix + "_";
		}
		
		public override string GetSuffix(string assetPath)
		{
			string assetTypeName = AssetDatabase.GetMainAssetTypeAtPath(assetPath)?.Name;
			Regex regex = new Regex(regexPattern);
			string suffix = regex.GetAllMatchesAsString(assetTypeName);

			return "_" + suffix;
		}

		#endregion
	}
}
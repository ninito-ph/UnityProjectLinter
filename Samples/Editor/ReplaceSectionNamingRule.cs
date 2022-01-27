using Ninito.UnityProjectLinter.Editor.Rules;
using Ninito.UnityProjectLinter.Editor.Utilities;
using UnityEngine;

namespace Ninito.UnityProjectLinter.Editor.Samples
{
	[CreateAssetMenu(fileName = CreateAssetMenus.ReplaceSectionNamingRuleFileName,
		menuName = CreateAssetMenus.ReplaceSectionNamingRuleMenuName, order = CreateAssetMenus.ReplaceSectionNamingRuleOrder)]
	public sealed class ReplaceSectionNamingRule : NamingRule
	{
		#region Fields

		[Header("Replacement")]
		[SerializeField]
		private string replace = " ";
		[SerializeField]
		private string replacement = "";

		#endregion
		
		#region Properties

		public override RuleContext Context => RuleContext.Infix;
		
		#endregion

		#region Public Methods

		public override bool Applies(string assetPath)
		{
			return true;
		}

		public override string GetInfix(string assetPath)
		{
			return AssetNameUtility.GetAssetNameByPath(assetPath).Replace(replace, replacement);
		}

		#endregion
	}
}
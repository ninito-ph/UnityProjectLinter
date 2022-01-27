using System;
using Ninito.UnityProjectLinter.Editor.Rules;
using UnityEditor;
using UnityEngine;

namespace Ninito.UnityProjectLinter.Editor.Samples
{
	[CreateAssetMenu(fileName = CreateAssetMenus.PrefixNamingRuleFileName,
		menuName = CreateAssetMenus.PrefixNamingRuleMenuName, order = CreateAssetMenus.PrefixNamingRuleOrder)]
	public sealed class SimplePrefixNamingRule : NamingRule
	{
		#region Fields

		[Header("Prefix")]
		[SerializeField]
		[Tooltip("The name of the type to define the prefix for")]
		private string typeName = String.Empty;

		private int _typeNameHash;

		[field: SerializeField]
		[field: Tooltip("The prefix for the defined type. Should not include an underscore (Will be added automatically).")]
		private string Prefix { get; set; } = String.Empty;

		#endregion

		#region Properties

		private int TypeNameHash
		{
			get
			{
				if (_typeNameHash == default)
				{
					_typeNameHash = typeName.GetHashCode();
				}

				return _typeNameHash;
			}
		}

		public override RuleContext Context => RuleContext.Prefix;
		
		#endregion

		#region Public Methods

		public override bool Applies(string assetPath)
		{
			return TypeMatches(AssetDatabase.GetMainAssetTypeAtPath(assetPath).Name);
		}

		public override string GetPrefix(string assetPath)
		{
			return Prefix + "_";
		}

		#endregion

		#region Private Methods

		private bool TypeMatches(string otherTypeName)
		{
			return otherTypeName.GetHashCode() == TypeNameHash;
		}

		#endregion
	}
}
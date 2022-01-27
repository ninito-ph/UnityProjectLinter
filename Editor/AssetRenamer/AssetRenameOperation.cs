using Ninito.UnityProjectLinter.Editor.Linter;
using Ninito.UnityProjectLinter.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace Ninito.UnityProjectLinter.Editor.AssetRenamer
{
	/// <summary>
	/// A struct that describes the operation of renaming a single asset.
	/// </summary>
	public struct AssetRenameOperation
	{
		#region Fields

		public Object Asset;
		public string NewName;
		public string SuggestedName;

		#endregion

		#region Properties

		public string CurrentName => Asset.name;

		#endregion

		#region Constructors

		public AssetRenameOperation(string assetPath)
		{
			Asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
			NewName = AssetNameUtility.GetAssetNameByPath(assetPath);
			SuggestedName = AssetNameLinter.GetSuggestedNameFor(assetPath);
		}

		#endregion
	}
}
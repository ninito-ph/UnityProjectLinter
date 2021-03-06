using System;
using System.IO;
using Ninito.UnityProjectLinter.Editor.Settings;
using Ninito.UnityProjectLinter.Editor.Utilities;
using Ninito.Whetstone.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Ninito.UnityProjectLinter.Editor.AssetRenamer
{
	/// <summary>
	///     A window that mass-renames assets
	/// </summary>
	public sealed class AssetRenamerWindow : AutoEditorWindow
	{
		#region Private Fields

		private AssetRenameOperation[] _renameOperations;

		private bool[] _shouldRenameAsset;
		private Vector2 _scrollPosition;
		private int _currentPage;
		private const int _pageLenght = 23;

		#endregion

		#region Properties

		public AssetLintingSettings LintingSettings { get; set; }
		public string[] AssetPathsToRename { get; set; }

		#endregion

		#region Unity Callbacks

		private void OnEnable()
		{
			_scrollPosition = Vector2.zero;
		}

		private void OnGUI()
		{
			EditorGUILayout.Space(10f);

			_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandWidth(true));
			EditorGUILayout.BeginHorizontal();

			DrawAssetRow();
			DrawNewAssetNameRow();
			DrawSuggestedAssetNameRow();

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndScrollView();

			DrawPageControls();
			DrawButtons();
		}

		#endregion

		#region Public Methods

		/// <summary>
		///     Creates a new asset renamer window
		/// </summary>
		/// <param name="assetPathsToRename">The asset paths of the assets to rename</param>
		/// <param name="lintingSettings">The linting settings to rename with</param>
		public static void CreateRenamerWindow(string[] assetPathsToRename, AssetLintingSettings lintingSettings)
		{
			AssetRenamerWindow window = GetWindow<AssetRenamerWindow>();
			window.LintingSettings = lintingSettings;
			window.AssetPathsToRename = assetPathsToRename;
			window.titleContent = new GUIContent("Asset Renamer");

			window.InitializeAssetRenamer();

			window.Show();
		}

		#endregion

		#region Private Methods

		/// <summary>
		///     Draws the pagination controls
		/// </summary>
		private void DrawPageControls()
		{
			EditorGUILayout.BeginHorizontal();

			EditorGUI.BeginDisabledGroup(_currentPage <= 0);

			if (GUILayout.Button("Previous Page"))
			{
				GUI.FocusControl(null);
				_currentPage--;
			}

			EditorGUI.EndDisabledGroup();

			GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
			centeredStyle.alignment = TextAnchor.UpperCenter;
			GUILayout.Label($"Page {_currentPage + 1} of {GetPageCount() + 1}", centeredStyle);

			EditorGUI.BeginDisabledGroup(_currentPage >= GetPageCount());

			if (GUILayout.Button("Next Page"))
			{
				GUI.FocusControl(null);
				_currentPage++;
			}

			EditorGUI.EndDisabledGroup();

			EditorGUILayout.EndHorizontal();
		}

		/// <summary>
		///     Gets the total page count of the assets to be renamed
		/// </summary>
		/// <returns>The total page count of the assets to be renamed</returns>
		private int GetPageCount()
		{
			return (int)Mathf.Ceil(AssetPathsToRename.Length / (float)_pageLenght) - 1;
		}

		/// <summary>
		///     Initializes the asset renamer's arrays
		/// </summary>
		private void InitializeAssetRenamer()
		{
			_renameOperations = new AssetRenameOperation[AssetPathsToRename.Length];

			for (int index = 0; index < AssetPathsToRename.Length; index++)
			{
				_renameOperations[index] = new AssetRenameOperation(AssetPathsToRename[index]);
			}
		}

		/// <summary>
		///     Draws the asset renamer window's buttons
		/// </summary>
		private void DrawButtons()
		{
			EditorGUILayout.Space(20f);

			EditorGUILayout.BeginHorizontal();

			DrawCancelButton();
			DrawSetToSuggestedNamesButton();
			DrawRenameButton();

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space(10f);
		}

		/// <summary>
		///     Draws the cancel rename button
		/// </summary>
		private static void DrawCancelButton()
		{
			if (!GUILayout.Button("Cancel")) return;
			GetWindow<AssetRenamerWindow>().Close();
		}

		/// <summary>
		///     Draws the cancel rename button
		/// </summary>
		private void DrawSetToSuggestedNamesButton()
		{
			if (!GUILayout.Button("Set Names to Suggested")) return;
			SetNewNamesToSuggestedNames();
		}

		/// <summary>
		///     Draws the rename asset button
		/// </summary>
		private void DrawRenameButton()
		{
			if (!GUILayout.Button("Rename All Assets")) return;
			RenameAssets();
		}

		/// <summary>
		///     Renames assets to their new names, if they are marked for renaming
		/// </summary>
		private void RenameAssets()
		{
			int renamedAssets = 0;
			int failedToRenameAssets = 0;
			int skippedAssets = 0;

			for (int index = 0; index < AssetPathsToRename.Length; index++)
			{
				EditorUtility.DisplayProgressBar("Renaming Assets",
					$"Renaming {_renameOperations[index].CurrentName} to {_renameOperations[index].NewName}...",
					AssetPathsToRename.Length / (float)index);

				if (!IsRenameValid(index) || !IsRenameSafe(index))
				{
					skippedAssets++;
					continue;
				}

				string renameSuccess =
					AssetDatabase.RenameAsset(AssetPathsToRename[index], _renameOperations[index].NewName);

				if (String.Empty == renameSuccess)
				{
					renamedAssets++;
				}
				else
				{
					failedToRenameAssets++;
				}
			}

			EditorUtility.ClearProgressBar();
			GetWindow<AssetRenamerWindow>().Close();
			Debug.Log(
				$"Renamed <b>{renamedAssets} asset(s)</b>, skipped <b>{skippedAssets} asset(s)</b>, and failed to rename <b>{failedToRenameAssets} asset(s).</b>");
		}

		/// <summary>
		///     Checks whether an asset rename is valid
		/// </summary>
		/// <param name="index">The index of the asset to rename</param>
		/// <returns>Whether the rename is valid</returns>
		private bool IsRenameValid(int index)
		{
			return !String.IsNullOrEmpty(_renameOperations[index].NewName) &&
			       !String.IsNullOrEmpty(AssetPathsToRename[index]) &&
			       IsNewNameDifferent(index);
		}

		/// <summary>
		///     Checks whether a file name is valid
		/// </summary>
		/// <param name="filename">The filename to check</param>
		/// <returns>Whether the filename is valid</returns>
		private static bool IsValidFilename(string filename)
		{
			if (filename == null) return false;
			return filename.IndexOfAny(Path.GetInvalidFileNameChars()) == -1;
		}

		/// <summary>
		///     Checks whether the rename is safe
		/// </summary>
		/// <param name="index">The index of the asset to rename</param>
		/// <returns>Whether it is safe to rename the asset at the index</returns>
		private bool IsRenameSafe(int index)
		{
			return AssetNameUtility.GetAssetNameByPath(AssetPathsToRename[index]) != SceneManager.GetActiveScene().name;
		}

		/// <summary>
		///     Checks whether the new name of the asset is different from the current name
		/// </summary>
		/// <param name="index">The index of the asset to check for</param>
		/// <returns>Whether the new name of the asset is different from the current name</returns>
		private bool IsNewNameDifferent(int index)
		{
			return _renameOperations[index].NewName != _renameOperations[index].CurrentName;
		}

		/// <summary>
		///     Draws the new asset name row of the renamer window
		/// </summary>
		private void DrawNewAssetNameRow()
		{
			EditorGUILayout.BeginVertical();

			EditorGUILayout.LabelField("New Name", EditorStyles.boldLabel);

			OnCurrentPageIndexes((newName, suggestedName, asset, index)
				=>
			{
				if (!IsValidFilename(newName))
				{
					ChangeGUIColor(EditorColors.ErrorRed);
				}
				else if (!IsNewNameDifferent(index))
				{
					ChangeGUIColor(new Color(0.75f, 0.75f, 0.75f));
				}

				if (IsRenameSafe(index))
				{
					_renameOperations[index].NewName = EditorGUILayout.TextField(newName);
				}
				else
				{
					EditorGUILayout.LabelField("Asset cannot be renamed currently.");
				}

				RestoreGUIColor();
			});

			EditorGUILayout.EndVertical();
		}

		/// <summary>
		///     Draws the suggested asset name row for the renamer window
		/// </summary>
		private void DrawSuggestedAssetNameRow()
		{
			EditorGUILayout.BeginVertical();

			EditorGUILayout.LabelField("Suggested Name", EditorStyles.boldLabel);

			EditorGUI.BeginDisabledGroup(true);

			OnCurrentPageIndexes((newName, suggestedName, asset, index)
				=>
			{
				EditorGUILayout.TextField(suggestedName);
			});

			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndVertical();
		}

		/// <summary>
		///     Draws the asset display row of the renamer window
		/// </summary>
		private void DrawAssetRow()
		{
			EditorGUILayout.BeginVertical();

			EditorGUILayout.LabelField("Asset", EditorStyles.boldLabel);

			EditorGUI.BeginDisabledGroup(true);

			OnCurrentPageIndexes((newName, suggestedName, asset, index)
				=>
			{
				if (!IsRenameSafe(index))
				{
					ChangeGUIColor(EditorColors.ErrorRed);
				}

				EditorGUILayout.ObjectField(asset, typeof(Object), false);

				RestoreGUIColor();
			});


			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndVertical();
		}

		/// <summary>
		///     Sets all new names to suggested names
		/// </summary>
		private void SetNewNamesToSuggestedNames()
		{
			for (int index = 0; index < _renameOperations.Length; index++)
			{
				_renameOperations[index].NewName = _renameOperations[index].SuggestedName;
			}
		}

		/// <summary>
		///     Performs an action of the currently displayed indexes of the current page
		/// </summary>
		/// <param name="newNameSuggestedNameAsset">The action to perform upon the current index</param>
		private void OnCurrentPageIndexes(Action<string, string, Object, int> newNameSuggestedNameAsset)
		{
			int startIndex = Mathf.Max(_pageLenght * _currentPage - 1, 0);
			int endIndex = Mathf.Min(startIndex + _pageLenght, AssetPathsToRename.Length - 1);

			for (int index = startIndex; index < endIndex; index++)
			{
				newNameSuggestedNameAsset(_renameOperations[index].NewName, _renameOperations[index].SuggestedName,
					_renameOperations[index].Asset,
					index);
			}
		}

		#endregion
	}
}
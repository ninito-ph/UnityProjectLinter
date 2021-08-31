using System;
using Ninito.UnityProjectLinter.Editor;
using Ninito.UnityProjectLinter.LintingRules;
using Ninito.UnityProjectLinter.Utilities;
using Ninito.Whetstone.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Ninito.UnityProjectLinter
{
    /// <summary>
    ///     A window that mass-renames assets
    /// </summary>
    public class AssetRenamerWindow : AutoEditorWindow
    {
        #region Private Fields

        private Object[] _assetsToBeRenamed;
        private string[] _newAssetNames;
        private string[] _suggestedAssetNames;
        private bool[] _shouldRenameAsset;
        private Vector2 _scrollPosition;

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
        ///     Initializes the asset renamer's arrays
        /// </summary>
        private void InitializeAssetRenamer()
        {
            _newAssetNames = new string[AssetPathsToRename.Length];
            _suggestedAssetNames = new string[AssetPathsToRename.Length];
            _assetsToBeRenamed = new Object[AssetPathsToRename.Length];

            for (int index = 0; index < AssetPathsToRename.Length; index++)
            {
                _assetsToBeRenamed[index] = AssetDatabase.LoadAssetAtPath<Object>(AssetPathsToRename[index]);
                _newAssetNames[index] = AssetNameUtility.GetAssetNameByPath(AssetPathsToRename[index]);
                _suggestedAssetNames[index] = LintingSettings.GetSuggestedNameFor(AssetPathsToRename[index]);
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
                if (!IsRenameValid(index) || !IsRenameSafe(index))
                {
                    skippedAssets++;
                    continue;
                }

                string renameSuccess = AssetDatabase.RenameAsset(AssetPathsToRename[index], _newAssetNames[index]);

                if (String.Empty == renameSuccess)
                {
                    renamedAssets++;
                }
                else
                {
                    failedToRenameAssets++;
                }
            }

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
            return !String.IsNullOrEmpty(_newAssetNames[index]) && !String.IsNullOrEmpty(AssetPathsToRename[index]) &&
                   IsNewNameDifferent(index);
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
            return _newAssetNames[index] != AssetNameUtility.GetAssetNameByPath(AssetPathsToRename[index]);
        }

        /// <summary>
        ///     Draws the new asset name row of the renamer window
        /// </summary>
        private void DrawNewAssetNameRow()
        {
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("New Name", EditorStyles.boldLabel);

            for (int index = 0; index < _newAssetNames.Length; index++)
            {
                if (!IsNewNameDifferent(index))
                {
                    ChangeGUIColor(new Color(0.75f, 0.75f, 0.75f));
                }

                if (IsRenameSafe(index))
                {
                    _newAssetNames[index] = EditorGUILayout.TextField(_newAssetNames[index]);
                }
                else
                {
                    EditorGUILayout.LabelField("Asset cannot be renamed currently.");
                }

                RestoreGUIColor();
            }

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

            for (int index = 0; index < AssetPathsToRename.Length; index++)
            {
                EditorGUILayout.TextField(_suggestedAssetNames[index]);
            }

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

            for (int index = 0; index < AssetPathsToRename.Length; index++)
            {
                if (!IsRenameSafe(index))
                {
                    ChangeGUIColor(EditorColors.ErrorRed);
                }

                EditorGUILayout.ObjectField(_assetsToBeRenamed[index], typeof(Object), false);

                RestoreGUIColor();
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        ///     Sets all new names to suggested names
        /// </summary>
        private void SetNewNamesToSuggestedNames()
        {
            for (int index = 0; index < _newAssetNames.Length; index++)
            {
                _newAssetNames[index] = _suggestedAssetNames[index];
            }
        }

        #endregion
    }
}
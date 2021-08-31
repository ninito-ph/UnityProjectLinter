using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Ninito.UnityProjectLinter.Editor;
using Ninito.UnityProjectLinter.LintingRules;
using Ninito.UnityProjectLinter.Other;
using Ninito.UnityProjectLinter.Utilities;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ninito.UnityProjectLinter
{
    /// <summary>
    ///     A custom inspector for Asset Linting Settings
    /// </summary>
    [CustomEditor(typeof(AssetLintingSettings))]
    public class AssetLintingSettingsEditor : AutoEditor
    {
        #region Private Fields

        private string _defaultRegexPrefix;
        private string _defaultRegexInput = "GameObject";
        private bool _regexIsInvalid;

        private bool _hasPrefix;
        private bool _hasVariantSuffix;

        private AssetLintingEditorTabs _currentTab = AssetLintingEditorTabs.GeneralSettings;
        private AssetLintingEditorTabs _previousTab;

        #endregion

        #region Unity Callbacks

        private void OnEnable()
        {
            UpdateRegexPreview();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            DrawTabs();

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            HandleFocusControl();
        }

        #endregion

        #region Private Methods
        
        /// <summary>
        ///     Wipes the focus' control on a tab change
        /// </summary>
        private void HandleFocusControl()
        {
            if (HasTabChanged())
            {
                GUI.FocusControl(null);
            }

            _previousTab = _currentTab;
        }

        /// <summary>
        ///     Checks whether the inspector's tab has changed
        /// </summary>
        /// <returns>Whether the inspector's tab has changed</returns>
        private bool HasTabChanged()
        {
            return _previousTab != _currentTab;
        }

        /// <summary>
        ///     Draws the inspector's tabs
        /// </summary>
        private void DrawTabs()
        {
            _currentTab = (AssetLintingEditorTabs)GUILayout.Toolbar((int)_currentTab,
                new[] { "General Settings", "Custom Naming Rules", "Ignored Assets & Paths" });

            DrawCurrentTab();
        }

        /// <summary>
        ///     Draws the inspector's current tab
        /// </summary>
        private void DrawCurrentTab()
        {
            EditorGUILayout.Space(20f);

            switch (_currentTab)
            {
                case AssetLintingEditorTabs.GeneralSettings:
                    DrawGeneralTab();
                    break;
                case AssetLintingEditorTabs.CustomNamingRules:
                    DrawNamingRulesTab();
                    break;
                case AssetLintingEditorTabs.IgnoredAssetsAndPaths:
                    DrawIgnoreTab();
                    break;
                default:
                    _currentTab = AssetLintingEditorTabs.GeneralSettings;
                    break;
            }
        }

        /// <summary>
        ///     Draws the general tab
        /// </summary>
        private void DrawGeneralTab()
        {
            DrawGeneralSettings();
            DrawDefaultRules();
            
            EditorGUILayout.Space(20f);
            DrawCheckAllAssetsButton();
            DrawAssetRenamerButton();
            DrawExportViolatingAssetsLogButton();
        }

        /// <summary>
        ///     Draws the ignore tab
        /// </summary>
        private void DrawIgnoreTab()
        {
            DrawSerializedProperty("ignoredPaths");

            EditorGUILayout.Space(20f);
            DrawSerializedProperty("ignoredAssets");

            EditorGUILayout.Space(20f);
            DrawIgnoreButtons();
        }

        /// <summary>
        ///     Draws the naming rules tab
        /// </summary>
        private void DrawNamingRulesTab()
        {
            DrawSerializedProperty("namingRules");
        }

        /// <summary>
        ///     Draws the ignore buttons
        /// </summary>
        private void DrawIgnoreButtons()
        {
            DrawIgnorePreview();

            GUILayout.BeginHorizontal();
            DrawIgnoreAssetButton();
            DrawIgnoreFolderButton();
            GUILayout.EndHorizontal();
        }

        /// <summary>
        ///     Draws the asset ignore previews
        /// </summary>
        private static void DrawIgnorePreview()
        {
            GUI.enabled = false;

            GUILayout.BeginHorizontal();

            EditorGUILayout.ObjectField(Selection.activeObject, typeof(Object), false);
            EditorGUILayout.TextField(
                AssetDatabaseUtilities.GetFolderOfAsset(AssetDatabase.GetAssetPath(Selection.activeObject)));

            GUILayout.EndHorizontal();

            GUI.enabled = true;
        }

        /// <summary>
        ///     Draws the ignore asset button
        /// </summary>
        private void DrawIgnoreAssetButton()
        {
            EditorGUI.BeginDisabledGroup(Selection.activeObject == null ||
                                         Selection.activeObject as DefaultAsset != null);

            if (GUILayout.Button("Ignore Asset"))
            {
                AssetLintingSettings settings = target as AssetLintingSettings;
                if (settings == null) return;
                settings.IgnoreAsset(AssetDatabase.GetAssetPath(Selection.activeObject));
            }

            EditorGUI.EndDisabledGroup();

            Repaint();
        }

        /// <summary>
        ///     Draws the ignore whole folder button
        /// </summary>
        private void DrawIgnoreFolderButton()
        {
            if (!GUILayout.Button("Ignore Whole Folder")) return;
            AssetLintingSettings settings = target as AssetLintingSettings;

            if (settings == null) return;

            string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            settings.IgnorePath(AssetDatabaseUtilities.GetFolderOfAsset(assetPath));

            Repaint();
        }

        /// <summary>
        ///     Draws the check all asset names button
        /// </summary>
        private void DrawCheckAllAssetsButton()
        {
            if (!GUILayout.Button("Check All Assets")) return;
            var lintedAssets = new HashSet<string>();
            AssetNameLinter.LintAssets(target as AssetLintingSettings, AssetNameLinter.GetAllAssetPaths(),
                ref lintedAssets);
        }

        /// <summary>
        ///     Draws the asset renamer button
        /// </summary>
        private void DrawAssetRenamerButton()
        {
            if (!GUILayout.Button("Open Asset Renamer")) return;
            AssetLintingSettings settings = target as AssetLintingSettings; 
            AssetRenamerWindow.CreateRenamerWindow(AssetNameLinter.GetAllViolatingAssetPaths(settings).ToArray(), settings);
        }

        /// <summary>
        ///     Draws the export violating assets log button
        /// </summary>
        private void DrawExportViolatingAssetsLogButton()
        {
            if (!GUILayout.Button("Export Violating Assets Log")) return;

            IRuleViolationLogger logger = new ViolatingAssetLogger();
            AssetNameLinter.LogViolatingAssets(target as AssetLintingSettings, logger);

            logger.GenerateLog();
        }

        /// <summary>
        ///     Draws the default rules
        /// </summary>
        private void DrawDefaultRules()
        {
            _hasPrefix = DrawSerializedProperty("defaultPrefixesEnabled").boolValue;

            if (_regexIsInvalid)
            {
                ChangeGUIColor(EditorColors.ErrorRed);
            }

            if (_hasPrefix)
            {
                DrawSerializedProperty("defaultPrefixRegex");
            }

            RestoreGUIColor();

            _hasVariantSuffix = DrawSerializedProperty("requireVariantSuffix").boolValue;

            if (_hasPrefix || _hasVariantSuffix)
            {
                UpdateRegexPreview();
                DrawAssetNamePreview();
            }

            EditorGUILayout.Space();
            DrawSerializedProperty("ignoreScriptAssets");
        }

        /// <summary>
        ///     Draws the asset name preview
        /// </summary>
        private void DrawAssetNamePreview()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("Preview Input");

            _defaultRegexInput = EditorGUILayout.TextField(_defaultRegexInput);

            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("Preview");

            EditorGUI.BeginDisabledGroup(true);

            EditorGUILayout.TextField(GetPreviewAssetName());

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        ///     Gets the preview asset name
        /// </summary>
        /// <returns>The preview asset name</returns>
        private string GetPreviewAssetName()
        {
            string assetName = "AssetName";

            if (_hasPrefix)
            {
                assetName = assetName.Insert(0, _defaultRegexPrefix + "_");
            }

            if (_hasVariantSuffix)
            {
                assetName += "_Variant";
            }

            return assetName;
        }

        /// <summary>
        ///     Updates the regex preview
        /// </summary>
        private void UpdateRegexPreview()
        {
            string defaultRegexPattern = serializedObject.FindProperty("defaultPrefixRegex").stringValue;

            // Unfortunately, .NET Regex does not support the necessary features to make a Regex-verifying Regex
            try
            {
                Regex defaultRegex = new Regex(defaultRegexPattern);
                _defaultRegexPrefix = defaultRegex.GetAllMatchesAsString(_defaultRegexInput);
                _regexIsInvalid = false;
            }
            catch
            {
                // ignored
                _defaultRegexPrefix = _defaultRegexInput;
                _regexIsInvalid = true;
                EditorGUILayout.HelpBox("Regex is invalid!", MessageType.Error);
            }
        }

        /// <summary>
        ///     Draws the general settings tab of the inspector
        /// </summary>
        private void DrawGeneralSettings()
        {
            bool multipleSettingsEnabled = AreThereMultipleEnabledSettings();

            if (multipleSettingsEnabled)
            {
                ChangeGUIColor(EditorColors.ErrorRed);
            }

            DrawSerializedProperty("enabled");

            RestoreGUIColor();

            if (multipleSettingsEnabled)
            {
                EditorGUILayout.HelpBox(
                    "Multiple linting settings are enabled. Make sure only one linting setting is active at any time!",
                    MessageType.Warning);
            }

            DrawSerializedProperty("warnOnIncorrect");
        }

        /// <summary>
        ///     Gets the active asset linting settings for the project
        /// </summary>
        /// <returns>The currently active asset linting settings</returns>
        private static bool AreThereMultipleEnabledSettings()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(AssetLintingSettings)}");

            return guids
                .Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<AssetLintingSettings>)
                .Count(settings => settings.Enabled) > 1;
        }

        #endregion

        #region Nested Enums

        /// <summary>
        ///     An enum of this editor's tabs
        /// </summary>
        private enum AssetLintingEditorTabs
        {
            GeneralSettings = 0,
            CustomNamingRules = 1,
            IgnoredAssetsAndPaths = 2
        }

        #endregion
    }
}
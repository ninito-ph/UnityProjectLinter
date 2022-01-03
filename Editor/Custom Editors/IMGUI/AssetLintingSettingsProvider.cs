using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Ninito.UnityProjectLinter.LintingRules;
using Ninito.UnityProjectLinter.Other;
using Ninito.UnityProjectLinter.Utilities;
using UnityEditor;
using UnityEngine;

namespace Ninito.UnityProjectLinter.Editor.Settings
{
    /// <summary>
    /// An IMGUI settings provider for the Unity Project Linter plugin.
    /// </summary>
    public static class AssetLintingSettingsProvider
    {
        private static string _defaultRegexPrefix;
        private static string _defaultRegexInput = "GameObject";
        private static bool _regexIsInvalid;

        private static bool _hasPrefix;
        private static bool _hasVariantSuffix;

        private static readonly SerializedObject _settings = AssetLintingSettings.GetOrCreateSerializedSettings();
        private static readonly Stack<Color> _guiColorStack = new Stack<Color>();

        private static readonly GUIStyle _headerStyle = new GUIStyle
        {
            fontStyle = FontStyle.Bold,
            fontSize = 18,
            normal =
            {
                textColor = EditorColors.FontWhite
            }
        };

        [SettingsProvider]
        public static SettingsProvider Register()
        {
            SettingsProvider provider = new SettingsProvider("Project/Unity Project Linter", SettingsScope.Project)
            {
                label = "Unity Project Linter",
                guiHandler = (searchContext) =>
                {
                    DrawGeneralSection();
                    DrawIgnoreSection();
                    DrawNamingRulesSection();
                    _settings.ApplyModifiedProperties();
                },
                keywords = new HashSet<string>(new[] { "Ninito", "Asset Linting", "Rule", "Linting", "Name", "Naming" })
            };

            return provider;
        }

        #region Private Methods

        /// <summary>
        ///     Draws the general tab
        /// </summary>
        private static void DrawGeneralSection()
        {
            ShowHeader("General");
            DrawGeneralSettings();
            DrawDefaultRules();

            EditorGUILayout.Space(20f);
            DrawCheckAllAssetsButton();
            DrawAssetRenamerButton();
            DrawExportViolatingAssetsLogButton();
            EditorGUILayout.Space(30f);
        }

        /// <summary>
        ///     Draws the ignore tab
        /// </summary>
        private static void DrawIgnoreSection()
        {
            ShowHeader("Ignored Assets");
            DrawSerializedProperty("ignoredPaths");

            EditorGUILayout.Space(20f);
            DrawSerializedProperty("ignoredAssets");

            EditorGUILayout.Space(20f);
            DrawIgnoreButtons();
            EditorGUILayout.Space(30f);
        }

        /// <summary>
        ///     Draws the naming rules tab
        /// </summary>
        private static void DrawNamingRulesSection()
        {
            ShowHeader("Linting Rules");
            DrawSerializedProperty("namingRules");
        }

        /// <summary>
        ///     Draws the ignore buttons
        /// </summary>
        private static void DrawIgnoreButtons()
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
        private static void DrawIgnoreAssetButton()
        {
            EditorGUI.BeginDisabledGroup(Selection.activeObject == null ||
                                         Selection.activeObject as DefaultAsset != null);

            if (GUILayout.Button("Ignore Asset"))
            {
                AssetLintingSettings settings = AssetLintingSettings.GetOrCreateSettings();
                settings.IgnoreAsset(AssetDatabase.GetAssetPath(Selection.activeObject));
            }

            EditorGUI.EndDisabledGroup();
        }

        /// <summary>
        ///     Draws the ignore whole folder button
        /// </summary>
        private static void DrawIgnoreFolderButton()
        {
            if (!GUILayout.Button("Ignore Whole Folder")) return;
            AssetLintingSettings settings = AssetLintingSettings.GetOrCreateSettings();

            if (settings == null) return;

            string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            settings.IgnorePath(AssetDatabaseUtilities.GetFolderOfAsset(assetPath));
        }

        /// <summary>
        ///     Draws the check all asset names button
        /// </summary>
        private static void DrawCheckAllAssetsButton()
        {
            if (!GUILayout.Button("Check All Assets")) return;
            HashSet<string> lintedAssets = new HashSet<string>();
            AssetNameLinter.LintAssets(AssetLintingSettings.GetOrCreateSettings(), AssetNameLinter.GetAllAssetPaths(),
                ref lintedAssets);
        }

        /// <summary>
        ///     Draws the asset renamer button
        /// </summary>
        private static void DrawAssetRenamerButton()
        {
            if (!GUILayout.Button("Open Asset Renamer")) return;
            AssetLintingSettings settings = AssetLintingSettings.GetOrCreateSettings();
            AssetRenamerWindow.CreateRenamerWindow(AssetNameLinter.GetAllViolatingAssetPaths(settings).ToArray(),
                settings);
        }

        /// <summary>
        ///     Draws the export violating assets log button
        /// </summary>
        private static void DrawExportViolatingAssetsLogButton()
        {
            if (!GUILayout.Button("Export Violating Assets Log")) return;

            IRuleViolationLogger logger = new ViolatingAssetLogger();
            AssetNameLinter.LogViolatingAssets(AssetLintingSettings.GetOrCreateSettings(), logger);

            logger.GenerateLog();
        }

        /// <summary>
        ///     Draws the default rules
        /// </summary>
        private static void DrawDefaultRules()
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
        private static void DrawAssetNamePreview()
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
        private static string GetPreviewAssetName()
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
        private static void UpdateRegexPreview()
        {
            string defaultRegexPattern = _settings.FindProperty("defaultPrefixRegex").stringValue;

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
        private static void DrawGeneralSettings()
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

        /// <summary>
        /// Draws a serialized property
        /// </summary>
        /// <param name="propertyName">The name of the property to draw</param>
        /// <returns>The property being drawn</returns>
        private static SerializedProperty DrawSerializedProperty(string propertyName)
        {
            SerializedProperty drawnProperty = _settings.FindProperty(propertyName);
            EditorGUILayout.PropertyField(drawnProperty);
            return drawnProperty;
        }
        
        /// <summary>
        /// Shows a header for a section of the inspector
        /// </summary>
        /// <param name="headerContent">The header's label content</param>
        private static void ShowHeader(string headerContent)
        {
            EditorGUILayout.LabelField(headerContent, _headerStyle);
            EditorGUILayout.Space(5f);
        }

        /// <summary>
        /// Changes the GUI's color
        /// </summary>
        /// <param name="newColor">The color to change to</param>
        private static void ChangeGUIColor(Color newColor)
        {
            _guiColorStack.Push(GUI.color);
            GUI.color = newColor;
        }

        /// <summary>
        /// Restores the GUI to its previous color
        /// </summary>
        private static void RestoreGUIColor()
        {
            if (_guiColorStack.Count <= 0) return;
            GUI.color = _guiColorStack.Pop();
        }

        #endregion
    }
}
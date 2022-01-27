using System;
using System.Collections.Generic;
using System.Linq;
using Ninito.UnityProjectLinter.Editor.AssetPostprocessors;
using Ninito.UnityProjectLinter.Editor.AssetRenamer;
using Ninito.UnityProjectLinter.Editor.Logging;
using Ninito.UnityProjectLinter.Editor.Settings;
using Ninito.UnityProjectLinter.Editor.Utilities;
using Ninito.Whetstone.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Ninito.UnityProjectLinter.Editor.Settings
{
	/// <summary>
	/// An IMGUI settings provider for the Unity Project Linter plugin.
	/// </summary>
	public sealed class AssetLintingSettingsProvider : SettingsProvider
	{
		#region Private Fields

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

		private static AssetLintingSettings _settings = AssetLintingSettings.GetOrCreateSettings();

		private static SerializedObject _settingsSerializedObject =
			AssetLintingSettings.GetSerializedSettings();

		private static readonly LoggerEntry[] _availableLoggers = LoggerSelectionAssistant.GetAvailableLoggers();
		private static int _selectedLoggerIndex = 0;

		#endregion

		#region SettingsProvider Implementation

		private AssetLintingSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) :
			base(path, scopes, keywords)
		{
		}

		#endregion

		#region Unity Callbacks

		[SettingsProvider]
		public static SettingsProvider Register()
		{
			SettingsProvider provider =
				new AssetLintingSettingsProvider("Project/Unity Project Linter", SettingsScope.Project)
				{
					label = "Unity Project Linter",
					keywords = GetSearchKeywordsFromSerializedObject(_settingsSerializedObject)
				};

			return provider;
		}

		public override void OnActivate(string searchContext, VisualElement rootElement)
		{
			_settings = AssetLintingSettings.GetOrCreateSettings();
			_settingsSerializedObject = AssetLintingSettings.GetSerializedSettings();
		}

		public override void OnGUI(string searchContext)
		{
			_settingsSerializedObject.Update();

			DrawGeneralSection();
			DrawLoggingSection();
			DrawIgnoreSection();
			DrawNamingRulesSection();

			_settingsSerializedObject.ApplyModifiedProperties();
		}

		#endregion

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
			EditorGUILayout.Space(30f);
		}

		/// <summary>
		///     Draws the logging tab
		/// </summary>
		private static void DrawLoggingSection()
		{
			ShowHeader("Logging");

			EditorGUILayout.BeginHorizontal();
			DrawExportViolatingAssetsLogButton();
			DrawLoggerSelectionDropdown();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space(30f);
		}

		/// <summary>
		///		Draws a dropdown containing selections for available loggers
		/// </summary>
		private static void DrawLoggerSelectionDropdown()
		{
			_selectedLoggerIndex = EditorGUILayout.Popup(_selectedLoggerIndex,
				_availableLoggers.Select(logger => logger.Name).ToArray());
		}

		/// <summary>
		///     Draws the ignore tab
		/// </summary>
		private static void DrawIgnoreSection()
		{
			ShowHeader("Ignored Assets");
			DrawSerializedProperty("IgnoredPaths");

			EditorGUILayout.Space(20f);
			DrawSerializedProperty("IgnoredAssets");

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
			DrawSerializedProperty("NamingRules");
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
				_settings.IgnoreAsset(AssetDatabase.GetAssetPath(Selection.activeObject));
			}

			EditorGUI.EndDisabledGroup();
		}

		/// <summary>
		///     Draws the ignore whole folder button
		/// </summary>
		private static void DrawIgnoreFolderButton()
		{
			if (!GUILayout.Button("Ignore Whole Folder")) return;

			if (_settings == null) return;

			string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
			_settings.IgnorePath(AssetDatabaseUtilities.GetFolderOfAsset(assetPath));
		}

		/// <summary>
		///     Draws the check all asset names button
		/// </summary>
		private static void DrawCheckAllAssetsButton()
		{
			if (!GUILayout.Button("Check All Assets")) return;
			HashSet<string> lintedAssets = new HashSet<string>();
			AssetNameReceiver.LintAssets(AssetNameReceiver.GetAllAssetPaths(), ref lintedAssets);
		}

		/// <summary>
		///     Draws the asset renamer button
		/// </summary>
		private static void DrawAssetRenamerButton()
		{
			if (!GUILayout.Button("Open Asset Renamer")) return;
			AssetRenamerWindow.CreateRenamerWindow(AssetNameReceiver.GetAllViolatingAssetPaths().ToArray(),
				_settings);
		}

		/// <summary>
		///     Draws the export violating assets log button
		/// </summary>
		private static void DrawExportViolatingAssetsLogButton()
		{
			if (!GUILayout.Button("Export Log")) return;

			if (!(Activator.CreateInstance(_availableLoggers[_selectedLoggerIndex]
				    .Type) is IRuleViolationLogger logger))
			{
				Debug.LogWarning("Failed to create instance of logger! Logging could not proceed!");
				return;
			}

			AssetNameReceiver.LogViolatingAssets(logger);
			logger.GenerateLog();
		}

		/// <summary>
		///     Draws the default rules
		/// </summary>
		private static void DrawDefaultRules()
		{
			DrawSerializedProperty("IgnoreScriptAssets");
		}

		/// <summary>
		///     Draws the general settings tab of the inspector
		/// </summary>
		private static void DrawGeneralSettings()
		{
			RestoreGUIColor();

			DrawSerializedProperty("WarnOnIncorrect");
		}

		/// <summary>
		/// Draws a serialized property
		/// </summary>
		/// <param name="propertyName">The name of the property to draw</param>
		/// <returns>The property being drawn</returns>
		private static SerializedProperty DrawSerializedProperty(string propertyName)
		{
			SerializedProperty drawnProperty = _settingsSerializedObject.FindProperty(propertyName);
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
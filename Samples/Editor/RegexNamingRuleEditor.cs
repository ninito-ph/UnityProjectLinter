using System.Text.RegularExpressions;
using Ninito.UnityProjectLinter.Editor.Extensions;
using Ninito.Whetstone.Editor;
using UnityEditor;

namespace Ninito.UnityProjectLinter.Editor.Samples
{
	/// <summary>
	/// A custom editor with Regex visualization support for RegexNamingRule
	/// </summary>
	[CustomEditor(typeof(RegexNamingRule))]
	public sealed class RegexNamingRuleEditor : AutoEditor
	{
		#region Fields

		private string _regexOutput;
		private string _regexInput = "GameObject";
		private string _defaultRegexPreview;
		private bool _regexIsValid;

		private RegexNamingRule _regexNamingRule;

		#endregion

		#region Unity Callbacks

		public override void OnInspectorGUI()
		{
			DrawDefaultSection();

			CacheRegexNamingRule();

			DrawRegexField();
			DrawAssetNamePreview();

			UpdateRegexPreview();

			SaveChanges();
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Draws the default rule properties
		/// </summary>
		private void DrawDefaultSection()
		{
			DrawSerializedProperty("priority");
			DrawSerializedProperty("context");
		}

		/// <summary>
		/// Saves changes made to the rule, if they are valid
		/// </summary>
		private void SaveChanges()
		{
			if (_regexIsValid)
			{
				serializedObject.ApplyModifiedProperties();
			}
		}
		
		/// <summary>
		/// Draws a field that receives a Regular Expression
		/// </summary>
		private void DrawRegexField()
		{
			if (!_regexIsValid)
			{
				ChangeGUIColor(EditorColors.ErrorRed);
			}

			DrawSerializedProperty("regexPattern");

			RestoreGUIColor();
		}

		/// <summary>
		/// Caches the Regex naming rule (Target object)
		/// </summary>
		private void CacheRegexNamingRule()
		{
			if (_regexNamingRule == null)
			{
				_regexNamingRule = target as RegexNamingRule;
			}
		}

		/// <summary>
		/// Draws the asset name preview
		/// </summary>
		private void DrawAssetNamePreview()
		{
			if (!_regexIsValid) return;
			
			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical();

			EditorGUILayout.LabelField("Preview Input");

			_regexInput = EditorGUILayout.TextField(_regexInput);

			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginVertical();

			EditorGUILayout.LabelField("Preview");

			EditorGUI.BeginDisabledGroup(true);

			EditorGUILayout.TextField(_regexOutput);

			EditorGUI.EndDisabledGroup();

			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
		}

		/// <summary>
		/// Updates the regex preview
		/// </summary>
		private void UpdateRegexPreview()
		{
			string regexPattern = serializedObject.FindProperty("regexPattern").stringValue;

			// Unfortunately, .NET Regex does not support the necessary features to make a Regex-verifying Regex
			try
			{
				Regex defaultRegex = new Regex(regexPattern);
				_regexOutput = defaultRegex.GetAllMatchesAsString(_regexInput);
				_regexIsValid = true;
			}
			catch
			{
				// ignored
				_regexOutput = _regexInput;
				_regexIsValid = false;
				EditorGUILayout.HelpBox("Regex is invalid!", MessageType.Error);
			}

			#endregion
		}
	}
}
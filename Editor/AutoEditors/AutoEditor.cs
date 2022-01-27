using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ninito.Whetstone.Editor
{
	/// <summary>
	/// A simple base class for custom inspectors to derive from
	/// </summary>
	public abstract class AutoEditor : UnityEditor.Editor
	{
		#region Fields

		private readonly Stack<Color> _guiColorStack = new Stack<Color>();

		#endregion

		#region Protected Methods

		/// <summary>
		/// Changes the GUI color to the specified color. Can be reverted to the previous color
		/// with <see cref="RestoreGUIColor"/>
		/// </summary>
		/// <param name="newColor">The color to change to</param>
		protected void ChangeGUIColor(Color newColor)
		{
			_guiColorStack.Push(GUI.color);
			GUI.color = newColor;
		}

		/// <summary>
		/// Restores the previous GUI color
		/// </summary>
		protected void RestoreGUIColor()
		{
			if (_guiColorStack.Count <= 0) return;
			GUI.color = _guiColorStack.Pop();
		}

		/// <summary>
		/// Draws a serialized property through its name
		/// </summary>
		/// <param name="propertyName">The serialized property's name</param>
		/// <returns>The serialized property being drawn</returns>
		protected SerializedProperty DrawSerializedProperty(string propertyName)
		{
			SerializedProperty drawnProperty = serializedObject.FindProperty(propertyName);
			EditorGUILayout.PropertyField(drawnProperty);
			return drawnProperty;
		}

		#endregion
	}
}
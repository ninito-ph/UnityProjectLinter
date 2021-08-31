using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ninito.UnityProjectLinter.Editor
{
    public class AutoEditor : UnityEditor.Editor
    {
        private readonly Stack<Color> _guiColorStack = new Stack<Color>();

        protected void ChangeGUIColor(Color newColor)
        {
            _guiColorStack.Push(GUI.color);
            GUI.color = newColor;
        }

        protected void RestoreGUIColor()
        {
            if (_guiColorStack.Count <= 0) return;
            GUI.color = _guiColorStack.Pop();
        }
        
        protected SerializedProperty DrawSerializedProperty(string propertyName)
        {
            SerializedProperty drawnProperty = serializedObject.FindProperty(propertyName);
            EditorGUILayout.PropertyField(drawnProperty);
            return drawnProperty;
        }
    }
}
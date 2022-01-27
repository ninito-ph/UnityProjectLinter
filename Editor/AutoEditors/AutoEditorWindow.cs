using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ninito.Whetstone.Editor
{
    public class AutoEditorWindow : EditorWindow
    {
        #region Private Fields

        private readonly Stack<Color> _guiColorStack = new Stack<Color>();

        #endregion

        #region Protected Methods

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

        #endregion
    }
}
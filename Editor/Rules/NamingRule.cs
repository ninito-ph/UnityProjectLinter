using System;
using UnityEngine;

namespace Ninito.UnityProjectLinter.Editor.Rules
{
    /// <summary>
    /// A naming rule that checks if an asset is named properly
    /// </summary>
    public abstract class NamingRule : ScriptableObject
    {
        #region Private Fields

        [Header("Rule Parameters")]
        [SerializeField]
        [Tooltip("The priority to be used when sorting the rules.")]
        private int priority;

        #endregion

        #region Properties

        public abstract RuleContext Context { get; }
        public int Priority => priority;
        
        #endregion

        #region Public Methods

        /// <summary>
        /// Checks whether the rule applies to the given asset
        /// </summary>
        /// <param name="assetPath">The path of the asset to check for</param>
        /// <returns>Whether the rule applies to the given asset</returns>
        public abstract bool Applies(string assetPath);
        
        /// <summary>
        /// Returns the appropriate prefix for the asset
        /// </summary>
        /// <param name="assetPath">The path of the asset to check for</param>
        /// <returns>The appropriate prefix for the asset</returns>
        public virtual string GetPrefix(string assetPath)
        {
            return String.Empty;
        }
        
        /// <summary>
        /// Returns the appropriate infix for the asset
        /// </summary>
        /// <param name="assetPath">The path of the asset to check for</param>
        /// <returns>The appropriate infix for the asset</returns>
        public virtual string GetInfix(string assetPath)
        {
            return String.Empty;
        }
        
        /// <summary>
        /// Returns the appropriate suffix for the asset
        /// </summary>
        /// <param name="assetPath">The path of the asset to check for</param>
        /// <returns>The appropriate suffix for the asset</returns>
        public virtual string GetSuffix(string assetPath)
        {
            return String.Empty;
        }

        #endregion

        #region Nested Enums

        /// <summary>
        /// A context to which the rule applies
        /// </summary>
        public enum RuleContext
        {
            Prefix,
            Suffix,
            Infix
        }

        #endregion
    }
}
using System;
using UnityEditor;
using UnityEngine;

namespace Ninito.UnityProjectLinter.LintingRules
{
    [CreateAssetMenu(fileName = CreateAssetMenus.PrefixNamingRuleFileName,
        menuName = CreateAssetMenus.PrefixNamingRuleMenuName, order = CreateAssetMenus.PrefixNamingRuleOrder)]
    public class SimplePrefixNamingRule : NamingRule
    {
        [SerializeField, Tooltip("The name of the type to define the prefix for")]
        private string typeName = String.Empty;
        
        private int _typeNameHash;
        
        [field: SerializeField, Tooltip("The prefix for the defined type. Implicitly includes an underscore.")]
        private string Prefix { get; set; } = String.Empty;

        private int TypeNameHash
        {
            get
            {
                if (_typeNameHash == default)
                {
                    _typeNameHash = typeName.GetHashCode();
                }

                return _typeNameHash;
            }
        }

        public override bool AppliesToAsset(string assetPath)
        {
            return TypeMatches(AssetDatabase.GetMainAssetTypeAtPath(assetPath).Name);
        }
        
        public override string GetFixForAsset(string assetPath)
        {
            return Prefix == String.Empty ? String.Empty : Prefix + "_";
        }
        
        private bool TypeMatches(string otherTypeName)
        {
            return otherTypeName.GetHashCode() == TypeNameHash;
        }
    }
}
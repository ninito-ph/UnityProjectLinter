using UnityEngine;

namespace Ninito.UnityProjectLinter.LintingRules
{
    public abstract class NamingRule : ScriptableObject
    {
        [Header("Rule Parameters"), SerializeField]
        private RuleContext context;
        
        public RuleContext Context
        {
            get => context;
            set => context = value;
        }

        public abstract bool AppliesToAsset(string assetPath);
        public abstract string GetFixForAsset(string assetPath);
        
        public enum RuleContext
        {
            Prefix,
            Suffix
        }
    }
}
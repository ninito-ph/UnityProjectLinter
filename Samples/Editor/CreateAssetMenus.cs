namespace Ninito.UnityProjectLinter.Editor.Samples
{
    /// <summary>
    /// A class that keeps all data of all Create Menu headers, for easier editing.
    /// </summary>
    public static class CreateAssetMenus
    {
        #region Rules

        public const string PrefixNamingRuleMenuName = "Unity Project Linter/Prefix Naming Rule";
        public const string PrefixNamingRuleFileName = "PNR_PrefixRule";
        public const int PrefixNamingRuleOrder = 11;
        
        public const string RegexNamingRuleMenuName = "Unity Project Linter/Regex Naming Rule";
        public const string RegexNamingRuleFileName = "RNR_RegexRule";
        public const int RegexNamingRuleOrder = 11;
        
        public const string VariantSuffixNamingRuleMenuName = "Unity Project Linter/Variant Suffix Naming Rule";
        public const string VariantSuffixNamingRuleFileName = "VSNR_VariantRule";
        public const int VariantSuffixNamingRuleOrder = 11;
        
        public const string ReplaceSectionNamingRuleMenuName = "Unity Project Linter/Replace Section Rule";
        public const string ReplaceSectionNamingRuleFileName = "RSNR_ReplaceSectionRule";
        public const int ReplaceSectionNamingRuleOrder = 11;

        #endregion
    }
}
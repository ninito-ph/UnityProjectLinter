namespace Ninito.UnityProjectLinter
{
    public static class CreateAssetMenus
    {
        #region Settings

        public const string AssetLintingSettingsMenuName = "Unity Project Linter/Asset Linting Settings";
        public const string AssetLintingSettingsFileName = "ALS_LintingSettings";
        public const int AssetLintingSettingsOrder = 0;

        #endregion

        #region Rules

        public const string PrefixNamingRuleMenuName = "Unity Project Linter/Prefix Naming Rule";
        public const string PrefixNamingRuleFileName = "PNR_PrefixRule";
        public const int PrefixNamingRuleOrder = 11;
        
        public const string SuffixNamingRuleMenuName = "Unity Project Linter/Suffix Naming Rule";
        public const string SuffixNamingRuleFileName = "SNR_SuffixRule";
        public const int SuffixNamingRuleOrder = 12;

        #endregion
    }
}
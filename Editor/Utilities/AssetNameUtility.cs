using System.Text.RegularExpressions;

namespace Ninito.UnityProjectLinter.Utilities
{
    /// <summary>
    ///     An utility that handles asset names
    /// </summary>
    public static class AssetNameUtility
    {
        /// <summary>
        ///     Gets the asset's name by its path using Regex
        /// </summary>
        /// <param name="desiredAssetPath">The asset's path</param>
        /// <returns>The asset's name</returns>
        public static string GetAssetNameByPath(string desiredAssetPath)
        {
            var nameRegex = new Regex(@"[^\/]+(?=\.[^\/.]*$)");
            return nameRegex.Match(desiredAssetPath).ToString();
        }
        
        /// <summary>
        ///     Gets whether the asset name contains spaces
        /// </summary>
        /// <param name="assetPath">The asset path to get the asset name from</param>
        /// <returns>Whether the asset name contains spaces</returns>
        public static bool AssetNameContainsSpaces(string assetPath)
        {
            return GetAssetNameByPath(assetPath).Contains(" ");
        }
    }
}
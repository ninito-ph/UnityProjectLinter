using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Ninito.UnityProjectLinter.Editor.Utilities
{
    /// <summary>
    ///     An class containing utility functions for AssetDatabase
    /// </summary>
    public sealed class AssetDatabaseUtilities
    {
        /// <summary>
        ///     Gets whether an asset is a prefab
        /// </summary>
        /// <param name="assetPath">The asset path to check in</param>
        /// <param name="prefab">The prefab at the asset path, should it exist</param>
        /// <returns>Whether the asset is a prefab</returns>
        public static bool IsAssetPrefab(string assetPath, out GameObject prefab)
        {
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            return prefab != null;
        }

        /// <summary>
        ///     Gets whether an asset is a prefab
        /// </summary>
        /// <param name="assetPath">The asset path to check in</param>
        /// <returns>Whether the asset is a prefab</returns>
        public static bool IsAssetPrefab(string assetPath)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(assetPath) != null;
        }

        /// <summary>
        ///     Gets whether the asset at the given path is a script
        /// </summary>
        /// <param name="assetPath">The path of the asset to check</param>
        /// <returns>Whether the asset at the given path is a script</returns>
        public static bool IsScriptAsset(string assetPath)
        {
            return AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath) != null;
        }

        /// <summary>
        ///     Gets the folder of an asset
        /// </summary>
        /// <param name="assetPath">The path of the asset to get the folder of</param>
        /// <returns>The folder path of the given asset</returns>
        public static string GetFolderOfAsset(string assetPath)
        {
            return Regex.Match(assetPath, @"^(.+)\/([^\/]+)$").Groups[1].ToString();
        }
    }
}
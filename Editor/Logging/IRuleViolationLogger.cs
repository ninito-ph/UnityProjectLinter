namespace Ninito.UnityProjectLinter.Editor.Logging
{
    public interface IRuleViolationLogger
    {
        /// <summary>
        ///     Logs a violation in an asset naming rule
        /// </summary>
        /// <param name="assetPath">The violating asset's path</param>
        public void LogViolation(string assetPath);
        
        /// <summary>
        ///     Generates the log file
        /// </summary>
        public void GenerateLog();
    }
}
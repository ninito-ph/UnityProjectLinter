using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Ninito.UnityProjectLinter.Editor.Utilities;
using UnityEngine;

namespace Ninito.UnityProjectLinter.Editor.Logging
{
    /// <summary>
    ///     A default logger for violating assets
    /// </summary>
    public sealed class TextLogger : IRuleViolationLogger
    {
        #region Private Fields

        private string _log = "/// ASSET NAMING INCONSISTENCIES ///\n\n";
        private int _totalViolationCount;

        #endregion

        #region IRuleViolationLogger Implementation

        public void LogViolation(string assetPath)
        {
            _totalViolationCount++;
            _log +=
                // ReSharper disable once StringLiteralTypo
                $"Asset     assetname:begin'{AssetNameUtility.GetAssetNameByPath(assetPath)}'assetname:end     at     assetpath:begin'{assetPath}'assetpath:end     has naming inconsistencies.\n";
        }

        public void GenerateLog()
        {
            string logFileName = GetLogFileName();

            AppendViolationsInfo();
            FormatLog();
            WriteLogToDisk(logFileName);
            OpenLogFile(logFileName);
        }

        #endregion

        #region Private Methods

        private void AppendViolationsInfo()
        {
            _log += "\n/// SUMMARY ///\n\n";
            _log += $"A total of {_totalViolationCount} naming inconsistencies were found.";
        }
        
        /// <summary>
        ///     Gets the filename for the log file
        /// </summary>
        /// <returns>The name for the log file</returns>
        private static string GetLogFileName()
        {
            return "AssetLintingLog_" + $"{DateTime.Now:u}".Replace(" ", "_").Replace(":", ".");
        }

        /// <summary>
        ///     Formats the log with visually-pleasing regular spacing
        /// </summary>
        private void FormatLog()
        {
            int longestFlaggedNameLenght = GetLongestFlaggedElementLenght("assetname");
            int longestFlaggedPathLenght = GetLongestFlaggedElementLenght("assetpath");

            _log = CompensateSpacingOf("assetname", longestFlaggedNameLenght);
            _log = CompensateSpacingOf("assetpath", longestFlaggedPathLenght);
        }

        /// <summary>
        ///     Gets the lenght of the longest element between the given flag
        /// </summary>
        /// <param name="flag">The flag to look for</param>
        /// <returns>The lenght of the longest element between the given flag</returns>
        private int GetLongestFlaggedElementLenght(string flag)
        {
            using StringReader reader = new StringReader(_log);
            int longestFlaggedElementLenght = 0;
            string currentLine;

            do
            {
                currentLine = reader.ReadLine();

                if (currentLine == null) continue;

                int flaggedElementLenght = GetLenghtOfFlaggedSubstring(flag, currentLine);

                if (longestFlaggedElementLenght < flaggedElementLenght)
                {
                    longestFlaggedElementLenght = flaggedElementLenght;
                }
            }
            while (currentLine != null);

            return longestFlaggedElementLenght;
        }

        /// <summary>
        ///     Gets the lenght of a flagged substring
        /// </summary>
        /// <param name="flag">The flag to look for that defines the end and beginning of the substring</param>
        /// <param name="targetString"></param>
        /// <returns></returns>
        private static int GetLenghtOfFlaggedSubstring(string flag, string targetString)
        {
            return targetString.IndexOf($"{flag}:end", StringComparison.Ordinal) -
                targetString.IndexOf($"{flag}:begin", StringComparison.Ordinal) + 1;
        }

        /// <summary>
        ///     Compensates the spacing of flagged content
        /// </summary>
        /// <param name="flag">The flag to compensate space in</param>
        /// <param name="idealSpacing">The ideal spacing for the given flag</param>
        /// <returns>The log with compensated spacing for the flag</returns>
        private string CompensateSpacingOf(string flag, int idealSpacing)
        {
            using StringReader reader = new StringReader(_log);
            string currentLine;
            string compensatedLog = String.Empty;

            do
            {
                currentLine = reader.ReadLine();

                if (currentLine == null) continue;

                int currentSpacing = GetLenghtOfFlaggedSubstring(flag, currentLine);
                int spacingToAdd = idealSpacing - currentSpacing;

                currentLine = currentLine.Replace($"{flag}:begin", "");
                currentLine = currentLine.Replace($"{flag}:end", String.Concat(Enumerable.Repeat(" ", spacingToAdd)));

                compensatedLog += currentLine + '\n';
            }
            while (currentLine != null);

            return compensatedLog;
        }

        /// <summary>
        ///     Opens the log file in the computes
        /// </summary>
        /// <param name="logFileName">The log file name</param>
        private static void OpenLogFile(string logFileName)
        {
            Process.Start(Application.dataPath + "/Logs/" + logFileName + ".txt");
        }

        /// <summary>
        ///     Writes the log to disk
        /// </summary>
        /// <param name="logFileName">The filename of the log</param>
        private void WriteLogToDisk(string logFileName)
        {
            Directory.CreateDirectory(Application.dataPath + "/Logs/");
            File.WriteAllText(Application.dataPath + "/Logs/" + logFileName + ".txt", _log);
        }

        #endregion
    }
}
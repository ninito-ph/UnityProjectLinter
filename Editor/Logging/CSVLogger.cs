using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Ninito.UnityProjectLinter.Editor.Utilities;
using UnityEngine;

namespace Ninito.UnityProjectLinter.Editor.Logging
{
	/// <summary>
	/// A rule violation logger that exports the violations to a CSV file.
	/// </summary>
	public sealed class CSVLogger : IRuleViolationLogger
	{
		#region Fields

		private readonly StringBuilder _csvBuilder = new StringBuilder();

		#endregion

		#region Constructors

		public CSVLogger()
		{
			_csvBuilder.AppendLine("Violating Asset,Path");
		}

		#endregion

		#region IRuleViolationLogger

		public void LogViolation(string assetPath)
		{
			string assetName = AssetNameUtility.GetAssetNameByPath(assetPath);
			_csvBuilder.AppendLine($"{assetName},{assetPath}");
		}

		public void GenerateLog()
		{
			string fileName = GetLogFileName();
			WriteLogToDisk(fileName);
			OpenLogFile(fileName);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Writes the log file to disk
		/// </summary>
		/// <param name="logFileName">The name of the log file</param>
		private void WriteLogToDisk(string logFileName)
		{
			Directory.CreateDirectory(Application.dataPath + "/Logs/");
			File.WriteAllText(Application.dataPath + "/Logs/" + logFileName + ".csv", _csvBuilder.ToString());
		}

		/// <summary>
		///     Opens the log file in the computes
		/// </summary>
		/// <param name="logFileName">The log file name</param>
		private static void OpenLogFile(string logFileName)
		{
			Process.Start(Application.dataPath + "/Logs/" + logFileName + ".csv");
		}

		/// <summary>
		/// Generates a file name for the log based on the current time
		/// </summary>
		/// <returns>A filename for a log</returns>
		private static string GetLogFileName()
		{
			return "AssetLintingLog_" + $"{DateTime.Now:u}".Replace(" ", "_").Replace(":", ".");
		}

		#endregion
	}
}
using System;

namespace Ninito.UnityProjectLinter.Editor.Logging
{
	/// <summary>
	/// A struct that holds information about a logger for selection
	/// </summary>
	public struct LoggerEntry
	{
		#region Fields

		public string Name;
		public Type Type;

		#endregion

		#region Constructors

		public LoggerEntry(string name, Type type)
		{
			Name = name;
			Type = type;
		}

		#endregion
	}
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ninito.UnityProjectLinter.Editor.Logging
{
	/// <summary>
	/// A class that assists the selection of available loggers
	/// </summary>
	public static class LoggerSelectionAssistant
	{
		/// <summary>
		/// Gets all available loggers
		/// </summary>
		/// <returns>An array of all available loggers</returns>
		public static LoggerEntry[] GetAvailableLoggers()
		{
			Type loggerType = typeof(IRuleViolationLogger);
			IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
			                                   .SelectMany(assembly => assembly.GetTypes())
			                                   .Where(type => loggerType.IsAssignableFrom(type));

			return types.Where(type => type != typeof(IRuleViolationLogger))
			            .Select(type => new LoggerEntry(type.Name, type)).ToArray();
		}
	}
}
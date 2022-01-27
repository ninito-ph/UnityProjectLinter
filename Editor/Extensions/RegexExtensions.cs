using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ninito.UnityProjectLinter
{
	/// <summary>
	///     An extension of Regex functions
	/// </summary>
	public static class RegexExtensions
	{
		/// <summary>
		///     Gets all matches as a concatenated string
		/// </summary>
		/// <param name="regex"></param>
		/// <param name="input">The input string</param>
		/// <returns>The concatenated characters of the input string using the given pattern</returns>
		public static string GetAllMatchesAsString(this Regex regex, string input)
		{
			if (String.IsNullOrEmpty(input)) return String.Empty;
			return regex.Matches(input).Cast<Match>()
			            .Aggregate(String.Empty, (current, match) => current + match.Value);
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace DpvClassLibrary.Workers
{
	public static class StringSearchExtension
	{
		public static bool ContainsOneOrMoreInList(this string stringToSearch, IEnumerable<string> listOfSubstringsToLookFor)
		{
			return listOfSubstringsToLookFor.Any((string substring) => stringToSearch.IndexOf(substring, StringComparison.InvariantCultureIgnoreCase) >= 0);
		}
	}
}

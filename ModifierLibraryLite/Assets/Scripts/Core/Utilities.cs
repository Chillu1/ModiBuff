using System.Collections.Generic;
using System.Linq;

namespace ModifierLibraryLite.Utilities
{
	public static class Utilities
	{
		public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> source)
		{
			return source ?? Enumerable.Empty<T>();
		}
	}
}
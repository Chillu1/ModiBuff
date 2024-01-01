using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ModiBuff.Core
{
	public static class ReadOnlyExtensions
	{
		public static IReadOnlyList<T> ToReadOnlyCollection<T>(this IEnumerable<T> enumerable)
		{
			return new ReadOnlyCollection<T>(enumerable.ToArray());
		}

		public static IReadOnlyDictionary<TKey, TValue> ToReadOnlyDictionary<TKey, TValue>(
			this IDictionary<TKey, TValue> dictionary)
		{
			return new ReadOnlyDictionary<TKey, TValue>(dictionary);
		}
	}
}
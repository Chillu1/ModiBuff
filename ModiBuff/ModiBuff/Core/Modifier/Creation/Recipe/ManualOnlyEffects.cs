using System;
using System.Collections.Generic;
using System.Reflection;

namespace ModiBuff.Core
{
	/// <summary>
	///		Contains all the effects that are either not ready for recipe usage
	///		or are not meant to be used in recipes.
	/// </summary>
	public static class ManualOnlyEffects
	{
		private static readonly HashSet<Type> effectTypes;

		static ManualOnlyEffects()
		{
			effectTypes = new HashSet<Type>(new TypeEqualityComparer())
			{
				typeof(ReactCallbackRegisterEffect<>),
			};
		}

		public static void AddManualOnlyEffect(Type effectType) => effectTypes.Add(effectType);

		public static bool IsManualOnlyEffect(Type effectType) => effectTypes.Contains(effectType);
		public static bool IsManualOnlyEffect<T>() where T : IEffect => IsManualOnlyEffect(typeof(T));
		public static bool IsManualOnlyEffect(IEffect effect) => IsManualOnlyEffect(effect.GetType());
		public static bool IsManualOnlyEffect<T>(T effect) where T : IEffect => IsManualOnlyEffect(effect.GetType());

		/// <summary>
		///		Checks that the types are of same type, if both types are generic, only checks the top level type.
		/// </summary>
		private sealed class TypeEqualityComparer : IEqualityComparer<Type>
		{
			public bool Equals(Type x, Type y)
			{
				if (x == null || y == null) return false;
				if (x.GetTypeInfo().IsGenericType && y.GetTypeInfo().IsGenericType)
					return x.GetGenericTypeDefinition() == y.GetGenericTypeDefinition();
				return Type.Equals(x, y);
			}

			public int GetHashCode(Type obj)
			{
				return obj.GetTypeInfo().BaseType.GetHashCode();
			}
		}
	}
}
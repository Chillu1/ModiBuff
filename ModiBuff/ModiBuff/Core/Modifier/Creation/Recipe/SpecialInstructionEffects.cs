using System;
using System.Collections.Generic;
using System.Reflection;

namespace ModiBuff.Core
{
	/// <summary>
	///		Contains all the effects that have special saving instructions
	/// </summary>
	public static class SpecialInstructionEffects //TODO Rename
	{
		private static readonly HashSet<Type> effectTypes;

		static SpecialInstructionEffects()
		{
			effectTypes = new HashSet<Type>(new TypeEqualityComparer())
			{
				typeof(ModifierActionEffect),
				typeof(RemoveEffect),
			};
		}

		public static bool AddSpecialInstructionEffect(Type effectType) => effectTypes.Add(effectType);

		public static bool IsSpecialInstructionEffect(Type effectType) => effectTypes.Contains(effectType);
		public static bool IsSpecialInstructionEffect<T>() where T : IEffect => IsSpecialInstructionEffect(typeof(T));
		public static bool IsSpecialInstructionEffect(IEffect effect) => IsSpecialInstructionEffect(effect.GetType());

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
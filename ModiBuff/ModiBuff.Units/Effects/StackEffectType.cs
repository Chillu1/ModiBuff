using System;

namespace ModiBuff.Core.Units
{
	[Flags]
	public enum StackEffectType
	{
		None = 0,
		Effect = 1,
		Add = 2,
		AddStacksBased = 4,
		Set = 8,
		SetStacksBased = 16,
	}

	public static class StackEffectTypeExtensions
	{
		public static bool UsesMutableState(this StackEffectType stackEffectType) =>
			(stackEffectType & StackEffectType.Add) == StackEffectType.Add ||
			(stackEffectType & StackEffectType.AddStacksBased) == StackEffectType.AddStacksBased;
	}
}
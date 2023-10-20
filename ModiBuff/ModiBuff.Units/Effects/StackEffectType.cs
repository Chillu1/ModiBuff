using System;

namespace ModiBuff.Core.Units
{
	[Flags]
	public enum StackEffectType
	{
		None = 0,
		Effect = 1,
		Add = 2, //Add to all damages?
		AddStacksBased = 4,
		//Multiply = 8, //Multiply all damages?
		//MultiplyStacksBased = 16,
		//SetMultiplierStacksBased = 32, //Multiply all damages?
	}

	public static class StackEffectTypeExtensions
	{
		public static bool UsesMutableState(this StackEffectType stackEffectType) =>
			(stackEffectType & StackEffectType.Add) == StackEffectType.Add ||
			(stackEffectType & StackEffectType.AddStacksBased) == StackEffectType.AddStacksBased;
	}
}
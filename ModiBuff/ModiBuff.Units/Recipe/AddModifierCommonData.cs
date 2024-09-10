namespace ModiBuff.Core.Units
{
	public enum ModifierAddType
	{
		Self = 1,
		Applier,
	}

	public record AddModifierCommonData<TUnit>(ModifierAddType ModifierType, TUnit UnitType);
}

namespace System.Runtime.CompilerServices
{
	internal static class IsExternalInit
	{
	}
}
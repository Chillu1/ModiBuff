namespace ModiBuff.Core.Units
{
	public enum ModifierAddType
	{
		Self = 1,
		Applier,
	}

	public record AddModifierCommonData<TUnit>(ModifierAddType ModifierType, TUnit UnitType);

	/// <summary>
	///		Custom AddModifierCommonData for non-standard/non-generic modifier add actions
	/// </summary>
	public record AddModifierCommonData<TModifier, TUnit>(TModifier ModifierType, TUnit UnitType);
}

namespace System.Runtime.CompilerServices
{
	internal static class IsExternalInit
	{
	}
}
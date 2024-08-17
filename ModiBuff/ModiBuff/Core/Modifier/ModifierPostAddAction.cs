namespace ModiBuff.Core
{
	public enum TimeType
	{
		Interval = 1,
		Duration,
	}

	public record ModifierPostAddAction;

	//public sealed record ModifierAuraAddAction(IList<IUnit> Targets) : ModifierAddAction;

	public sealed record StackAction(int StacksCount) : ModifierPostAddAction;

	public sealed record AddTimeAction(TimeType TimeType, float Time) : ModifierPostAddAction;
}

namespace System.Runtime.CompilerServices
{
	internal static class IsExternalInit
	{
	}
}
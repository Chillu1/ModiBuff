using System.Collections.Generic;

namespace ModiBuff.Core
{
	public record ModifierAddAction;

	public sealed record ModifierAuraAddAction(IList<IUnit> Targets) : ModifierAddAction;
}

namespace System.Runtime.CompilerServices
{
	internal static class IsExternalInit
	{
	}
}
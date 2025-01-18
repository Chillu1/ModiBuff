using System;
using System.Runtime.CompilerServices;

namespace ModiBuff.Core
{
	[Flags]
	public enum EffectState //TODO Rename, EffectOption(s)?
	{
		None,
		ValueIsRevertible = 1,
		IsRevertible = 2,
		IsTogglable = 4,
	}

	public static class EffectStateExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasFlag(this EffectState effectState, EffectState flag) =>
			(effectState & flag) == flag;
	}
}
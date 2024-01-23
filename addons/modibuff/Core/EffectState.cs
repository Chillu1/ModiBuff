namespace ModiBuff.Core
{
	public enum EffectState //TODO Rename, EffectOption(s)?
	{
		None,
		IsRevertible,
		IsRevertibleAndTogglable,
	}

	public static class EffectStateExtensions
	{
		//Looks confusing
		public static bool IsRevertible(this EffectState effectState) =>
			effectState == EffectState.IsRevertible || effectState == EffectState.IsRevertibleAndTogglable;

		public static bool IsTogglable(this EffectState effectState) =>
			effectState == EffectState.IsRevertibleAndTogglable;

		public static bool IsRevertibleOrTogglable(this EffectState effectState) =>
			effectState == EffectState.IsRevertible || effectState == EffectState.IsRevertibleAndTogglable;
	}
}
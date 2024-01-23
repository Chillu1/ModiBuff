namespace ModiBuff.Core
{
	/// <summary>
	///		If the effect doesn't always use mutable state functionality
	/// </summary>
	public interface IMutableStateEffect : IStateEffect
	{
		bool UsesMutableState { get; }
		bool UsesMutableStackEffect { get; } //TODO Probably refactor/streamline, enum?
	}

	/*[Flags]
	public enum MutableStateType
	{
		None,
		Uses,
		UsesStack,
	}

	public static class MutableStateTypeExtensions
	{
		public static MutableStateType Get(bool isRevertible, StackEffectType stackEffectType)
		{
			var mutableStateType = MutableStateType.None;
			if (isRevertible || stackEffectType.UsesMutableState())
				mutableStateType |= MutableStateType.Uses;
			if (stackEffectType.UsesMutableState())
				mutableStateType |= MutableStateType.UsesStack;
			return mutableStateType;
		}
	}*/
}
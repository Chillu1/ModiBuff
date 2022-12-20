namespace ModiBuff.Core
{
	public interface IEffect
	{
		/// <param name="acter">owner</param>
		void Effect(IUnit target, IUnit acter);
	}

	public static class EffectExtensions
	{
		public static bool HasState(this IEffect effect, IUnit target, IUnit acter)
		{
			return (effect is IRevertEffect revertEffect && revertEffect.IsRevertible)
			       || effect is IStackEffect stackEffect;
		}
	}
}
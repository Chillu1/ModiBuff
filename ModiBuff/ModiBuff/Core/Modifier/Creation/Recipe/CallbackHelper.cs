namespace ModiBuff.Core
{
	public static class CallbackHelper
	{
		public static void Effect(IEffect effect, IUnit target, IUnit source)
		{
			EffectMethods.Effect.Invoke(effect, EffectMethods.SetEffectParameters(target, source));
		}

		public static void StackEffect(IEffect effect, int stacks, IUnit target, IUnit source)
		{
			EffectMethods.StackEffect.Invoke(effect, EffectMethods.SetStackParameters(stacks, target, source));
		}
	}
}
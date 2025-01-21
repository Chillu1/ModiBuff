using System.Runtime.CompilerServices;

namespace ModiBuff.Core.Units
{
	public static class PostEffectExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void TryEffect(this IPostEffect<float>[] postEffects, float value, IUnit target, IUnit source)
		{
			if (postEffects == null)
				return;

			foreach (var postEffect in postEffects)
				//TODO Design choice, shuold base value be the condition one?
				if (postEffect is not IConditionEffect conditionEffect ||
				    conditionEffect.Check(value, target, source))
					postEffect.Effect(value, target, source);
		}
	}
}
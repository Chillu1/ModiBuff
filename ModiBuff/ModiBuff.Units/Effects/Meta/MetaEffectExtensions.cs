using System.Runtime.CompilerServices;

namespace ModiBuff.Core.Units
{
	public static class MetaEffectExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float TryApply(this IMetaEffect<float, float>[] metaEffects, float value, IUnit target,
			IUnit source)
		{
			if (metaEffects == null)
				return value;

			foreach (var metaEffect in metaEffects)
				//TODO Design choice, shuold base value be the condition one?
				if (metaEffect is not IConditionEffect conditionEffect ||
				    conditionEffect.Check(value, target, source))
					value = metaEffect.Effect(value, target, source);

			return value;
		}
	}
}
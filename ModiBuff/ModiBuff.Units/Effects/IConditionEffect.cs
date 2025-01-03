using System.Linq;
using System.Runtime.CompilerServices;

namespace ModiBuff.Core.Units
{
	public interface IConditionEffect
	{
		//TODO Should be private, but would rather avoid inheritance
		Condition[] Conditions { get; set; }
	}

	public static class ConditionEffectExtensions
	{
		public static T Condition<T>(this T conditionEffect, Condition condition) where T : IConditionEffect
		{
			conditionEffect.Conditions = conditionEffect.Conditions.Append(condition).ToArray();
			return conditionEffect;
		}

		public static T Condition<T>(this T conditionEffect, params Condition[] conditions) where T : IConditionEffect
		{
			conditionEffect.Conditions = conditionEffect.Conditions.Concat(conditions).ToArray();
			return conditionEffect;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Check(this IConditionEffect conditionEffect, float value, IUnit target, IUnit source)
		{
			for (int i = 0; i < conditionEffect.Conditions.Length; i++)
			{
				ref readonly var condition = ref conditionEffect.Conditions[i];
				condition.Targeting.UpdateTargetSource(target, source, out var effectTarget, out var effectSource);
				if (!condition.Check(value, effectTarget, effectSource))
					return false;
			}

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Check(this IConditionEffect conditionEffect, float value, int stacks, IUnit target,
			IUnit source)
		{
			for (int i = 0; i < conditionEffect.Conditions.Length; i++)
			{
				ref readonly var condition = ref conditionEffect.Conditions[i];
				condition.Targeting.UpdateTargetSource(target, source, out var effectTarget, out var effectSource);
				if (!condition.Check(value, stacks, effectTarget, effectSource))
					return false;
			}

			return true;
		}
	}
}
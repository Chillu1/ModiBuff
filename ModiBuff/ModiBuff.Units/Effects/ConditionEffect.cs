using System;
using System.Linq;

namespace ModiBuff.Core.Units
{
	public abstract class ConditionEffect
	{
		private Condition[] _conditions = Array.Empty<Condition>();

		public T Condition<T>(Condition condition) where T : ConditionEffect
		{
			_conditions = _conditions.Append(condition).ToArray();
			return (T)this;
		}

		public T Condition<T>(params Condition[] conditions) where T : ConditionEffect
		{
			_conditions = _conditions.Concat(conditions).ToArray();
			return (T)this;
		}

		public IMetaEffect<float, float> ConditionMeta(Condition condition)
		{
			_conditions = _conditions.Append(condition).ToArray();
			return (IMetaEffect<float, float>)this;
		}

		public IPostEffect<float> ConditionPost(Condition condition)
		{
			_conditions = _conditions.Append(condition).ToArray();
			return (IPostEffect<float>)this;
		}

		public bool Check(float value, IUnit target, IUnit source)
		{
			for (int i = 0; i < _conditions.Length; i++)
			{
				ref readonly var condition = ref _conditions[i];
				condition.Targeting.UpdateTargetSource(target, source, out var effectTarget, out var effectSource);
				if (!condition.Check(value, effectTarget, effectSource))
					return false;
			}

			return true;
		}

		public bool Check(float value, int stacks, IUnit target, IUnit source)
		{
			for (int i = 0; i < _conditions.Length; i++)
			{
				ref readonly var condition = ref _conditions[i];
				condition.Targeting.UpdateTargetSource(target, source, out var effectTarget, out var effectSource);
				if (!condition.Check(value, stacks, effectTarget, effectSource))
					return false;
			}

			return true;
		}
	}
}
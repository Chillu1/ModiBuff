using System;
using System.Runtime.CompilerServices;

namespace ModiBuff.Core
{
	public sealed class HealEffect : ITargetEffect, IEventTrigger, IStateEffect, IStackEffect, IRevertEffect, IEffect
	{
		public bool IsRevertible { get; }

		private readonly float _heal;
		private readonly StackEffectType _stackEffect;
		private Targeting _targeting;
		private bool _isEventBased;

		private float _extraHeal;
		private float _totalHeal;

		public HealEffect(float heal, bool revertible = false, StackEffectType stack = StackEffectType.Effect) :
			this(heal, revertible, stack, Targeting.TargetSource)
		{
		}

		private HealEffect(float heal, bool revertible, StackEffectType stack, Targeting targeting)
		{
			_heal = heal;
			IsRevertible = revertible;
			_stackEffect = stack;
			_targeting = targeting;
		}

		public void SetTargeting(Targeting targeting) => _targeting = targeting;
		public void SetEventBased() => _isEventBased = true;

		public void Effect(IUnit target, IUnit source)
		{
			if (IsRevertible)
				_totalHeal = _heal + _extraHeal;

			Effect(_heal + _extraHeal, (IHealable)target, source);
		}

		public void RevertEffect(IUnit target, IUnit source)
		{
			Effect(-_totalHeal, (IHealable)target, source);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Effect(float value, IHealable target, IUnit source)
		{
			switch (_targeting)
			{
				case Targeting.TargetSource:
					target.Heal(value, source, !_isEventBased);
					break;
				case Targeting.SourceTarget:
					((IHealable)source).Heal(value, target, !_isEventBased);
					break;
				case Targeting.TargetTarget:
					target.Heal(value, target, !_isEventBased);
					break;
				case Targeting.SourceSource:
					((IHealable)source).Heal(value, source, !_isEventBased);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void StackEffect(int stacks, float value, IUnit target, IUnit source)
		{
			if ((_stackEffect & StackEffectType.Add) != 0)
				_totalHeal += value;

			if ((_stackEffect & StackEffectType.AddStacksBased) != 0)
				_totalHeal += value * stacks;

			if ((_stackEffect & StackEffectType.Effect) != 0)
				Effect(target, source);
		}

		public void ResetState()
		{
			_extraHeal = 0;
			_totalHeal = 0;
		}

		public IStateEffect ShallowClone() => new HealEffect(_heal, IsRevertible, _stackEffect, _targeting);
		object IShallowClone.ShallowClone() => ShallowClone();
	}
}
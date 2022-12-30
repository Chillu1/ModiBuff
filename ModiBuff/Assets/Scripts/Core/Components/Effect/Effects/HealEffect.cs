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
			this(heal, revertible, stack, Targeting.TargetActer)
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

		public void Effect(IUnit target, IUnit acter)
		{
			if (IsRevertible)
				_totalHeal = _heal + _extraHeal;

			Effect(_heal + _extraHeal, target, acter);
		}

		public void RevertEffect(IUnit target, IUnit acter)
		{
			Effect(-_totalHeal, target, acter);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Effect(float value, IUnit target, IUnit acter)
		{
			switch (_targeting)
			{
				case Targeting.TargetActer:
					target.Heal(value, acter, !_isEventBased);
					break;
				case Targeting.ActerTarget:
					acter.Heal(value, target, !_isEventBased);
					break;
				case Targeting.TargetTarget:
					target.Heal(value, target, !_isEventBased);
					break;
				case Targeting.ActerActer:
					acter.Heal(value, acter, !_isEventBased);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void StackEffect(int stacks, float value, ITargetComponent targetComponent)
		{
			if ((_stackEffect & StackEffectType.Add) != 0)
				_totalHeal += value;

			if ((_stackEffect & StackEffectType.AddStacksBased) != 0)
				_totalHeal += value * stacks;

			if ((_stackEffect & StackEffectType.Effect) != 0)
				Effect(targetComponent.Target, targetComponent.Acter);
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
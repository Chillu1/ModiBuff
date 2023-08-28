using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ModiBuff.Core
{
	public sealed class StackComponent : IStackComponent, IStateReset
	{
		private readonly WhenStackEffect _whenStackEffect;
		private readonly int _maxStacks;
		private readonly int _everyXStacks;
		private readonly IStackEffect[] _effects;
		private readonly ModifierCheck _modifierCheck;
		private readonly bool _check;

		private ITargetComponent _targetComponent;

		private int _stacks;
		private float _value;

		public StackComponent(WhenStackEffect whenStackEffect, float value, int maxStacks, int everyXStacks, IStackEffect[] effects,
			ModifierCheck check)
		{
			_whenStackEffect = whenStackEffect;
			_value = value;
			_maxStacks = maxStacks;
			_everyXStacks = everyXStacks;

			_effects = effects;
			_modifierCheck = check;

			_check = check != null;
		}

		public void SetupTarget(ITargetComponent targetComponent) => _targetComponent = targetComponent;

		public void Stack()
		{
			if (_maxStacks != -1 && _stacks >= _maxStacks)
				return;

			_stacks++;

			if (_check && !_modifierCheck.Check(_targetComponent.Source))
				return;

			switch (_whenStackEffect)
			{
				case WhenStackEffect.Always:
					StackEffect();
					return;
				case WhenStackEffect.OnMaxStacks:
					if (_stacks == _maxStacks)
						StackEffect();
					return;
				case WhenStackEffect.EveryXStacks:
					if (_everyXStacks > 0 && _stacks % _everyXStacks == 0)
						StackEffect();
					return;
				default:
#if DEBUG && !MODIBUFF_PROFILE
					Logger.LogError("Invalid stack effect: " + _whenStackEffect);
#endif
					return;
			}
		}

		public void ResetState()
		{
			_stacks = 0;
			_targetComponent.ResetState();
			for (int i = 0; i < _effects.Length; i++)
				if (_effects[i] is IStateReset effect)
					effect.ResetState();
		}

		public IStackComponent ShallowClone()
		{
			var effects = new IStackEffect[_effects.Length];
			for (int i = 0; i < _effects.Length; i++)
			{
				if (_effects[i] is IStateEffect effect)
					effects[i] = (IStackEffect)effect.ShallowClone();
				else
					effects[i] = _effects[i];
			}

			return new StackComponent(_whenStackEffect, _value, _maxStacks, _everyXStacks, effects, _modifierCheck);
		}

		object IShallowClone.ShallowClone() => ShallowClone();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void StackEffect()
		{
			int length = _effects.Length;
			switch (_targetComponent)
			{
				case IMultiTargetComponent targetComponent:
					for (int i = 0; i < length; i++)
					for (int j = 0; j < targetComponent.Targets.Count; j++)
						_effects[i].StackEffect(_stacks, _value, targetComponent.Targets[j], targetComponent.Source);
					break;
				case ISingleTargetComponent targetComponent:
					for (int i = 0; i < length; i++)
						_effects[i].StackEffect(_stacks, _value, targetComponent.Target, targetComponent.Source);
					break;
			}
		}
	}


	public enum WhenStackEffect
	{
		None,

		/// <summary>
		///		Always trigger the stack effects.
		/// </summary>
		Always,

		/// <summary>
		///		Only trigger the stack effects when the max stacks are reached.
		/// </summary>
		OnMaxStacks,

		/// <summary>
		///		Trigger the stack effects every X stacks.
		/// </summary>
		EveryXStacks,
		//OnZeroStacks,
	}
}
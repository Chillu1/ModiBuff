using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ModiBuff.Core
{
	public struct StackComponent : ITarget, IStateReset
	{
		public bool IsValid => _effects != null && _effects.Length > 0;

		private readonly WhenStackEffect _whenStackEffect;
		private readonly int _maxStacks;
		private readonly int _everyXStacks;
		private readonly IStackEffect[] _effects;
		private readonly ModifierCheck _modifierCheck;
		private readonly IStateReset[] _stateResetEffects;

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

			var stateEffectsList = new List<IStateReset>();
			for (int i = 0; i < effects.Length; i++)
			{
				if (effects[i] is IStateReset stateResetEffect)
					stateEffectsList.Add(stateResetEffect);
			}

			_stateResetEffects = stateEffectsList.ToArray();

			_targetComponent = null;
			_stacks = 0;
		}

		public void SetupTarget(ITargetComponent targetComponent) => _targetComponent = targetComponent;

		public void Stack()
		{
			if (_maxStacks != -1 && _stacks >= _maxStacks)
				return;

			_stacks++;

			if (_modifierCheck != null && !_modifierCheck.Check(_targetComponent.Source))
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
			for (int i = 0; i < _stateResetEffects.Length; i++)
				_stateResetEffects[i].ResetState();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void StackEffect()
		{
			int length = _effects.Length;
			switch (_targetComponent)
			{
				case MultiTargetComponent multiTargetComponent:
					for (int i = 0; i < length; i++)
					for (int j = 0; j < multiTargetComponent.Targets.Count; j++)
						_effects[i].StackEffect(_stacks, _value, multiTargetComponent.Targets[j], multiTargetComponent.Source);
					break;
				case SingleTargetComponent singleTargetComponent:
					for (int i = 0; i < length; i++)
						_effects[i].StackEffect(_stacks, _value, singleTargetComponent.Target, singleTargetComponent.Source);
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
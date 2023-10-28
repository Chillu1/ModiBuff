using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ModiBuff.Core
{
	public sealed class StackTimerComponent : IStackTimerComponent
	{
		public int Stacks => _stacks;
		public int MaxStacks => _maxStacks;

		private readonly WhenStackEffect _whenStackEffect;
		private readonly float _stackTimer;
		private readonly float _value;
		private readonly int _maxStacks;
		private readonly int _everyXStacks;
		private readonly IStackEffect[] _effects;
		private readonly IStackRevertEffect[] _revertEffects;
		private readonly ModifierCheck _modifierCheck;
		private readonly IStateReset[] _stateResetEffects;
		private readonly List<float> _stackTimers;

		private ITargetComponent _targetComponent;

		private int _stacks;

		public StackTimerComponent(WhenStackEffect whenStackEffect, float stackTimer, float value, int maxStacks,
			int everyXStacks, IStackEffect[] effects, ModifierCheck check)
		{
			_whenStackEffect = whenStackEffect;
			_stackTimer = stackTimer;
			_value = value;
			_maxStacks = maxStacks;
			_everyXStacks = everyXStacks;

			_effects = effects;
			_revertEffects = effects.Where(x => x is IStackRevertEffect).Cast<IStackRevertEffect>().ToArray();
			_modifierCheck = check;

			var stateEffectsList = new List<IStateReset>();
			for (int i = 0; i < effects.Length; i++)
			{
				if (effects[i] is IStateReset stateResetEffect)
					stateEffectsList.Add(stateResetEffect);
			}

			_stateResetEffects = stateEffectsList.ToArray();

			_stackTimers = new List<float>();
		}

		public void SetupTarget(ITargetComponent targetComponent) => _targetComponent = targetComponent;

		public void Update(float delta)
		{
			for (int i = 0; i < _stackTimers.Count;)
			{
				float stackTimer = _stackTimers[i] - delta;

				if (stackTimer > 0)
				{
					_stackTimers[i] = stackTimer;
					i++;
					continue;
				}

				switch (_targetComponent)
				{
					case SingleTargetComponent singleTargetComponent:
						for (int j = 0; j < _revertEffects.Length; j++)
							_revertEffects[j].RevertStack(1, _value, singleTargetComponent.Target,
								singleTargetComponent.Source);
						break;
					case MultiTargetComponent multiTargetComponent:
						for (int j = 0; j < _revertEffects.Length; j++)
						for (int k = 0; k < multiTargetComponent.Targets.Count; k++)
							_revertEffects[j].RevertStack(1, _value, multiTargetComponent.Targets[k],
								multiTargetComponent.Source);
						break;
				}

				_stackTimers.RemoveAt(i);
			}
		}

		public void Stack()
		{
			if (_maxStacks != -1 && _stacks >= _maxStacks)
				return;

			_stacks++;
			_stackTimers.Add(_stackTimer);

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
					Logger.LogError("[ModiBuff] Invalid stack effect: " + _whenStackEffect);
#endif
					return;
			}
		}

		public void ResetStacks()
		{
			switch (_targetComponent)
			{
				case SingleTargetComponent singleTargetComponent:
					for (int i = 0; i < _stackTimers.Count; i++)
					for (int j = 0; j < _revertEffects.Length; j++)
						_revertEffects[j].RevertStack(1, _value, singleTargetComponent.Target,
							singleTargetComponent.Source);
					break;
				case MultiTargetComponent multiTargetComponent:
					for (int i = 0; i < _stackTimers.Count; i++)
					for (int j = 0; j < _revertEffects.Length; j++)
					for (int k = 0; k < multiTargetComponent.Targets.Count; k++)
						_revertEffects[j].RevertStack(1, _value, multiTargetComponent.Targets[k],
							multiTargetComponent.Source);
					break;
			}

			_stackTimers.Clear();
			_stacks = 0;
		}

		public void ResetState()
		{
			_stackTimers.Clear();
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
						_effects[i].StackEffect(_stacks, _value, multiTargetComponent.Targets[j],
							multiTargetComponent.Source);
					break;
				case SingleTargetComponent singleTargetComponent:
					for (int i = 0; i < length; i++)
						_effects[i].StackEffect(_stacks, _value, singleTargetComponent.Target,
							singleTargetComponent.Source);
					break;
			}
		}
	}
}
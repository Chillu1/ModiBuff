using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ModiBuff.Core
{
	public sealed class StackComponent : IStackComponent, IUpdatable
	{
		public int Stacks => _stacks;
		public int MaxStacks => _maxStacks;
		public bool UsesIndependentStackTime => _independentStackTime > 0;

		private readonly WhenStackEffect _whenStackEffect;
		private readonly float _independentStackTime;
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

		public StackComponent(WhenStackEffect whenStackEffect, float value, int maxStacks,
			int everyXStacks, IStackEffect[] effects, ModifierCheck check, float independentStackTime = -1)
		{
			_whenStackEffect = whenStackEffect;
			_independentStackTime = independentStackTime;
			if (_independentStackTime > 0)
				_stackTimers = new List<float>();
			_value = value;
			_maxStacks = maxStacks;
			_everyXStacks = everyXStacks;

			_effects = effects;
			_modifierCheck = check;

			var revertEffectsList = new List<IStackRevertEffect>();
			var stateEffectsList = new List<IStateReset>();
			for (int i = 0; i < effects.Length; i++)
			{
				if (effects[i] is IStackRevertEffect stackRevertEffect && stackRevertEffect.IsRevertible)
					revertEffectsList.Add(stackRevertEffect);
				if (effects[i] is IStateReset stateResetEffect)
					stateEffectsList.Add(stateResetEffect);
			}

			_revertEffects = revertEffectsList.ToArray();
			_stateResetEffects = stateEffectsList.ToArray();
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
							_revertEffects[j].RevertStack(_stacks, _value, singleTargetComponent.Target,
								singleTargetComponent.Source);
						break;
					case MultiTargetComponent multiTargetComponent:
						for (int j = 0; j < _revertEffects.Length; j++)
						for (int k = 0; k < multiTargetComponent.Targets.Count; k++)
							_revertEffects[j].RevertStack(_stacks, _value, multiTargetComponent.Targets[k],
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
			if (_independentStackTime > 0)
				_stackTimers.Add(_independentStackTime);

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
			if (_independentStackTime > 0)
			{
				RevertStacks(_stackTimers.Count);
				_stackTimers.Clear();
			}
			else
			{
				//TODO We could revert state with one RevertEffect call here,
				//but we'd need to update the total value after to 0 
				RevertStacks(_stacks);
			}

			_stacks = 0;

			return;

			void RevertStacks(int stackCount)
			{
				switch (_targetComponent)
				{
					case SingleTargetComponent singleTargetComponent:
						for (int i = 0; i < _revertEffects.Length; i++)
						for (int j = 0; j < stackCount; j++)
							_revertEffects[i].RevertStack(_stacks, _value, singleTargetComponent.Target,
								singleTargetComponent.Source);
						break;
					case MultiTargetComponent multiTargetComponent:
						for (int i = 0; i < _revertEffects.Length; i++)
						for (int j = 0; j < multiTargetComponent.Targets.Count; j++)
						for (int k = 0; k < stackCount; k++)
							_revertEffects[i].RevertStack(_stacks, _value, multiTargetComponent.Targets[j],
								multiTargetComponent.Source);
						break;
				}
			}
		}

		public void ResetState()
		{
			_stackTimers?.Clear();
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
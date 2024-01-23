using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ModiBuff.Core
{
	public sealed class StackComponent : IStackComponent, IUpdatable
	{
		public int Stacks => _stacks;
		public int MaxStacks => _maxStacks;
		public bool UsesStackTime => _singleStackTime > 0 || _independentStackTime > 0;

		private readonly WhenStackEffect _whenStackEffect;
		private readonly float _singleStackTime;
		private readonly float _independentStackTime;
		private readonly int _maxStacks;
		private readonly int _everyXStacks;
		private readonly IStackEffect[] _effects;
		private readonly IStackRevertEffect[] _revertEffects;
		private readonly ModifierCheck _modifierCheck;
		private readonly IStateReset[] _stateResetEffects;

		private float _singleStackTimer;
		private readonly List<float> _stackTimers;

		private ITargetComponent _targetComponent;

		private int _stacks;

		public StackComponent(WhenStackEffect whenStackEffect, int maxStacks, int everyXStacks,
			float singleStackTime, float independentStackTime, IStackEffect[] effects, ModifierCheck check)
		{
			_whenStackEffect = whenStackEffect;
			_singleStackTime = singleStackTime;
			if (singleStackTime > 0)
				_singleStackTimer = singleStackTime;
			_independentStackTime = independentStackTime;
			if (_independentStackTime > 0)
				_stackTimers = new List<float>();
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
				{
					//Skip mutable state effects that don't use mutable stack effects
					if (effects[i] is IMutableStateEffect stateEffect && !stateEffect.UsesMutableStackEffect)
						continue;

					stateEffectsList.Add(stateResetEffect);
				}
			}

			_revertEffects = revertEffectsList.ToArray();
			_stateResetEffects = stateEffectsList.ToArray();
		}

		public void SetupTarget(ITargetComponent targetComponent) => _targetComponent = targetComponent;

		public void Update(float delta)
		{
			for (int i = 0; i < _stackTimers?.Count;)
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
							_revertEffects[j].RevertStack(_stacks, singleTargetComponent.Target,
								singleTargetComponent.Source);
						break;
					case MultiTargetComponent multiTargetComponent:
						for (int j = 0; j < _revertEffects.Length; j++)
						for (int k = 0; k < multiTargetComponent.Targets.Count; k++)
							_revertEffects[j].RevertStack(_stacks, multiTargetComponent.Targets[k],
								multiTargetComponent.Source);
						break;
				}

				_stackTimers.RemoveAt(i);
				_stacks--;
			}

			if (_stacks == 0 || _singleStackTimer <= 0)
				return;

			_singleStackTimer -= delta;
			if (_singleStackTimer <= 0)
			{
				_singleStackTimer = _singleStackTime;
				ResetStacks();
			}
		}

		public void Stack()
		{
			//TODO Do we want to always reset the timer, or only on successful stack? Maybe enum configurable?
			if (_singleStackTime > 0)
				_singleStackTimer = _singleStackTime;

			if (_maxStacks != -1 && _stacks >= _maxStacks)
				return;

			//TODO Should effect check guard stacks as well? Maybe enum configurable?
			if (_modifierCheck != null && !_modifierCheck.Check(_targetComponent.Source))
				return;

			_stacks++;
			if (_independentStackTime > 0)
				_stackTimers.Add(_independentStackTime);

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
				//^This won't reset the extra state, which we probably want if we have a stack timer
				RevertStacks(_stacks);
			}

			_stacks = 0;

			return;

			void RevertStacks(int stackCount)
			{
				if (_revertEffects.Length == 0)
				{
#if DEBUG && !MODIBUFF_PROFILE
					Logger.LogWarning("[ModiBuff] Resetting stacks without reverting any effects. Intended?");
#endif
					return;
				}

				switch (_targetComponent)
				{
					case SingleTargetComponent singleTargetComponent:
						for (int i = 0; i < stackCount; i++)
						{
							for (int j = 0; j < _revertEffects.Length; j++)
								_revertEffects[j].RevertStack(_stacks, singleTargetComponent.Target,
									singleTargetComponent.Source);
							_stacks--;
						}

						break;
					case MultiTargetComponent multiTargetComponent:
						for (int i = 0; i < stackCount; i++)
						{
							for (int j = 0; j < _revertEffects.Length; j++)
							for (int k = 0; k < multiTargetComponent.Targets.Count; k++)
								_revertEffects[j].RevertStack(_stacks, multiTargetComponent.Targets[k],
									multiTargetComponent.Source);

							_stacks--;
						}

						break;
				}
			}
		}

		public void ResetState()
		{
			_singleStackTimer = _singleStackTime;
			_stackTimers?.Clear();
			_stacks = 0;
			//We reset effect state in stack component only because Stack() is the only method
			//that changes extra state. When the effect is reverted total state is always set back to default.
			//Will be an issue if we try to change state in Effect() or other methods.
			for (int i = 0; i < _stateResetEffects.Length; i++)
				_stateResetEffects[i].ResetState();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void StackEffect()
		{
			switch (_targetComponent)
			{
				case MultiTargetComponent multiTargetComponent:
					for (int i = 0; i < _effects.Length; i++)
					for (int j = 0; j < multiTargetComponent.Targets.Count; j++)
						_effects[i].StackEffect(_stacks, multiTargetComponent.Targets[j], multiTargetComponent.Source);
					break;
				case SingleTargetComponent singleTargetComponent:
					for (int i = 0; i < _effects.Length; i++)
						_effects[i].StackEffect(_stacks, singleTargetComponent.Target, singleTargetComponent.Source);
					break;
			}
		}

		public SaveData SaveState() => new SaveData(_stacks, _singleStackTimer, _stackTimers);

		public void LoadState(SaveData saveData)
		{
			_stacks = saveData.Stacks;
			_singleStackTimer = saveData.SingleStackTime;
			if (saveData.StackTimers != null)
				_stackTimers.AddRange(saveData.StackTimers);
		}

		public struct SaveData
		{
			public readonly int Stacks;
			public readonly float SingleStackTime;
			public readonly IReadOnlyList<float> StackTimers;

#if MODIBUFF_SYSTEM_TEXT_JSON && (NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER || NET462_OR_GREATER)
			[System.Text.Json.Serialization.JsonConstructor]
#endif
			public SaveData(int stacks, float singleStackTime, IReadOnlyList<float> stackTimers)
			{
				Stacks = stacks;
				SingleStackTime = singleStackTime;
				StackTimers = stackTimers;
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
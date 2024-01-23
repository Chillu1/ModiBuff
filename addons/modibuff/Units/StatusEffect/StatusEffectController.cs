using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ModiBuff.Core.Units
{
	/// <summary>
	///		Simple status effect controller. Doesn't care about different status effect instances.
	///		Much faster than <see cref="MultiInstanceStatusEffectController"/> but can't have infinite unique status effect instances.
	/// </summary>
	public sealed class StatusEffectController : ISingleInstanceStatusEffectController<LegalAction, StatusEffectType>,
		IStateReset
	{
		private readonly float[] _legalActionTimers;

		//If this is slow, change to a bunch of bools: CanAct, CanMove, etc...
		private LegalAction _legalActions;

		public StatusEffectController()
		{
			int legalActionsLength = LegalActionHelper.BaseCount;
			_legalActionTimers = new float[legalActionsLength];
			for (int i = 0; i < _legalActionTimers.Length; i++)
				_legalActionTimers[i] = 0;

			_legalActions = LegalAction.All;
		}

		public void Update(float deltaTime)
		{
			for (int i = 0; i < _legalActionTimers.Length; i++)
			{
				if (_legalActionTimers[i] <= 0)
					continue;

				_legalActionTimers[i] -= deltaTime;
				if (_legalActionTimers[i] <= 0)
				{
					_legalActionTimers[i] = 0;
					_legalActions |= (LegalAction)(1 << i);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasLegalAction(LegalAction legalAction) => (_legalActions & legalAction) != 0;

		public bool HasStatusEffect(StatusEffectType statusEffectType)
		{
			//Get all indexes of the status effect type
			LegalAction[] legalActions = StatusEffectTypeHelper.LegalActions[(int)statusEffectType];
			//Check if all of them are bigger than 0
			for (int i = 0; i < legalActions.Length; i++)
			{
				if ((_legalActions & legalActions[i]) == 0)
					continue;

				return false;
			}

			return true;
		}

		public void ChangeStatusEffect(StatusEffectType statusEffectType, float duration)
		{
			LegalAction[] legalActions = StatusEffectTypeHelper.LegalActions[(int)statusEffectType];
			for (int i = 0; i < legalActions.Length; i++)
			{
				var legalAction = legalActions[i];
				int legalActionIndex = StatusEffectTypeHelper.LegalActionToIndex[(int)legalAction];
				if (_legalActionTimers[legalActionIndex] >= duration)
					continue;

				_legalActionTimers[legalActionIndex] = duration;
				_legalActions &= ~legalAction;
			}
		}

		public void DecreaseStatusEffect(StatusEffectType statusEffectType, float duration)
		{
			LegalAction[] legalActions = StatusEffectTypeHelper.LegalActions[(int)statusEffectType];
			for (int i = 0; i < legalActions.Length; i++)
			{
				var legalAction = legalActions[i];
				int legalActionIndex = StatusEffectTypeHelper.LegalActionToIndex[(int)legalAction];
				float currentDuration = _legalActionTimers[legalActionIndex];
				if (currentDuration <= 0)
					continue;

				currentDuration -= duration;
				if (currentDuration <= 0)
				{
					_legalActionTimers[legalActionIndex] = 0;
					_legalActions |= legalAction;
				}
				else
				{
					_legalActionTimers[legalActionIndex] = currentDuration;
				}
			}
		}

		public void ResetState()
		{
			Array.Clear(_legalActionTimers, 0, _legalActionTimers.Length);
			_legalActions = LegalAction.All;
		}

		public SaveData SaveState() => new SaveData(_legalActionTimers, _legalActions);

		public void LoadState(SaveData saveData)
		{
			for (int i = 0; i < _legalActionTimers.Length; i++)
				_legalActionTimers[i] = saveData.LegalActionTimers[i];
			_legalActions = saveData.LegalActions;
		}

		public struct SaveData
		{
			public readonly IReadOnlyList<float> LegalActionTimers;
			public readonly LegalAction LegalActions;

#if MODIBUFF_SYSTEM_TEXT_JSON && (NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER || NET462_OR_GREATER)
			[System.Text.Json.Serialization.JsonConstructor]
#endif
			public SaveData(IReadOnlyList<float> legalActionTimers, LegalAction legalActions)
			{
				LegalActionTimers = legalActionTimers;
				LegalActions = legalActions;
			}
		}
	}
}
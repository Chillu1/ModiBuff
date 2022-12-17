using System;
using UnityEngine;

namespace ModifierLibraryLite.Core.Units
{
	public sealed class StatusEffectController
	{
		//"Reference count" of the number of times this effect has been applied, and storing status effect timers
		//VS
		//LegalAction array, with legal action timers
		private readonly float[] _legalActionTimers;

		//If this is slow, change to a bunch of bools
		public LegalAction LegalActions { get; private set; }

		private int[] _referenceCounts;

		public StatusEffectController()
		{
			int legalActionsLength = LegalActionHelper.BaseCount;
			_legalActionTimers = new float[legalActionsLength];
			for (int i = 0; i < _legalActionTimers.Length; i++)
				_legalActionTimers[i] = 0;
			_referenceCounts = new int[legalActionsLength];
			for (int i = 0; i < _referenceCounts.Length; i++)
				_referenceCounts[i] = 0;

			LegalActions = LegalAction.All;
		}

		public void Update(in float deltaTime)
		{
			for (int i = 0; i < _legalActionTimers.Length; i++)
			{
				Debug.Log($"LegalAction: {((LegalAction)(1 << i)).ToString()} Timer: {_legalActionTimers[i]}");
				if (_legalActionTimers[i] <= 0)
					continue;

				_legalActionTimers[i] -= deltaTime;
				if (_legalActionTimers[i] <= 0)
				{
					_legalActionTimers[i] = 0;
					Debug.Log($"StatusEffectController: {((LegalAction)(1 << i)).ToString()} has expired");
					TryRestoreLegalAction(i);
				}
			}
		}

		public bool HasStatusEffect(StatusEffectType statusEffectType)
		{
			//Get all indexes of the status effect type
			var legalActions = StatusEffectTypeHelper.LegalActions[(int)statusEffectType];
			//Check if all of them are bigger than 0
			for (int i = 0; i < legalActions.Length; i++)
			{
				long legalActionIndex = Utilities.Utilities.FastLog2((double)legalActions[i]);
				if (_referenceCounts[legalActionIndex] <= 0)
					return false;
			}

			return true;
		}

		public void ChangeStatusEffect(StatusEffectType statusEffectType, float duration)
		{
			var legalActions = StatusEffectTypeHelper.LegalActions[(int)statusEffectType];
			for (int i = 0; i < legalActions.Length; i++)
			{
				long legalActionIndex = Utilities.Utilities.FastLog2((double)legalActions[i]);
				if (_legalActionTimers[legalActionIndex] >= duration)
					continue;

				_legalActionTimers[legalActionIndex] = duration;
				_referenceCounts[legalActionIndex]++;
				LegalActions &= ~legalActions[i];
			}
		}

		private void TryRestoreLegalAction(int index)
		{
			_referenceCounts[index]--;
			if (_referenceCounts[index] <= 0)
			{
				_referenceCounts[index] = 0;
				Debug.Log("Restoring legal action: " + (LegalAction)(1 << index) + ". Current legal actions: " + LegalActions);
				LegalActions |= (LegalAction)(1 << index);
			}
		}
	}
}
using System.Runtime.CompilerServices;

namespace ModiBuff.Core.Units
{
	public sealed class StatusEffectController : IStatusEffectController
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
				long legalActionIndex = Utilities.Utilities.FastLog2((double)legalActions[i]);
				if (_legalActionTimers[legalActionIndex] >= duration)
					continue;

				_legalActionTimers[legalActionIndex] = duration;
				_legalActions &= ~legalActions[i];
			}
		}

		public void DecreaseStatusEffect(StatusEffectType statusEffectType, float duration)
		{
			LegalAction[] legalActions = StatusEffectTypeHelper.LegalActions[(int)statusEffectType];
			for (int i = 0; i < legalActions.Length; i++)
			{
				long legalActionIndex = Utilities.Utilities.FastLog2((double)legalActions[i]);
				float currentDuration = _legalActionTimers[legalActionIndex];
				if (currentDuration <= 0)
					continue;

				currentDuration -= duration;
				if (currentDuration <= 0)
				{
					_legalActionTimers[legalActionIndex] = 0;
					_legalActions |= legalActions[i];
				}
				else
				{
					_legalActionTimers[legalActionIndex] = currentDuration;
				}
			}
		}
	}
}
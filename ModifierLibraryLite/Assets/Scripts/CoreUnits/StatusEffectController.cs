namespace ModifierLibraryLite.Core.Units
{
	public sealed class StatusEffectController
	{
		private readonly float[] _legalActionTimers;

		//If this is slow, change to a bunch of bools: CanAct, CanMove, etc...
		public LegalAction LegalActions { get; private set; }

		public StatusEffectController()
		{
			int legalActionsLength = LegalActionHelper.BaseCount;
			_legalActionTimers = new float[legalActionsLength];
			for (int i = 0; i < _legalActionTimers.Length; i++)
				_legalActionTimers[i] = 0;

			LegalActions = LegalAction.All;
		}

		public void Update(in float deltaTime)
		{
			for (int i = 0; i < _legalActionTimers.Length; i++)
			{
				if (_legalActionTimers[i] <= 0)
					continue;

				_legalActionTimers[i] -= deltaTime;
				if (_legalActionTimers[i] <= 0)
				{
					_legalActionTimers[i] = 0;
					LegalActions |= (LegalAction)(1 << i);
				}
			}
		}

		public bool HasLegalAction(LegalAction legalAction)
		{
			return (LegalActions & legalAction) != 0;
		}

		public bool HasStatusEffect(StatusEffectType statusEffectType)
		{
			//Get all indexes of the status effect type
			var legalActions = StatusEffectTypeHelper.LegalActions[(int)statusEffectType];
			//Check if all of them are bigger than 0
			for (int i = 0; i < legalActions.Length; i++)
			{
				if ((LegalActions & legalActions[i]) == 0)
					continue;

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
				LegalActions &= ~legalActions[i];
			}
		}

		public void DecreaseStatusEffect(StatusEffectType statusEffectType, float duration)
		{
			var legalActions = StatusEffectTypeHelper.LegalActions[(int)statusEffectType];
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
					LegalActions |= legalActions[i];
				}
				else
				{
					_legalActionTimers[legalActionIndex] = currentDuration;
				}
			}
		}
	}
}
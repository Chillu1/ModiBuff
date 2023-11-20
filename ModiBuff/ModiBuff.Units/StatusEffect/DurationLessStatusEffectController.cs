using System.Runtime.CompilerServices;

namespace ModiBuff.Core.Units
{
	public sealed class DurationLessStatusEffectController : IStateReset,
		IDurationLessStatusEffectController<LegalAction, StatusEffectType>
	{
		//If this is slow, change to a bunch of bools: CanAct, CanMove, etc...
		private LegalAction _legalActions = LegalAction.All;

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

		public void ApplyStatusEffect(StatusEffectType statusEffectType)
		{
			LegalAction[] legalActions = StatusEffectTypeHelper.LegalActions[(int)statusEffectType];
			for (int i = 0; i < legalActions.Length; i++)
				_legalActions &= ~legalActions[i];
		}

		public void RemoveStatusEffect(StatusEffectType statusEffectType)
		{
			LegalAction[] legalActions = StatusEffectTypeHelper.LegalActions[(int)statusEffectType];
			for (int i = 0; i < legalActions.Length; i++)
				_legalActions |= legalActions[i];
		}

		public void ResetState()
		{
			_legalActions = LegalAction.All;
		}
	}
}
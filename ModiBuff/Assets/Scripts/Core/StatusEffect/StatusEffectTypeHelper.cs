namespace ModiBuff.Core
{
	public static class StatusEffectTypeHelper
	{
		public static LegalAction[][] LegalActions;

		static StatusEffectTypeHelper()
		{
			LegalActions = new LegalAction[(int)StatusEffectType.Last + 1][];

			var stunLegalAction = new[]
			{
				LegalAction.Act, LegalAction.Cast, LegalAction.Move, LegalAction.Prioritize, LegalAction.Think
			};
			LegalActions[(int)StatusEffectType.Stun] = stunLegalAction;

			var freezeLegalAction = new[]
			{
				LegalAction.Act, LegalAction.Move
			};
			LegalActions[(int)StatusEffectType.Freeze] = freezeLegalAction;

			var rootLegalAction = new[]
			{
				LegalAction.Move, LegalAction.Prioritize
			};
			LegalActions[(int)StatusEffectType.Root] = rootLegalAction;

			var disarmLegalAction = new[]
			{
				LegalAction.Act
			};
			LegalActions[(int)StatusEffectType.Disarm] = disarmLegalAction;

			var silenceLegalAction = new[]
			{
				LegalAction.Cast, LegalAction.Prioritize
			};
			LegalActions[(int)StatusEffectType.Silence] = silenceLegalAction;

			var tauntLegalAction = new[]
			{
				LegalAction.Cast, LegalAction.Prioritize
			};
			LegalActions[(int)StatusEffectType.Taunt] = tauntLegalAction;

			var confuseLegalAction = new[]
			{
				LegalAction.Prioritize, LegalAction.Think
			};
			LegalActions[(int)StatusEffectType.Confuse] = confuseLegalAction;
		}
	}
}
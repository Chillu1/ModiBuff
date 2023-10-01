using System;
using System.Linq;

namespace ModiBuff.Core.Units
{
	public static class StatusEffectTypeHelper
	{
		public static readonly LegalAction[][] LegalActions;
		public static readonly int[] LegalActionToIndex;

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

			var sleepLegalAction = new[]
			{
				LegalAction.Act, LegalAction.Cast, LegalAction.Move, LegalAction.Prioritize
			};
			LegalActions[(int)StatusEffectType.Sleep] = sleepLegalAction;

			LegalActionToIndex = new int[(int)Enum.GetValues(typeof(LegalAction)).Cast<LegalAction>().Max() + 1];
			for (int i = 0; i < LegalActionToIndex.Length; i++)
				LegalActionToIndex[i] = (int)Utilities.Utilities.FastLog2(i);
		}
	}
}
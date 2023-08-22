using ModiBuff.Core;
using ModiBuff.Core.Units;

namespace ModiBuff.Tests
{
	public class TestModifierRecipes : Core.TestModifierRecipes
	{
		public TestModifierRecipes(ModifierIdManager idManager) : base(idManager)
		{
		}

		protected override void SetupRecipes()
		{
			base.SetupRecipes();

			Add("InitAttackAction")
				.Effect(new AttackActionEffect(), EffectOn.Init);

			Add("InitAttackAction_Self")
				.Effect(new AttackActionEffect(), EffectOn.Init, Targeting.TargetTarget);

			AddEvent("AttackSelf_OnHit_Event", EffectOnEvent.WhenAttacked)
				.Effect(new SelfAttackActionEffect());
		}
	}
}
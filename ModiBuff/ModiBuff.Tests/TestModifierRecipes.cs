using ModiBuff.Core;
using ModiBuff.Core.Units;
using CostCheck = ModiBuff.Core.Units.CostCheck;

namespace ModiBuff.Tests
{
	public class TestModifierRecipes : Core.Units.TestModifierRecipes
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

			Add("InitDamage_CostManaEffect")
				.EffectCheck(new CostCheck(CostType.Mana, 5))
				.Effect(new DamageEffect(5), EffectOn.Init);

			AddEvent("AttackSelf_OnHit_Event", EffectOnEvent.WhenAttacked)
				.Effect(new SelfAttackActionEffect());
		}
	}
}
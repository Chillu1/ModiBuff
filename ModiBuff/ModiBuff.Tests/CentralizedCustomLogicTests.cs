using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class CentralizedCustomLogicTests : ModifierTests
	{
		public static readonly RecipeAddFunc PoisonRecipe = add => add("Poison")
			.Stack(WhenStackEffect.Always)
			.Effect(new PoisonDamageEffect(), EffectOn.Interval | EffectOn.Stack)
			.Interval(1)
			.Remove(5).Refresh();

		[Test]
		public void PoisonEffect()
		{
			AddRecipe(PoisonRecipe);
			Setup();

			int id = IdManager.GetId("Poison").Value;
			Unit.AddApplierModifierNew(id, ApplierType.Cast);
			Unit.TryCast(id, Enemy);
			Enemy.Update(1);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			Unit.TryCast(id, Enemy);
			Enemy.Update(1);
			Assert.AreEqual(EnemyHealth - 5 - 5 * 2, Enemy.Health);
		}

		private static readonly RecipeAddFunc[] healBasedOnPoisonStacksEventRecipes =
		{
			add => add("HealPerPoisonStack")
				.Effect(new HealEffect(0, HealEffect.EffectState.None,
					StackEffectType.Effect | StackEffectType.SetStacksBased, 1), EffectOn.CallbackEffect)
				.CallbackEffect(CallbackType.PoisonDamage, effect =>
					new PoisonEvent((target, source, stacks, totalStacks, damage) =>
					{
						//Kind of a stacks hack rn, by using stack effect in callbacks
						((IStackEffect)effect).StackEffect(stacks, target, source);
					})),
			//ModifierAction.Stack version
			add => add("HealPerPoisonStack")
				.Stack(WhenStackEffect.Always)
				.CustomStack(CustomStackEffectOn.CallbackEffect)
				.Effect(new HealEffect(0, HealEffect.EffectState.None,
					StackEffectType.Effect | StackEffectType.SetStacksBased, 1), EffectOn.Stack)
				.CallbackEffect(CallbackType.PoisonDamage, effect =>
					new PoisonEvent((target, source, stacks, totalStacks, damage) =>
					{
						effect.Effect(target, source);
					}))
		};

		[TestCaseSource(nameof(healBasedOnPoisonStacksEventRecipes))]
		public void HealBasedOnPoisonStacksEvent(RecipeAddFunc recipe)
		{
			AddRecipe(PoisonRecipe);
			AddRecipe(recipe);
			Setup();

			Enemy.AddModifierSelf("HealPerPoisonStack");
			int id = IdManager.GetId("Poison").Value;
			Unit.AddApplierModifierNew(id, ApplierType.Cast);
			Unit.TryCast(id, Enemy);
			Enemy.Update(1);
			Enemy.AddModifierSelf("HealPerPoisonStack"); //Checks for stack behaviour
			Assert.AreEqual(EnemyHealth - 5 + 1, Enemy.Health);

			Unit.TryCast(id, Enemy);
			Enemy.Update(1);
			Assert.AreEqual(EnemyHealth - 5 + 1 - 5 * 2 + 1 * 2, Enemy.Health);
		}

		[Test]
		public void HealBasedOnPoisonStacks()
		{
			AddRecipe(PoisonRecipe);
			AddRecipe("PoisonHeal")
				.Stack(WhenStackEffect.Always)
				.Effect(new HealEffect(0, HealEffect.EffectState.None,
						StackEffectType.Effect | StackEffectType.SetStacksBased, 1)
					.SetMetaEffects(new AddValueBasedOnPoisonStacksMetaEffect(1f)), EffectOn.Stack);
			AddRecipe("PosionHealApplier")
				.Effect(new ApplierEffect("Poison"), EffectOn.Init)
				.Effect(new ApplierEffect("PoisonHeal", targeting: Targeting.SourceTarget), EffectOn.Init);
			Setup();

			Unit.TakeDamage(UnitHealth / 2f, Unit);
			Unit.AddApplierModifierNew("PosionHealApplier", ApplierType.Cast);

			Unit.TryCast("PosionHealApplier", Enemy);
			Assert.AreEqual(UnitHealth / 2f + 1, Unit.Health);

			Unit.TryCast("PosionHealApplier", Enemy);
			Assert.AreEqual(UnitHealth / 2f + 1 + 1 * 2, Unit.Health);
		}

		[Test]
		public void PoisonDamageThornsEvent()
		{
			AddRecipe(PoisonRecipe);
			AddRecipe("PoisonThorns")
				.Callback(CallbackType.PoisonDamage,
					new PoisonEvent((target, source, stacks, totalStacks, damage) =>
					{
						((IAttackable<float, float>)source).TakeDamage(damage, target);
					}));
			Setup();

			Enemy.AddModifierSelf("PoisonThorns");
			int id = IdManager.GetId("Poison").Value;
			Unit.AddApplierModifierNew(id, ApplierType.Cast);

			Unit.TryCast(id, Enemy);
			Assert.AreEqual(UnitHealth, Unit.Health);

			Enemy.Update(1);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.TryCast(id, Enemy);
			Enemy.Update(1);
			Assert.AreEqual(UnitHealth - 5 - 5 * 2, Unit.Health);

			Ally.AddApplierModifierNew(id, ApplierType.Cast);
			Ally.TryCast(id, Enemy);

			Enemy.Update(1);
			Assert.AreEqual(UnitHealth - 5 - 5 * 2 - 5 * 2, Unit.Health);
			Assert.AreEqual(AllyHealth - 5, Ally.Health);
		}
	}
}
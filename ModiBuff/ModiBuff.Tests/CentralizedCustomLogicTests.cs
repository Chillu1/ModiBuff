using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class CentralizedCustomLogicTests : ModifierTests
	{
		private readonly RecipeAddFunc _poisonRecipe = add => add("Poison")
			.Stack(WhenStackEffect.Always)
			.Effect(new PoisonDamageEffect(StackEffectType.None, 5), EffectOn.Interval | EffectOn.Stack)
			.Interval(1)
			.Remove(5).Refresh();

		[Test]
		public void PoisonEffect()
		{
			AddRecipe(_poisonRecipe);
			Setup();

			Unit.AddApplierModifier(Recipes.GetGenerator("Poison"), ApplierType.Cast);
			Unit.TryCast("Poison", Enemy);
			Enemy.Update(1);
			Assert.AreEqual(EnemyHealth - 5, Enemy.Health);

			Unit.TryCast("Poison", Enemy);
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
				.Tag(Core.TagType.ZeroDefaultStacks)
				.Stack(WhenStackEffect.Always)
				.Effect(new HealEffect(0, HealEffect.EffectState.None,
					StackEffectType.Effect | StackEffectType.SetStacksBased, 1), EffectOn.Stack)
				.CallbackEffect(CallbackType.PoisonDamage, effect =>
					new PoisonEvent((target, source, stacks, totalStacks, damage) =>
					{
						effect.Effect(target, source);
					}))
				.ModifierAction(ModifierAction.Stack, EffectOn.CallbackEffect)
		};

		[TestCaseSource(nameof(healBasedOnPoisonStacksEventRecipes))]
		public void HealBasedOnPoisonStacksEvent(RecipeAddFunc recipe)
		{
			AddRecipe(_poisonRecipe);
			AddRecipe(recipe);
			Setup();

			Enemy.AddModifierSelf("HealPerPoisonStack");
			Unit.AddApplierModifier(Recipes.GetGenerator("Poison"), ApplierType.Cast);
			Unit.TryCast("Poison", Enemy);
			Enemy.Update(1);
			Assert.AreEqual(EnemyHealth - 5 + 1, Enemy.Health);

			Unit.TryCast("Poison", Enemy);
			Enemy.Update(1);
			Assert.AreEqual(EnemyHealth - 5 + 1 - 5 * 2 + 1 * 2, Enemy.Health);
		}

		[Test]
		public void HealBasedOnPoisonStacks()
		{
			AddRecipe(_poisonRecipe);
			AddRecipe("PoisonHealHeal")
				.Stack(WhenStackEffect.Always)
				.Effect(new HealEffect(0, HealEffect.EffectState.None,
						StackEffectType.Effect | StackEffectType.SetStacksBased, 1)
					.SetMetaEffects(new AddValueBasedOnPoisonStacksMetaEffect(1f)), EffectOn.Stack);
			AddEffect("PoisonHeal",
				new ApplierEffect("Poison"),
				new ApplierEffect("PoisonHealHeal", Targeting.SourceTarget));
			Setup();

			Unit.TakeDamage(UnitHealth / 2f, Unit);
			Unit.AddEffectApplier("PoisonHeal");

			Unit.TryCastEffect("PoisonHeal", Enemy);
			Assert.AreEqual(UnitHealth / 2f + 1, Unit.Health);

			Unit.TryCastEffect("PoisonHeal", Enemy);
			Assert.AreEqual(UnitHealth / 2f + 1 + 1 * 2, Unit.Health);
		}

		[Test]
		public void PoisonDamageThornsEvent()
		{
			AddRecipe(_poisonRecipe);
			AddRecipe("PoisonThorns")
				.Callback(new Callback<CallbackType>(CallbackType.PoisonDamage,
					new PoisonEvent((target, source, stacks, totalStacks, damage) =>
					{
						((IAttackable<float, float>)source).TakeDamage(damage, target);
					})));
			Setup();

			Enemy.AddModifierSelf("PoisonThorns");
			Unit.AddApplierModifier(Recipes.GetGenerator("Poison"), ApplierType.Cast);

			Unit.TryCast("Poison", Enemy);
			Assert.AreEqual(UnitHealth, Unit.Health);

			Enemy.Update(1);
			Assert.AreEqual(UnitHealth - 5, Unit.Health);

			Unit.TryCast("Poison", Enemy);
			Enemy.Update(1);
			Assert.AreEqual(UnitHealth - 5 - 5 * 2, Unit.Health);

			Ally.AddApplierModifier(Recipes.GetGenerator("Poison"), ApplierType.Cast);
			Ally.TryCast("Poison", Enemy);

			Enemy.Update(1);
			Assert.AreEqual(UnitHealth - 5 - 5 * 2 - 5 * 2, Unit.Health);
			Assert.AreEqual(AllyHealth - 5, Ally.Health);
		}
	}
}
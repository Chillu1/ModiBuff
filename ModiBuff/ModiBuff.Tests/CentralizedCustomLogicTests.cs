using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class CentralizedCustomLogicTests : ModifierTests
	{
		private readonly RecipeAddFunc _poisonRecipe = add => add("Poison")
			.Stack(WhenStackEffect.Always)
			.Effect(new PoisonDamageEffect(0, StackEffectType.Add, 5), EffectOn.Interval | EffectOn.Stack)
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

		[Test]
		public void HealBasedOnPoisonStacksEvent()
		{
			AddRecipe(_poisonRecipe);
			AddGenerator("HealPerPoisonStack", (id, genId, name, tag) =>
			{
				var healEffect = new HealEffect(0, false, StackEffectType.Effect | StackEffectType.SetStacksBased, 1);

				var @event = new PoisonEvent((target, source, stacks, damage) =>
				{
					//Kind of a stacks hack rn
					healEffect.StackEffect(stacks, target, source);
				});
				var callback = new CustomCallbackRegisterEffect<CustomCallbackType>(
					new CustomCallback<CustomCallbackType>(CustomCallbackType.PoisonDamage, @event));

				var initComponent = new InitComponent(false, new IEffect[] { callback }, null);
				//var stackComponent = new StackComponent(WhenStackEffect.Always, -1, -1, -1,
				//	new IStackEffect[] { healEffect }, null);

				return new Modifier(id, genId, name, initComponent, null, null, null,
					new SingleTargetComponent(), null);
			});
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
			//This becomes a problem, because we have to use one unified effect for poison for all of them to stack
			//Maybe we should have a bare bones poison effect, that each poison damage effect uses for stacks? Somehow
			//We also could obvs store the stacks on the unit instead
			//We could make a "channel" that gets the effect stacks from the unified poison effect, but might be bad design
			// AddRecipe("PoisonHeal")
			// 	.Stack(WhenStackEffect.Always)
			// 	.Effect(new PoisonDamageEffect(0, StackEffectType.Add, 5)
			// 		.SetPostEffects(new HealFromPoisonStacksFedPostEffect(1f)), EffectOn.Interval | EffectOn.Stack)
			// 	.Interval(1)
			// 	.Remove(5).Refresh();
			//AddRecipe("PoisonHeal")
			//	.Effect(new ApplierEffect("Poison")
			//		.SetPostEffects(new HealFromPoisonStacksMetaEffect(1f)), EffectOn.Init); //Is this possible somehow?
			AddRecipe(_poisonRecipe);
			AddRecipe("PoisonHealHeal")
				.Stack(WhenStackEffect.Always)
				.Effect(new HealEffect(0, false, StackEffectType.Effect | StackEffectType.SetStacksBased, 1)
					.SetMetaEffects(new HealFromPoisonStacksMetaEffect(1f)), EffectOn.Stack);
			AddRecipe("PoisonHeal")
				.Effect(new ApplierEffect("Poison"), EffectOn.Init)
				.Effect(new ApplierEffect("PoisonHealHeal", Targeting.SourceTarget), EffectOn.Init);
			Setup();

			Unit.TakeDamage(UnitHealth / 2f, Unit);
			Unit.AddApplierModifier(Recipes.GetGenerator("PoisonHeal"), ApplierType.Cast);

			Unit.TryCast("PoisonHeal", Enemy);
			Assert.AreEqual(UnitHealth / 2f + 1, Unit.Health);

			Unit.TryCast("PoisonHeal", Enemy);
			Assert.AreEqual(UnitHealth / 2f + 1 + 1 * 2, Unit.Health);
		}

		/*
		 Deal more damage when bleeding (Meta effect)
		   Each time poison stack is inflicted, heal for X poison stacks.
		   Return poison damage back to appliers (each interval, would need to store appliers & their count)
		 */
	}
}
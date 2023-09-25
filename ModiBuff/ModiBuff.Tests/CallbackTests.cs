using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class CallbackTests : ModifierTests
	{
		[Test]
		public void Init_AddDamage_HalfHealth_TriggerCallback_RemoveAndRevert()
		{
			AddGenerators(new ManualGeneratorData("InitAddDamageRevertibleHalfHealthCallback", (id, genId, name) =>
			{
				var effect = new AddDamageEffect(5, true);
				var removeEffect = new RemoveEffect(id, genId);
				removeEffect.SetRevertibleEffects(new IRevertEffect[] { effect });
				var registerEffect = new CallbackRegisterEffect<CallbackType>(
					CallbackType.StrongHit, (target, source) => removeEffect.Effect(target, source));
				var initComponent = new InitComponent(false, new IEffect[] { effect, registerEffect }, null);
				return new Modifier(id, genId, name, initComponent, null, default(StackComponent), null, new SingleTargetComponent());
			}, new ModifierAddData(true, false, false, false)));

			Unit.AddModifierSelf("InitAddDamageRevertibleHalfHealthCallback");
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);

			//Take enough damage to trigger the callback
			Unit.TakeDamage(UnitHealth * 0.6f, Unit);
			Assert.AreEqual(UnitDamage, Unit.Damage);
		}

		[Test]
		public void Init_RegisterCallbackHealToFullWhenTakingStrongHit_ManualHeal()
		{
			AddGenerators(new ManualGeneratorData("InitHealToFullHalfHealthCallback", (id, genId, name) =>
			{
				var registerEffect = new CallbackRegisterEffect<CallbackType>(
					CallbackType.StrongHit, (target, source) =>
					{
						var damageable = (IDamagable<float, float>)target;
						((IHealable<float, float>)target).Heal(damageable.MaxHealth - damageable.Health, source);
					});
				var initComponent = new InitComponent(false, new IEffect[] { registerEffect }, null);
				return new Modifier(id, genId, name, initComponent, null, default(StackComponent), null, new SingleTargetComponent());
			}, new ModifierAddData(true, false, false, false)));

			Unit.AddModifierSelf("InitHealToFullHalfHealthCallback");
			Assert.AreEqual(UnitHealth, Unit.Health);

			//Take enough damage to trigger the callback
			Unit.TakeDamage(UnitHealth * 0.6f, Unit); //Takes 60% of max hp damage, triggers callback, heals to full

			Assert.AreEqual(UnitHealth, Unit.Health);
		}

		[Test]
		public void Init_RegisterCallbackHealToFullWhenTakingStrongHit_ManualHealEffect()
		{
			AddGenerators(new ManualGeneratorData("InitHealToFullHalfHealthCallback", (id, genId, name) =>
			{
				var effect = new HealEffect(0).SetMetaEffects(new AddValueBasedOnStatDiffMetaEffect(StatType.MaxHealth));
				var registerEffect = new CallbackRegisterEffect<CallbackType>(
					CallbackType.StrongHit, (target, source) => effect.Effect(target, source));
				var initComponent = new InitComponent(false, new IEffect[] { registerEffect }, null);
				return new Modifier(id, genId, name, initComponent, null, default(StackComponent), null, new SingleTargetComponent());
			}, new ModifierAddData(true, false, false, false)));

			Unit.AddModifierSelf("InitHealToFullHalfHealthCallback");
			Assert.AreEqual(UnitHealth, Unit.Health);

			//Take enough damage to trigger the callback
			Unit.TakeDamage(UnitHealth * 0.6f, Unit); //Takes 60% of max hp damage, triggers callback, heals to full

			Assert.AreEqual(UnitHealth, Unit.Health);
		}

		[Test]
		public void Init_RegisterCallbackHealToFullWhenTakingStrongHit_Recipe()
		{
			AddRecipes(add => add("InitHealToFullHalfHealthCallback")
				.Callback(CallbackType.StrongHit, (target, source) =>
				{
					var damageable = (IDamagable<float, float>)target;
					((IHealable<float, float>)target).Heal(damageable.MaxHealth - damageable.Health, source);
				})
			);

			Unit.AddModifierSelf("InitHealToFullHalfHealthCallback");
			Assert.AreEqual(UnitHealth, Unit.Health);

			//Take enough damage to trigger the callback
			Unit.TakeDamage(UnitHealth * 0.6f, Unit); //Takes 60% of max hp damage, triggers callback, heals to full

			Assert.AreEqual(UnitHealth, Unit.Health);
		}

		[Test]
		public void Init_RegisterCallbackHealToFullWhenTakingStrongHit_RecipeEffect()
		{
			AddRecipes(add => add("InitHealToFullHalfHealthCallback")
				.Effect(new HealEffect(0).SetMetaEffects(new AddValueBasedOnStatDiffMetaEffect(StatType.MaxHealth)), EffectOn.Callback)
				.Callback(CallbackType.StrongHit)
			);

			Unit.AddModifierSelf("InitHealToFullHalfHealthCallback");
			Assert.AreEqual(UnitHealth, Unit.Health);

			//Take enough damage to trigger the callback
			Unit.TakeDamage(UnitHealth * 0.6f, Unit); //Takes 60% of max hp damage, triggers callback, heals to full

			Assert.AreEqual(UnitHealth, Unit.Health);
		}
	}
}
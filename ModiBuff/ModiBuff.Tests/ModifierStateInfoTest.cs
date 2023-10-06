using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class ModifierStateInfoTest : ModifierTests
	{
		[Test]
		public void InitDamage_CorrectBaseDamage_Recipe()
		{
			AddRecipe("InitDamage")
				.Effect(new DamageEffect(5), EffectOn.Init);
			Setup();

			var modifier = Pool.Rent(IdManager.GetId("InitDamage"));
			var state = modifier.GetState<DamageEffect.Data>();
			Assert.AreEqual(5, state.BaseDamage);
			Assert.AreEqual(0, state.ExtraDamage);
		}

		[Test]
		public void InitDamage_CorrectBaseDamage_Manual()
		{
			AddGenerator("InitDamage", (id, genId, name) =>
			{
				var damageEffect = new DamageEffect(5);
				var initComponent = new InitComponent(false, new IEffect[] { damageEffect }, null);

				return new Modifier(id, genId, name, initComponent, null, default(StackComponent), null,
					new SingleTargetComponent(), new ModifierStateInfo(damageEffect));
			}, new ModifierAddData(true, false, false, false));
			Setup();

			var modifier = Pool.Rent(IdManager.GetId("InitDamage"));
			var state = modifier.GetState<DamageEffect.Data>();
			Assert.AreEqual(5, state.BaseDamage);
			Assert.AreEqual(0, state.ExtraDamage);
		}

		[Test]
		public void InitDoubleStackDamage_CorrectBaseDamage()
		{
			AddRecipe("DoubleStackDamage")
				.Interval(1)
				.Effect(new DamageEffect(5), EffectOn.Init | EffectOn.Interval)
				.Effect(new DamageEffect(10, StackEffectType.Add), EffectOn.Stack)
				.Stack(WhenStackEffect.Always, value: 2);
			Setup();

			var modifier = Pool.Rent(IdManager.GetId("DoubleStackDamage"));
			var firstDamageState = modifier.GetState<DamageEffect.Data>();
			Assert.AreEqual(5, firstDamageState.BaseDamage);
			Assert.AreEqual(0, firstDamageState.ExtraDamage);
			var secondDamageState = modifier.GetState<DamageEffect.Data>(1);
			Assert.AreEqual(10, secondDamageState.BaseDamage);
			Assert.AreEqual(0, secondDamageState.ExtraDamage);

			modifier.UpdateSingleTargetSource(Unit, Unit);
			modifier.Stack();

			secondDamageState = modifier.GetState<DamageEffect.Data>(1);
			Assert.AreEqual(10, secondDamageState.BaseDamage);
			Assert.AreEqual(2, secondDamageState.ExtraDamage);
		}
	}
}
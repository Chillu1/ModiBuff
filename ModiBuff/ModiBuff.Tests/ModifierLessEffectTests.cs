using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class ModifierLessEffectTests : ModifierTests
	{
		[Test]
		public void InitDamageModifierLess()
		{
			AddEffect("5Damage", new DamageEffect(5f));
			AddEffect("10Damage", new DamageEffectNoState(10f));
			Setup();

			Unit.ApplyEffectSelf("5Damage");
			Assert.AreEqual(UnitHealth - 5f, Unit.Health);

			Unit.ApplyEffectSelf("10Damage");
			Assert.AreEqual(UnitHealth - 5f - 10f, Unit.Health);
		}

		[Test]
		public void StunSilenceModifierLess()
		{
			AddEffect("StunSilence", new SingleInstanceStatusEffectEffect(StatusEffectType.Stun, 1f),
				new SingleInstanceStatusEffectEffect(StatusEffectType.Silence, 2f));
			Setup();

			Unit.ApplyEffectSelf("StunSilence");
			Assert.True(Unit.HasStatusEffectSingle(StatusEffectType.Stun));
			Assert.True(Unit.HasStatusEffectSingle(StatusEffectType.Silence));

			Unit.Update(1f);
			Assert.False(Unit.HasStatusEffectSingle(StatusEffectType.Stun));
			Assert.True(Unit.HasStatusEffectSingle(StatusEffectType.Silence));
		}

		[Test]
		public void RevertibleModifierlessEffect_Invalid()
		{
			Assert.Throws<AssertionException>(() =>
			{
				AddEffect("Revertible", new AddDamageEffect(5, EffectState.IsRevertible));
			});
			Setup();
		}

		[Test]
		public void UsesMutableStateModifierlessEffect_Invalid()
		{
			Assert.Throws<AssertionException>(() =>
			{
				AddEffect("Revertible", new AddDamageEffect(5, stackEffect: StackEffectType.Add, stackValue: 1));
			});
			Setup();
		}
	}
}
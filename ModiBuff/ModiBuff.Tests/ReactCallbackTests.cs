using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public class ReactCallbackTests : ModifierTests
	{
		[Test]
		public void AddDamageAbove5RemoveDamageBelow5React()
		{
			AddGenerator("AddDamageAbove5RemoveDamageBelow5React", (id, genId, name) =>
			{
				var effect = new AddDamageEffect(5, true, true);
				var action = new DamagedChangedEvent((unit, newDamage, deltaDamage) =>
				{
					if (newDamage > 9)
						effect.Effect(unit, unit);
					else
						effect.RevertEffect(unit, unit);
				});

				var registerReactEffect = new ReactCallbackRegisterEffect<ReactType>(
					new ReactCallback<ReactType>(ReactType.DamageChanged, action));
				var initComponent = new InitComponent(false, new IEffect[] { registerReactEffect }, null);
				return new Modifier(id, genId, name, initComponent, null, default(StackComponent), null, new SingleTargetComponent());
			}, new ModifierAddData(true, false, false, false));
			Setup();

			Unit.AddModifierSelf("AddDamageAbove5RemoveDamageBelow5React"); //Starts with 10 baseDmg, adds 5 from effect
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
			Unit.Update(0);

			//Remove 6 damage, should remove the effect, making it 15 - 6 - 5 = 4
			Unit.AddDamage(-6); //Revert
			Assert.AreEqual(UnitDamage - 6, Unit.Damage);

			Unit.Update(0);
			Unit.AddDamage(6);
			Assert.AreEqual(UnitDamage + 5, Unit.Damage);
		}
	}
}
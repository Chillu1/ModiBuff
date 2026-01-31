using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class TargetTests : ModifierTests
	{
		[Test]
		public void MultiTarget_AddDamage_Revertible()
		{
			AddRecipe("InitAddDamageRevertible")
				.Effect(new AddDamageEffect(5, EffectState.IsRevertible), EffectOn.Init)
				.Remove(5);
			Setup();

			int id = IdManager.GetId("InitAddDamageRevertible").Value;
			Unit.AddApplierModifierNew(id, ApplierType.Cast);

			Unit.TryCast(id, Enemy);
			Unit.TryCast(id, Ally);

			Assert.AreEqual(EnemyDamage + 5, Enemy.Damage);
			Assert.AreEqual(AllyDamage + 5, Ally.Damage);

			Enemy.Update(5);
			Assert.AreEqual(EnemyDamage, Enemy.Damage);

			Ally.Update(5);
			Assert.AreEqual(AllyDamage, Ally.Damage);
		}
	}
}
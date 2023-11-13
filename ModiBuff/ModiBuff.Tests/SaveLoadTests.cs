using ModiBuff.Core;
using ModiBuff.Core.Units;
using NUnit.Framework;

namespace ModiBuff.Tests
{
	public sealed class SaveLoadTests : ModifierTests
	{
		[Test]
		public void SaveLoadInitDamage()
		{
			AddRecipe("NoDamage")
				.Effect(new DamageEffect(0), EffectOn.Init);
			Setup();

			var modifier = Pool.Rent(IdManager.GetId("InitDamage"));
			var saveState = modifier.SaveState();
			var loadedModifier = Pool.Rent(IdManager.GetId("InitDamage"));
			loadedModifier.LoadState(saveState, Unit);

			loadedModifier.UpdateSingleTargetSource(Unit, Unit);
			loadedModifier.Init();
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
		}

		[Test]
		public void SaveLoadInitExtraDamage()
		{
			AddRecipe("InitStackExtraDamage")
				.Stack(WhenStackEffect.Always)
				.Effect(new DamageEffect(5, StackEffectType.Add, stackValue: 2), EffectOn.Init | EffectOn.Stack);
			Setup();

			var modifier = Pool.Rent(IdManager.GetId("InitStackExtraDamage"));
			modifier.UpdateSingleTargetSource(Unit, Unit);
			modifier.Stack();
			var saveState = modifier.SaveState();
			var loadedModifier = Pool.Rent(IdManager.GetId("InitStackExtraDamage"));
			loadedModifier.LoadState(saveState, Unit);

			loadedModifier.UpdateSingleTargetSource(Unit, Unit);
			loadedModifier.Init();
			Assert.AreEqual(UnitHealth - 5 - 2, Unit.Health);
		}

		[Test]
		public void SaveAddDamageExtraStateLoad()
		{
			AddRecipe("AddDamageExtraState")
				.Stack(WhenStackEffect.Always)
				.Effect(new AddDamageEffect(5, EffectState.IsRevertible, StackEffectType.Add, stackValue: 2),
					EffectOn.Init | EffectOn.Stack)
				.Remove(5);
			Setup();

			var modifier = Pool.Rent(IdManager.GetId("AddDamageExtraState"));
			modifier.UpdateSingleTargetSource(Unit, Unit);
			modifier.Stack();

			var saveState = modifier.SaveState();
			var loadedModifier = Pool.Rent(IdManager.GetId("AddDamageExtraState"));
			loadedModifier.LoadState(saveState, Unit);

			loadedModifier.UpdateSingleTargetSource(Unit, Unit);
			loadedModifier.Init();
			Assert.AreEqual(UnitDamage + 5 + 2, Unit.Damage);

			loadedModifier.Update(5);
			Assert.AreEqual(UnitDamage, Unit.Damage);
		}

		[Test]
		public void SaveUnitLoad()
		{
			AddRecipe("AddDamageExtraState")
				.Stack(WhenStackEffect.Always)
				.Effect(new AddDamageEffect(5, EffectState.IsRevertible, StackEffectType.Add, stackValue: 2),
					EffectOn.Init | EffectOn.Stack)
				.Remove(5);
			Setup();

			Unit.AddModifierSelf("InitDamage");
			Assert.AreEqual(UnitHealth - 5, Unit.Health);
			Unit.AddModifierSelf("AddDamageExtraState");
			Unit.AddModifierSelf("AddDamageExtraState");
			Unit.Update(2);
			Assert.AreEqual(UnitDamage + 5 + 5 + 2, Unit.Damage);

			var saveData = Unit.SaveState();
			var loadedUnit = new Unit();
			loadedUnit.LoadState(saveData);

			Assert.AreEqual(UnitHealth - 5, loadedUnit.Health);
			Assert.AreEqual(UnitDamage + 5 + 5 + 2, loadedUnit.Damage);

			loadedUnit.Update(3);
			Assert.AreEqual(UnitDamage, loadedUnit.Damage);
		}

		//TODO add damage is enabled check
	}
}
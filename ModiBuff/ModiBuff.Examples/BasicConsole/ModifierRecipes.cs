using ModiBuff.Core;
using ModiBuff.Core.Units;

namespace ModiBuff.Examples.BasicConsole
{
	/// <summary>
	///		We need to define our own version of ModifierRecipes
	///		This is where we define how the modifiers should work
	///		It needs to be made in the start of the game, to be fed into the pool
	/// </summary>
	public sealed class ModifierRecipes : Core.ModifierRecipes
	{
		public ModifierRecipes(ModifierIdManager idManager) : base(idManager)
		{
			//We need to call the CreateGenerators function, to finish the setup
			//(Note that the base class will use the SetupRecipes function, which we override)
			//This can be done in the constructor, or outside of the class
			CreateGenerators();
		}

		protected override void SetupRecipes()
		{
			//This is how modifiers are defined, the add method gives out a new recipe
			//We need to give it a name, then add functionality to it

			//This is the most basic modifier possible
			//When this modifiers gets added to a unit, it will deal 5 damage to it
			//The display name (Light Blow) and description are optional, they're used for UI/UX
			Add("InitDamage", "Light Blow", "Description")
				//The EffectOn.Init means that the effect will be triggered each time the modifier is added
				.Effect(new DamageEffect(5), EffectOn.Init);

			//This is a classic example of a DoT (Damage over time) modifier
			//Once this modifier is added to a unit, it will deal 2 damage to it each second
			//It will also remove itself after 5 seconds
			//The refresh method means that the remove timer will be reset if the modifier is added again
			Add("DoT", "Poison Dart", "Poisons target for 5 seconds, doing damage over time, refreshable")
				//Trigger effect(s) each second
				.Interval(1)
				.Effect(new DamageEffect(2), EffectOn.Interval)
				//Remove after 5 seconds, refresh timer if modifier is added again
				.Remove(5).Refresh();

			Add("FireSlimeSelfDoT", "Fireball", "Burns target for 5 seconds, doing damage over time, refreshable")
				.Interval(1)
				.Effect(new DamageEffect(1), EffectOn.Interval);

			//Here we introduce a new effect, and a chance for the modifier to be applied
			Add("DisarmChance", "Disarm", "Disarms target for 1 second, 20% chance to apply")
				//When applying a modifier (through attacking or casting it)
				//It will have 20% chance to apply the modifier to the unit
				.ApplyChance(0.2f)
				//Disarms (can't attack) the target unit for 1 second when applied
				.Effect(new StatusEffectEffect(StatusEffectType.Disarm, 1f), EffectOn.Init)
				.Remove(1f).Refresh();

			Add("InitHeal", "Healing Touch", "Heals the target")
				.Effect(new HealEffect(5), EffectOn.Init);
		}
	}
}
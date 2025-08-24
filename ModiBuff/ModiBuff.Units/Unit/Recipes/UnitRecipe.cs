using System.Linq;

namespace ModiBuff.Core.Units
{
	public sealed class UnitRecipe
	{
		public int Id { get; }
		public string Name { get; }
		public UnitType UnitType { get; }

		private float _damage;
		private float _health;

		private ModifierAddReference[] _modifierAddReferences;

		private readonly IModifierRecipes _modifierRecipes;

		public UnitRecipe(string name, UnitType unitType, ModifierRecipes modifierRecipes)
		{
			//Id = 
			Name = name;
			UnitType = unitType;
			_modifierRecipes = modifierRecipes;
		}

		//public Unit Create() => new Unit(_health, _damage, _modifierAddReferences, UnitType);

		public UnitRecipe Health(float health)
		{
			_health = health;
			return this;
		}

		public UnitRecipe Damage(float damage)
		{
			_damage = damage;
			return this;
		}

		public UnitRecipe Modifiers(params (string name, ApplierType? applier)[] modifiers)
		{
			_modifierAddReferences = modifiers
				.Select(r => new ModifierAddReference(_modifierRecipes.GetGenerator(r.Item1), r.Item2)).ToArray();
			return this;
		}
	}
}
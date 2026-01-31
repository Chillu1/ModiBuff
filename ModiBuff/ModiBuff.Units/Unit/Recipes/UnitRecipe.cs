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

		private (int Id, ApplierType? ApplierType)[] _modifiers;

		private readonly IModifierRecipes _modifierRecipes;

		public UnitRecipe(string name, UnitType unitType, ModifierRecipes modifierRecipes)
		{
			//Id = 
			Name = name;
			UnitType = unitType;
			_modifierRecipes = modifierRecipes;
		}

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
			_modifiers = modifiers.Select(r => (ModifierIdManager.GetIdByName(r.name).Value, r.applier)).ToArray();
			return this;
		}
	}
}
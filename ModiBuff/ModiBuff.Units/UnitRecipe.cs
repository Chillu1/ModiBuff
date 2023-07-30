namespace ModiBuff.Core.Units
{
	public sealed class UnitRecipe
	{
		public int Id { get; }
		public string Name { get; }

		private float _damage;
		private float _health;

		private ModifierAddReference[] _modifierAddReferences;

		public UnitRecipe(string name)
		{
			//Id = 
			Name = name;
		}

		public Unit Create() => new Unit(_health, _damage, _modifierAddReferences);

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

		public UnitRecipe Modifiers(params ModifierAddReference[] modifierAddReferences)
		{
			_modifierAddReferences = modifierAddReferences;
			return this;
		}
	}
}
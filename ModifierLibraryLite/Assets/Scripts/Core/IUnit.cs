namespace ModifierLibraryLite.Core
{
	public delegate void UnitEvent(IUnit self, IUnit acter);

	public interface IUnit
	{
		float Health { get; }
		float Damage { get; }
		float HealValue { get; }

		void Update(in float deltaTime);
		float Attack(IUnit target);
		float TakeDamage(float damage, IUnit acter, bool triggersEvents = true);
		float Heal(float heal, IUnit acter);
		float Heal(IUnit target);

		void AddDamage(float damage);
		void UseHealth(float value);

		bool TryAddModifier(ModifierRecipe recipe, IUnit target, IUnit sender = null);


		//void TryApplyModifiers(IReadOnlyCollection<ModifierCheck> modifierChecks, IUnit acter);

		//void TryApplyModifiers(IReadOnlyCollection<ModifierRecipe> recipes, IUnit acter);
	}
}
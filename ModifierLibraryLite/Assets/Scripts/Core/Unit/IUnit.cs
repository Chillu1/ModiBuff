namespace ModifierLibraryLite.Core
{
	public delegate void UnitEvent(IUnit self, IUnit acter);

	public interface IUnit
	{
		float Health { get; }
		float Damage { get; }
		float HealValue { get; }
		float Mana { get; }

		void Update(in float deltaTime);
		float Attack(IUnit target);
		float TakeDamage(float damage, IUnit acter, bool triggersEvents = true);
		float Heal(float heal, IUnit acter);
		float Heal(IUnit target);

		void AddDamage(float damage);
		void UseHealth(float value);
		void UseMana(float value);

		bool HasStatusEffect(StatusEffectType statusEffect);
		void ChangeStatusEffect(StatusEffectType statusEffectType, float duration);

		bool TryAddModifier(int id, IUnit target, IUnit acter);


		//void TryApplyModifiers(IReadOnlyCollection<ModifierCheck> modifierChecks, IUnit acter);

		//void TryApplyModifiers(IReadOnlyCollection<ModifierRecipe> recipes, IUnit acter);
		void PrepareRemoveModifier(int id);
	}
}
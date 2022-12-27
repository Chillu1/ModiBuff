namespace ModiBuff.Core
{
	public delegate void UnitEvent(IUnit self, IUnit acter);

	public interface IUnit
	{
		float Health { get; }
		float Damage { get; }
		float HealValue { get; }
		float Mana { get; }

		void Update(float deltaTime);
		float Attack(IUnit target, bool triggersEvents = true);
		float TakeDamage(float damage, IUnit acter, bool triggersEvents = true);
		float Heal(float heal, IUnit acter);
		float Heal(IUnit target);

		void AddDamage(float damage);
		void UseHealth(float value);
		void UseMana(float value);

		void AddEffectEvent(IEffect effect, EffectOnEvent @event);
		void RemoveEffectEvent(IEffect effect, EffectOnEvent @event);

		bool HasStatusEffect(StatusEffectType statusEffect);
		void ChangeStatusEffect(StatusEffectType statusEffectType, float duration);
		void DecreaseStatusEffect(StatusEffectType statusEffectType, float duration);

		bool TryAddModifier(int id, IUnit acter);
		bool TryAddModifierTarget(int id, IUnit target, IUnit acter);


		//void TryApplyModifiers(IReadOnlyCollection<ModifierCheck> modifierChecks, IUnit acter);

		//void TryApplyModifiers(IReadOnlyCollection<ModifierRecipe> recipes, IUnit acter);
		void PrepareRemoveModifier(int id);
	}
}
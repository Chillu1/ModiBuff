namespace ModiBuff.Core
{
	public delegate void UnitEvent(IUnit self, IUnit source);

	public interface IUnit
	{
		float Mana { get; }
		float MaxMana { get; }

		ModifierController ModifierController { get; }

		void Update(float deltaTime);

		float Attack(IUnit target, bool triggersEvents = true);

		void AddDamage(float damage);
		void UseHealth(float value);
		void UseMana(float value);

		void AddEffectEvent(IEffect effect, EffectOnEvent @event);
		void RemoveEffectEvent(IEffect effect, EffectOnEvent @event);

		bool HasLegalAction(LegalAction legalAction);
		bool HasStatusEffect(StatusEffectType statusEffect);
		void ChangeStatusEffect(StatusEffectType statusEffectType, float duration);
		void DecreaseStatusEffect(StatusEffectType statusEffectType, float duration);

		bool TryAddModifier(int id, IUnit source);
		bool TryAddModifierTarget(int id, IUnit target, IUnit source);
	}
}
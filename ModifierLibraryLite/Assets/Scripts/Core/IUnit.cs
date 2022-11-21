namespace ModifierLibraryLite.Core
{
	public interface IUnit
	{
		float Health { get; }
		float Damage { get; }
		float HealValue { get; }

		void Update(in float deltaTime);
		float TakeDamage(float damage, IUnit acter);
		float Heal(float heal, IUnit acter);
		float Heal(IUnit target);

		bool TryAddModifier(Modifier modifier, IUnit target);
	}
}
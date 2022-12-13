using System.Collections.Generic;

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

		void AddDamage(float damage);

		bool TryAddModifier(ModifierRecipe recipe, IUnit target, IUnit sender = null);
		void TryApplyModifiers(IReadOnlyCollection<ModifierCheck> modifierChecks, IUnit acter);
	}
}
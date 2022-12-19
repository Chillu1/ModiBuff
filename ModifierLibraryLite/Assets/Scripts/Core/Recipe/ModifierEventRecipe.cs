using System.Collections.Generic;

namespace ModifierLibraryLite.Core
{
	/// <summary>
	///		High level API for creating modifier event recipe.
	/// </summary>
	/// <example>OnHit, OnAttack, WhenAttacking</example>
	public sealed class ModifierEventRecipe : IModifierRecipe
	{
		public int Id { get; }
		public string Name { get; }

		private readonly List<IEffect> _effects;

		public ModifierEventRecipe()
		{
			_effects = new List<IEffect>();
		}

		public ModifierEventRecipe Remove(float duration)
		{
			//Duration(duration);
			//_removeEffect = new RemoveEffect();
			//Effect(_removeEffect, EffectOn.Duration);
			return this;
		}

		public ModifierEventRecipe Effect(IEffect effect)
		{
			_effects.Add(effect);
			return this;
		}
	}
}
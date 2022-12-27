using System.Collections.Generic;

namespace ModiBuff.Core
{
	/// <summary>
	///		High level API for creating modifier event recipe.
	/// </summary>
	/// <example>OnHit, OnAttack, WhenAttacking</example>
	public sealed class ModifierEventRecipe : IModifierRecipe
	{
		public int Id { get; }
		public string Name { get; }
		public bool HasChecks { get; }

		private readonly EffectOnEvent _effectOnEvent;

		private readonly List<IEffect> _effects;

		public ModifierEventRecipe(string name, EffectOnEvent effectOnEvent)
		{
			Id = ModifierIdManager.GetFreeId(name);
			Name = name;
			_effectOnEvent = effectOnEvent;

			_effects = new List<IEffect>(2);
		}

		Modifier IModifierRecipe.Create()
		{
			var eventEffect = new EventEffect((IEffect)((IShallowClone)_effects[0]).ShallowClone(), _effectOnEvent);
			var initComponent = new InitComponent(true, eventEffect, null);
			//If has remove duration, durationComp

			return new Modifier(Id, Name, initComponent, null, null, null);
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

		ModifierCheck IModifierRecipe.CreateApplyCheck()
		{
			return null;
		}

		public void Finish()
		{
		}
	}
}
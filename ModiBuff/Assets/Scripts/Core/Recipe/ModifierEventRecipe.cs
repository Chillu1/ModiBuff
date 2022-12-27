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

		private float _removeDuration;
		private EffectWrapper _removeEffectWrapper;

		private bool _refreshDuration;

		public ModifierEventRecipe(string name, EffectOnEvent effectOnEvent)
		{
			Id = ModifierIdManager.GetFreeId(name);
			Name = name;
			_effectOnEvent = effectOnEvent;

			_effects = new List<IEffect>(2);
		}

		//---PostFinish---

		Modifier IModifierRecipe.Create()
		{
			//TODO Clone stateful
			var revertList = new List<IRevertEffect>();
			var eventEffect = new EventEffect(_effects[0], _effectOnEvent);
			var initComponent = new InitComponent(true, eventEffect, null);

			ITimeComponent[] timeComponents = null;
			if (_removeDuration > 0)
			{
				timeComponents = new ITimeComponent[]
				{
					new DurationComponent(_removeDuration, _refreshDuration, _removeEffectWrapper.GetEffect())
				};
			}

			if (eventEffect is IRevertEffect revertEffect && revertEffect.IsRevertible)
				revertList.Add(revertEffect);

			if (_removeEffectWrapper != null)
			{
				((RemoveEffect)_removeEffectWrapper.GetEffect()).SetRevertibleEffects(revertList.ToArray());
				_removeEffectWrapper.Reset();
			}

			return new Modifier(Id, Name, initComponent, timeComponents, null, null);
		}

		//---Actions---

		public ModifierEventRecipe Remove(float duration)
		{
			_removeDuration = duration;
			_removeEffectWrapper = new EffectWrapper(new RemoveEffect(), EffectOn.Duration);
			return this;
		}

		public ModifierEventRecipe Refresh()
		{
			_refreshDuration = true;
			return this;
		}

		//---Effects---

		public ModifierEventRecipe Effect(IEffect effect)
		{
			_effects.Add(effect);
			return this;
		}

		ModifierCheck IModifierRecipe.CreateApplyCheck() => throw new System.NotImplementedException();

		public void Finish()
		{
		}
	}
}
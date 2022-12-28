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

		private readonly List<EffectWrapper> _effects;

		private float _removeDuration;
		private EffectWrapper _removeEffectWrapper;

		private bool _refreshDuration;

		private readonly List<IRevertEffect> _revertEffects;

		public ModifierEventRecipe(string name, EffectOnEvent effectOnEvent)
		{
			Id = ModifierIdManager.GetFreeId(name);
			Name = name;
			_effectOnEvent = effectOnEvent;

			_effects = new List<EffectWrapper>(2);
			_revertEffects = new List<IRevertEffect>(2);
		}

		//---PostFinish---

		Modifier IModifierRecipe.Create()
		{
			var effects = new IEffect[_effects.Count];
			for (int i = 0; i < _effects.Count; i++)
				effects[i] = _effects[i].GetEffect();

			var eventEffect = new EventEffect(effects, _effectOnEvent);
			var initComponent = new InitComponent(true, eventEffect, null);

			ITimeComponent[] timeComponents = null;
			if (_removeDuration > 0)
			{
				timeComponents = new ITimeComponent[]
				{
					new DurationComponent(_removeDuration, _refreshDuration, _removeEffectWrapper.GetEffect())
				};
			}

			//TODO Do we want to be able to revert the effects inside the event as well?
			if (eventEffect is IRevertEffect revertEffect && revertEffect.IsRevertible)
				_revertEffects.Add(revertEffect);

			if (_removeEffectWrapper != null)
			{
				((RemoveEffect)_removeEffectWrapper.GetEffect()).SetRevertibleEffects(_revertEffects.ToArray());
				_removeEffectWrapper.Reset();
			}

			for (int i = 0; i < _effects.Count; i++)
				_effects[i].Reset();
			_revertEffects.Clear();

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
			if (effect is IEventTrigger eventTrigger)
				eventTrigger.SetEventBased();

			_effects.Add(new EffectWrapper(effect, EffectOn.Init));
			return this;
		}

		ModifierCheck IModifierRecipe.CreateApplyCheck() => throw new System.NotImplementedException();

		public void Finish()
		{
		}
	}
}
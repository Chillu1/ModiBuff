using System;
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

		private readonly object _effectOnEvent;
		private readonly EventEffectFactory _eventEffectFunc;

		private readonly List<EffectWrapper> _effects;

		private TagType _tag;

		private float _removeDuration;
		private EffectWrapper _removeEffectWrapper;

		private bool _refreshDuration;

		public ModifierEventRecipe(int id, string name, object effectOnEvent, EventEffectFactory eventEffectFunc)
		{
			Id = id;
			Name = name;
			_effectOnEvent = effectOnEvent;
			_eventEffectFunc = eventEffectFunc;

			_effects = new List<EffectWrapper>(2);
		}

		public ModifierEventRecipe Tag(int tag)
		{
			_tag = (TagType)tag;
			return this;
		}

		//---Actions---

		public ModifierEventRecipe Remove(float duration)
		{
			_removeDuration = duration;
			_removeEffectWrapper = new EffectWrapper(new RemoveEffect(Id), EffectOn.Duration);
			return this;
		}

		public ModifierEventRecipe Refresh()
		{
			_refreshDuration = true;
			return this;
		}

		//---Effects---

		public ModifierEventRecipe Effect(IEffect effect, Targeting targeting = Targeting.TargetSource)
		{
			if (effect is ITargetEffect effectTarget)
				effectTarget.SetTargeting(targeting);

			_effects.Add(new EffectWrapper(effect, EffectOn.Init));
			return this;
		}

		public IModifierGenerator CreateModifierGenerator()
		{
			_tag |= TagType.IsInit;
			if (_refreshDuration)
				_tag |= TagType.IsRefresh;

			return new ModifierEventGenerator(Id, Name, _effectOnEvent, _eventEffectFunc, _effects, _removeDuration,
				_removeEffectWrapper,
				_refreshDuration);
		}

		public ModifierInfo CreateModifierInfo()
		{
			return new ModifierInfo(Id, Name, Name, "");
		}

		public TagType GetTag() => _tag;
	}
}
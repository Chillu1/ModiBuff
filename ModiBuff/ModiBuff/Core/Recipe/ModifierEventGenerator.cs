using System.Collections.Generic;

namespace ModiBuff.Core
{
	public sealed class ModifierEventGenerator : IModifierGenerator
	{
		public int Id { get; }
		public int GenId { get; private set; }
		public string Name { get; }

		private readonly object _effectOnEvent;
		private readonly EventEffectFactory _eventEffectFunc;

		private readonly List<EffectWrapper> _effects;

		private readonly float _removeDuration;
		private readonly EffectWrapper _removeEffectWrapper;

		private readonly bool _refreshDuration;

		private readonly List<IRevertEffect> _revertEffects;

		public ModifierEventGenerator(int id, string name, object effectOnEvent, EventEffectFactory eventEffectFunc,
			List<EffectWrapper> effects, float removeDuration, EffectWrapper removeEffectWrapper, bool refreshDuration)
		{
			Id = id;
			Name = name;

			_effectOnEvent = effectOnEvent;
			_eventEffectFunc = eventEffectFunc;

			_effects = effects;

			_removeDuration = removeDuration;
			_removeEffectWrapper = removeEffectWrapper;

			_refreshDuration = refreshDuration;

			_revertEffects = new List<IRevertEffect>(2);
		}

		Modifier IModifierGenerator.Create()
		{
			var effects = new IEffect[_effects.Count];
			for (int i = 0; i < _effects.Count; i++)
				effects[i] = _effects[i].GetEffect();

			var eventEffect = _eventEffectFunc(effects, _effectOnEvent);
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

			return new Modifier(Id, GenId++, Name, initComponent, timeComponents, null, null, new SingleTargetComponent());
		}
	}
}
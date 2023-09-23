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

		private readonly EffectWrapper[] _effects;

		private readonly float _removeDuration;
		private readonly EffectWrapper _removeEffectWrapper;

		private readonly bool _refreshDuration;

		private readonly bool _hasRevertEffects;

		public ModifierEventGenerator(int id, string name, object effectOnEvent, EventEffectFactory eventEffectFunc,
			List<EffectWrapper> effects, float removeDuration, EffectWrapper removeEffectWrapper, bool refreshDuration)
		{
			Id = id;
			Name = name;

			_effectOnEvent = effectOnEvent;
			_eventEffectFunc = eventEffectFunc;

			_effects = effects.ToArray();

			_removeDuration = removeDuration;
			_removeEffectWrapper = removeEffectWrapper;

			_refreshDuration = refreshDuration;

			if (_eventEffectFunc(new IEffect[0], _effectOnEvent) is IRevertEffect revertEffect && revertEffect.IsRevertible)
				_hasRevertEffects = true;
		}

		Modifier IModifierGenerator.Create()
		{
			int effectsLength = _effects.Length;
			var effects = new IEffect[effectsLength];
			for (int i = 0; i < effectsLength; i++)
				effects[i] = _effects[i].GetEffect();

			var eventEffect = _eventEffectFunc(effects, _effectOnEvent);
			var initComponent = new InitComponent(true, new[] { eventEffect }, null);

			IRealTimeComponent[] timeComponents = null;
			if (_removeDuration > 0)
			{
				timeComponents = new IRealTimeComponent[]
				{
					new DurationComponent(_removeDuration, _refreshDuration, new[] { _removeEffectWrapper.GetEffect() })
				};
			}

			if (_removeEffectWrapper != null)
			{
				//TODO Do we want to be able to revert the effects inside the event as well?
				if (_hasRevertEffects)
					((RemoveEffect)_removeEffectWrapper.GetEffect()).SetRevertibleEffects(new[] { (IRevertEffect)eventEffect });
				_removeEffectWrapper.Reset();
			}

			for (int i = 0; i < effectsLength; i++)
				_effects[i].Reset();

			return new Modifier(Id, GenId++, Name, initComponent, timeComponents, default, null, new SingleTargetComponent());
		}
	}
}
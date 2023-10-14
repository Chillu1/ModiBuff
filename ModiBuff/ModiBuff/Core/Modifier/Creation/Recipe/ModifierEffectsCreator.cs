using System.Collections.Generic;

namespace ModiBuff.Core
{
	/// <summary>
	///		Class responsible for handling the correct cloning and sync of modifiers
	/// </summary>
	public sealed class ModifierEffectsCreator
	{
		private readonly EffectWrapper[] _effectWrappers;
		private readonly EffectWrapper[] _effectsWithModifierInfoWrappers;
		private readonly EffectWrapper _removeEffectWrapper;
		private readonly EffectWrapper _eventRegisterWrapper;
		private readonly EffectWrapper _callbackRegisterWrapper;

		private IRevertEffect[] _revertEffects;
		private IEffect[] _initEffects;
		private IEffect[] _intervalEffects;
		private IEffect[] _durationEffects;
		private IStackEffect[] _stackEffects;
		private IEffect[] _callbacks;
		private IEffect[] _eventEffects;

		private int _revertEffectsIndex,
			_initEffectsIndex,
			_intervalEffectsIndex,
			_durationEffectsIndex,
			_stackEffectsIndex,
			_callbackEffectsIndex,
			_eventEffectsIndex;

		public ModifierEffectsCreator(List<EffectWrapper> effectWrappers, EffectWrapper removeEffectWrapper,
			EffectWrapper eventRegisterWrapper, EffectWrapper callbackRegisterWrapper)
		{
			var effectsWithModifierInfoWrappers = new List<EffectWrapper>();
			_effectWrappers = effectWrappers.ToArray();
			_removeEffectWrapper = removeEffectWrapper;
			_eventRegisterWrapper = eventRegisterWrapper;
			_callbackRegisterWrapper = callbackRegisterWrapper;

			for (int i = 0; i < _effectWrappers.Length; i++)
			{
				var effectWrapper = _effectWrappers[i];

				if (effectWrapper.GetEffect() is IModifierStateInfo)
					effectsWithModifierInfoWrappers.Add(effectWrapper);

				if (effectWrapper.GetEffect() is IRevertEffect revertEffect && revertEffect.IsRevertible)
					_revertEffectsIndex++;

				if ((effectWrapper.EffectOn & EffectOn.Init) != 0)
					_initEffectsIndex++;
				if ((effectWrapper.EffectOn & EffectOn.Interval) != 0)
					_intervalEffectsIndex++;
				if ((effectWrapper.EffectOn & EffectOn.Duration) != 0)
					_durationEffectsIndex++;
				if ((effectWrapper.EffectOn & EffectOn.Stack) != 0)
					_stackEffectsIndex++;
				if ((effectWrapper.EffectOn & EffectOn.Callback) != 0)
					_callbackEffectsIndex++;
				if ((effectWrapper.EffectOn & EffectOn.Event) != 0)
					_eventEffectsIndex++;
			}

			_effectsWithModifierInfoWrappers = effectsWithModifierInfoWrappers.ToArray();
		}

		public SyncedModifierEffects Create(int genId)
		{
			//Setup all arrays of possible effects on effectOns
			if (_initEffectsIndex > 0)
			{
				_initEffects = new IEffect[_initEffectsIndex];
				_initEffectsIndex = 0;
			}

			if (_intervalEffectsIndex > 0)
			{
				_intervalEffects = new IEffect[_intervalEffectsIndex];
				_intervalEffectsIndex = 0;
			}

			if (_durationEffectsIndex > 0)
			{
				_durationEffects = new IEffect[_durationEffectsIndex];
				_durationEffectsIndex = 0;
			}

			if (_stackEffectsIndex > 0)
			{
				_stackEffects = new IStackEffect[_stackEffectsIndex];
				_stackEffectsIndex = 0;
			}

			if (_callbackEffectsIndex > 0)
			{
				_callbacks = new IEffect[_callbackEffectsIndex];
				_callbackEffectsIndex = 0;
			}

			if (_eventEffectsIndex > 0)
			{
				_eventEffects = new IEffect[_eventEffectsIndex];
				_eventEffectsIndex = 0;
			}

			if (_revertEffectsIndex > 0)
			{
				_revertEffects = new IRevertEffect[_revertEffectsIndex];
				_revertEffectsIndex = 0;
			}

			//Go over all of the effects, and put them into the correct arrays
			//Here we're also responsible for cloning, and feeding them the correct genId
			for (int i = 0; i < _effectWrappers.Length; i++)
			{
				var effectWrapper = _effectWrappers[i];
				var effect = effectWrapper.GetEffect();
				var effectOn = effectWrapper.EffectOn;

				if (effect is IModifierGenIdOwner modifierGenIdOwner)
					modifierGenIdOwner.SetGenId(genId);
				if (effect is IRevertEffect revertEffect && revertEffect.IsRevertible)
					_revertEffects[_revertEffectsIndex++] = revertEffect;

				if ((effectOn & EffectOn.Init) != 0)
					_initEffects[_initEffectsIndex++] = effect;
				if ((effectOn & EffectOn.Interval) != 0)
					_intervalEffects[_intervalEffectsIndex++] = effect;
				if ((effectOn & EffectOn.Duration) != 0)
					_durationEffects[_durationEffectsIndex++] = effect;
				if ((effectOn & EffectOn.Stack) != 0)
					_stackEffects[_stackEffectsIndex++] = (IStackEffect)effect;
				if ((effectOn & EffectOn.Callback) != 0)
					_callbacks[_callbackEffectsIndex++] = effect;
				if ((effectOn & EffectOn.Event) != 0)
					_eventEffects[_eventEffectsIndex++] = effect;
			}

			ModifierStateInfo modifierStateInfo = null;
			if (_effectsWithModifierInfoWrappers.Length > 0)
			{
				var modifierStateInfoEffects = new IModifierStateInfo[_effectsWithModifierInfoWrappers.Length];
				for (int i = 0; i < _effectsWithModifierInfoWrappers.Length; i++)
					modifierStateInfoEffects[i] = (IModifierStateInfo)_effectsWithModifierInfoWrappers[i].GetEffect();
				modifierStateInfo = new ModifierStateInfo(modifierStateInfoEffects);
			}

			//Set the effects arrays on our special effects (callback, event, remove-revert)
			if (_eventRegisterWrapper != null)
			{
				((IRecipeFeedEffects)_eventRegisterWrapper.GetEffect()).SetEffects(_eventEffects);
				_eventRegisterWrapper.Reset();
			}

			if (_callbackRegisterWrapper != null)
			{
				((IRecipeFeedEffects)_callbackRegisterWrapper.GetEffect()).SetEffects(_callbacks);
				_callbackRegisterWrapper.Reset();
			}

			if (_removeEffectWrapper != null)
			{
				if (_revertEffects != null)
					((RemoveEffect)_removeEffectWrapper.GetEffect()).SetRevertibleEffects(_revertEffects);
				_removeEffectWrapper.Reset();
			}

			//Reset all the clones in wrappers
			for (int i = 0; i < _effectWrappers.Length; i++)
				_effectWrappers[i].Reset();

			return new SyncedModifierEffects(_initEffects, _intervalEffects, _durationEffects, _stackEffects,
				modifierStateInfo);
		}
	}

	public readonly ref struct SyncedModifierEffects
	{
		public readonly IEffect[] InitEffects;
		public readonly IEffect[] IntervalEffects;
		public readonly IEffect[] DurationEffects;
		public readonly IStackEffect[] StackEffects;
		public readonly ModifierStateInfo ModifierStateInfo;

		public SyncedModifierEffects(IEffect[] initEffectsArray, IEffect[] intervalEffectsArray,
			IEffect[] durationEffectsArray, IStackEffect[] stackEffectsArray,
			ModifierStateInfo modifierStateInfo)
		{
			InitEffects = initEffectsArray;
			IntervalEffects = intervalEffectsArray;
			DurationEffects = durationEffectsArray;
			StackEffects = stackEffectsArray;
			ModifierStateInfo = modifierStateInfo;
		}
	}
}
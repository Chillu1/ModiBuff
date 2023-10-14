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
		private readonly EffectWrapper _callbackRegisterWrapper;
		private readonly EffectWrapper _eventRegisterWrapper;

		private readonly int _revertEffectsCount,
			_initEffectsCount,
			_intervalEffectsCount,
			_durationEffectsCount,
			_stackEffectsCount,
			_callbackEffectsCount,
			_eventEffectsCount;

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
			EffectWrapper callbackRegisterWrapper, EffectWrapper eventRegisterWrapper)
		{
			var effectsWithModifierInfoWrappers = new List<EffectWrapper>();
			_effectWrappers = effectWrappers.ToArray();
			_removeEffectWrapper = removeEffectWrapper;
			_callbackRegisterWrapper = callbackRegisterWrapper;
			_eventRegisterWrapper = eventRegisterWrapper;

			if (_removeEffectWrapper != null)
			{
				//Probably never a thing, but added just in case
				if ((_removeEffectWrapper.EffectOn & EffectOn.Init) != 0)
					_initEffectsCount++;
				if ((_removeEffectWrapper.EffectOn & EffectOn.Interval) != 0)
					_intervalEffectsCount++;
				if ((_removeEffectWrapper.EffectOn & EffectOn.Duration) != 0)
					_durationEffectsCount++;
				if ((_removeEffectWrapper.EffectOn & EffectOn.Callback) != 0)
					_callbackEffectsCount++;
				if ((_removeEffectWrapper.EffectOn & EffectOn.Event) != 0)
					_eventEffectsCount++;
			}

			if (callbackRegisterWrapper?.GetEffect() is IRevertEffect callbackRevert && callbackRevert.IsRevertible)
				_revertEffectsCount++;

			for (int i = 0; i < _effectWrappers.Length; i++)
			{
				var effectWrapper = _effectWrappers[i];

				if (effectWrapper.GetEffect() is IModifierStateInfo)
					effectsWithModifierInfoWrappers.Add(effectWrapper);

				if (effectWrapper.GetEffect() is IRevertEffect revertEffect && revertEffect.IsRevertible)
					_revertEffectsCount++;

				if ((effectWrapper.EffectOn & EffectOn.Init) != 0)
					_initEffectsCount++;
				if ((effectWrapper.EffectOn & EffectOn.Interval) != 0)
					_intervalEffectsCount++;
				if ((effectWrapper.EffectOn & EffectOn.Duration) != 0)
					_durationEffectsCount++;
				if ((effectWrapper.EffectOn & EffectOn.Stack) != 0)
					_stackEffectsCount++;
				if ((effectWrapper.EffectOn & EffectOn.Callback) != 0)
					_callbackEffectsCount++;
				if ((effectWrapper.EffectOn & EffectOn.Event) != 0)
					_eventEffectsCount++;
			}

			_effectsWithModifierInfoWrappers = effectsWithModifierInfoWrappers.ToArray();
		}

		public SyncedModifierEffects Create(int genId)
		{
			if (_initEffectsCount > 0)
			{
				_initEffectsIndex = 0;
				_initEffects = new IEffect[_initEffectsCount];
			}

			if (_intervalEffectsCount > 0)
			{
				_intervalEffectsIndex = 0;
				_intervalEffects = new IEffect[_intervalEffectsCount];
			}

			if (_durationEffectsCount > 0)
			{
				_durationEffectsIndex = 0;
				_durationEffects = new IEffect[_durationEffectsCount];
			}

			if (_stackEffectsCount > 0)
			{
				_stackEffectsIndex = 0;
				_stackEffects = new IStackEffect[_stackEffectsCount];
			}

			if (_callbackEffectsCount > 0)
			{
				_callbacks = new IEffect[_callbackEffectsCount];
				_callbackEffectsIndex = 0;
			}

			if (_eventEffectsCount > 0)
			{
				_eventEffects = new IEffect[_eventEffectsCount];
				_eventEffectsIndex = 0;
			}

			if (_revertEffectsCount > 0)
			{
				_revertEffectsIndex = 0;
				_revertEffects = new IRevertEffect[_revertEffectsCount];
			}

			if (_removeEffectWrapper != null)
			{
				_removeEffectWrapper.UpdateGenId(genId);

				//Probably never a thing, but added just in case
				if ((_removeEffectWrapper.EffectOn & EffectOn.Init) != 0)
					_initEffects[_initEffectsIndex++] = _removeEffectWrapper.GetEffect();
				if ((_removeEffectWrapper.EffectOn & EffectOn.Interval) != 0)
					_intervalEffects[_intervalEffectsIndex++] = _removeEffectWrapper.GetEffect();
				if ((_removeEffectWrapper.EffectOn & EffectOn.Duration) != 0)
					_durationEffects[_durationEffectsIndex++] = _removeEffectWrapper.GetEffect();
			}

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

			if (_callbackRegisterWrapper != null)
			{
				//Manually register remove effect wrapper if it's EffectOn is Callback
				//Since remove effect should never be fed as a normal effect wrapper (it's a special case)
				if (_removeEffectWrapper != null && _removeEffectWrapper.EffectOn.HasFlag(EffectOn.Callback))
					_callbacks[_callbackEffectsIndex++] = _removeEffectWrapper.GetEffect();
				if (_callbackRegisterWrapper.GetEffect() is IRevertEffect revertEffect && revertEffect.IsRevertible)
					_revertEffects[_revertEffectsIndex++] = revertEffect;
				((IRecipeFeedEffects)_callbackRegisterWrapper.GetEffect()).SetEffects(_callbacks);
				_callbackRegisterWrapper.Reset();
				_callbacks = null;
			}

			if (_eventRegisterWrapper != null)
			{
				if (_removeEffectWrapper != null && _removeEffectWrapper.EffectOn.HasFlag(EffectOn.Event))
					_eventEffects[_eventEffectsIndex++] = _removeEffectWrapper.GetEffect();
				((IRecipeFeedEffects)_eventRegisterWrapper.GetEffect()).SetEffects(_eventEffects);
				_eventRegisterWrapper.Reset();
				_eventEffects = null;
			}

			if (_removeEffectWrapper != null)
			{
				if (_revertEffects != null)
					((RemoveEffect)_removeEffectWrapper.GetEffect()).SetRevertibleEffects(_revertEffects);
				_removeEffectWrapper.Reset();
			}

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
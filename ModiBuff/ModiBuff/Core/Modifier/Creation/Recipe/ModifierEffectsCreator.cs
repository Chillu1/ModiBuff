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
		private readonly EffectWrapper _dispelRegisterWrapper;
		private readonly EffectWrapper _eventRegisterWrapper;
		private readonly EffectWrapper _callbackUnitRegisterWrapper;
		private readonly EffectWrapper _callbackEffectRegisterWrapper;

		private IRevertEffect[] _revertEffects;
		private IEffect[] _initEffects;
		private IEffect[] _intervalEffects;
		private IEffect[] _durationEffects;
		private IStackEffect[] _stackEffects;
		private IEffect[] _eventEffects;
		private IEffect[] _callbackUnitEffects;
		private IEffect[] _callbackEffectEffects;

		private int _revertEffectsIndex,
			_initEffectsIndex,
			_intervalEffectsIndex,
			_durationEffectsIndex,
			_stackEffectsIndex,
			_eventEffectsIndex,
			_callbackUnitEffectsIndex,
			_callbackEffectEffectsIndex;

		public ModifierEffectsCreator(List<EffectWrapper> effectWrappers, EffectWrapper removeEffectWrapper,
			EffectWrapper dispelRegisterWrapper, EffectWrapper eventRegisterWrapper,
			EffectWrapper callbackUnitRegisterWrapper, EffectWrapper callbackEffectRegisterWrapper)
		{
			var effectsWithModifierInfoWrappers = new List<EffectWrapper>();
			_effectWrappers = effectWrappers.ToArray();
			_removeEffectWrapper = removeEffectWrapper;
			_dispelRegisterWrapper = dispelRegisterWrapper;
			_eventRegisterWrapper = eventRegisterWrapper;
			_callbackUnitRegisterWrapper = callbackUnitRegisterWrapper;
			_callbackEffectRegisterWrapper = callbackEffectRegisterWrapper;

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
				if ((effectWrapper.EffectOn & EffectOn.Event) != 0)
					_eventEffectsIndex++;
				if ((effectWrapper.EffectOn & EffectOn.CallbackUnit) != 0)
					_callbackUnitEffectsIndex++;
				if ((effectWrapper.EffectOn & EffectOn.CallbackEffect) != 0)
					_callbackEffectEffectsIndex++;
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

			if (_eventEffectsIndex > 0)
			{
				_eventEffects = new IEffect[_eventEffectsIndex];
				_eventEffectsIndex = 0;
			}

			if (_callbackUnitEffectsIndex > 0)
			{
				_callbackUnitEffects = new IEffect[_callbackUnitEffectsIndex];
				_callbackUnitEffectsIndex = 0;
			}

			if (_callbackEffectEffectsIndex > 0)
			{
				_callbackEffectEffects = new IEffect[_callbackEffectEffectsIndex];
				_callbackEffectEffectsIndex = 0;
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
				if ((effectOn & EffectOn.Event) != 0)
					_eventEffects[_eventEffectsIndex++] = effect;
				if ((effectOn & EffectOn.CallbackUnit) != 0)
					_callbackUnitEffects[_callbackUnitEffectsIndex++] = effect;
				if ((effectOn & EffectOn.CallbackEffect) != 0)
					_callbackEffectEffects[_callbackEffectEffectsIndex++] = effect;
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
			//No need to reset manually special wrappers manually
			//Since they're always fed to effectWrappers, that we reset at the end
			_eventRegisterWrapper?.GetEffectAs<IRecipeFeedEffects>().SetEffects(_eventEffects);
			_callbackUnitRegisterWrapper?.GetEffectAs<IRecipeFeedEffects>().SetEffects(_callbackUnitEffects);
			_callbackEffectRegisterWrapper?.GetEffectAs<IRecipeFeedEffects>().SetEffects(_callbackEffectEffects);

			if (_removeEffectWrapper != null && _revertEffects != null)
				_removeEffectWrapper.GetEffectAs<RemoveEffect>().SetRevertibleEffects(_revertEffects);

			_dispelRegisterWrapper?.GetEffectAs<DispelRegisterEffect>()
				.SetRemoveEffect(_removeEffectWrapper.GetEffectAs<RemoveEffect>());

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
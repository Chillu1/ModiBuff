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
		private readonly EffectWrapper[] _savableEffectsWrappers;
		private readonly EffectWrapper _removeEffectWrapper;
		private readonly EffectWrapper _dispelRegisterWrapper;
		private readonly EffectWrapper _eventRegisterWrapper;
		private readonly EffectWrapper _callbackUnitRegisterWrapper;
		private readonly EffectWrapper[] _callbackEffectRegisterWrappers;
		private readonly EffectWrapper _callbackEffectUnitsRegisterWrapper;

		private IRevertEffect[] _revertEffects;
		private IEffect[] _initEffects;
		private IEffect[] _intervalEffects;
		private IEffect[] _durationEffects;
		private IStackEffect[] _stackEffects;
		private IEffect[] _eventEffects;
		private IEffect[] _callbackUnitEffects;
		private IEffect[][] _callbackEffectEffects;
		private IEffect[] _callbackEffectUnitsEffects;

		private int _revertEffectsIndex,
			_initEffectsIndex,
			_intervalEffectsIndex,
			_durationEffectsIndex,
			_stackEffectsIndex,
			_eventEffectsIndex,
			_callbackUnitEffectsIndex,
			_callbackEffectEffectsIndex,
			_callbackEffectUnitsEffectsIndex,
			_callbackEffectEffectsIndex2,
			_callbackEffectEffectsIndex3,
			_callbackEffectEffectsIndex4;

		public ModifierEffectsCreator(List<EffectWrapper> effectWrappers, EffectWrapper removeEffectWrapper,
			EffectWrapper dispelRegisterWrapper, EffectWrapper eventRegisterWrapper,
			EffectWrapper callbackUnitRegisterWrapper, EffectWrapper[] callbackEffectRegisterWrappers,
			EffectWrapper callbackEffectUnitsRegisterWrapper)
		{
			var effectsWithModifierInfoWrappers = new List<EffectWrapper>();
			var savableEffectsWrappers = new List<EffectWrapper>();
			_effectWrappers = effectWrappers.ToArray();
			_removeEffectWrapper = removeEffectWrapper;
			_dispelRegisterWrapper = dispelRegisterWrapper;
			_eventRegisterWrapper = eventRegisterWrapper;
			_callbackUnitRegisterWrapper = callbackUnitRegisterWrapper;
			_callbackEffectRegisterWrappers = callbackEffectRegisterWrappers;
			_callbackEffectUnitsRegisterWrapper = callbackEffectUnitsRegisterWrapper;

			for (int i = 0; i < _effectWrappers.Length; i++)
			{
				var effectWrapper = _effectWrappers[i];

				if (effectWrapper.GetEffect() is IEffectStateInfo)
					effectsWithModifierInfoWrappers.Add(effectWrapper);
				if (effectWrapper.GetEffect() is ISavable)
					savableEffectsWrappers.Add(effectWrapper);

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
				if ((effectWrapper.EffectOn & EffectOn.CallbackEffectUnits) != 0)
					_callbackEffectUnitsEffectsIndex++;
				if ((effectWrapper.EffectOn & EffectOn.CallbackEffect2) != 0)
					_callbackEffectEffectsIndex2++;
				if ((effectWrapper.EffectOn & EffectOn.CallbackEffect3) != 0)
					_callbackEffectEffectsIndex3++;
				if ((effectWrapper.EffectOn & EffectOn.CallbackEffect4) != 0)
					_callbackEffectEffectsIndex4++;
			}

			_effectsWithModifierInfoWrappers = effectsWithModifierInfoWrappers.ToArray();
			_savableEffectsWrappers = savableEffectsWrappers.ToArray();
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

			if (_callbackEffectEffectsIndex > 0 || _callbackEffectEffectsIndex2 > 0 ||
			    _callbackEffectEffectsIndex3 > 0 || _callbackEffectEffectsIndex4 > 0)
				_callbackEffectEffects = new IEffect[4][];

			if (_callbackEffectEffectsIndex > 0)
			{
				_callbackEffectEffects[0] = new IEffect[_callbackEffectEffectsIndex];
				_callbackEffectEffectsIndex = 0;
			}

			if (_callbackEffectUnitsEffectsIndex > 0)
			{
				_callbackEffectUnitsEffects = new IEffect[_callbackEffectUnitsEffectsIndex];
				_callbackEffectUnitsEffectsIndex = 0;
			}

			if (_callbackEffectEffectsIndex2 > 0)
			{
				_callbackEffectEffects[1] = new IEffect[_callbackEffectEffectsIndex2];
				_callbackEffectEffectsIndex2 = 0;
			}

			if (_callbackEffectEffectsIndex3 > 0)
			{
				_callbackEffectEffects[2] = new IEffect[_callbackEffectEffectsIndex3];
				_callbackEffectEffectsIndex3 = 0;
			}

			if (_callbackEffectEffectsIndex4 > 0)
			{
				_callbackEffectEffects[3] = new IEffect[_callbackEffectEffectsIndex4];
				_callbackEffectEffectsIndex4 = 0;
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
					_callbackEffectEffects[0][_callbackEffectEffectsIndex++] = effect;
				if ((effectOn & EffectOn.CallbackEffectUnits) != 0)
					_callbackEffectUnitsEffects[_callbackEffectUnitsEffectsIndex++] = effect;
				if ((effectOn & EffectOn.CallbackEffect2) != 0)
					_callbackEffectEffects[1][_callbackEffectEffectsIndex2++] = effect;
				if ((effectOn & EffectOn.CallbackEffect3) != 0)
					_callbackEffectEffects[2][_callbackEffectEffectsIndex3++] = effect;
				if ((effectOn & EffectOn.CallbackEffect4) != 0)
					_callbackEffectEffects[3][_callbackEffectEffectsIndex4++] = effect;
			}

			EffectStateInfo effectStateInfo = default;
			if (_effectsWithModifierInfoWrappers.Length > 0)
			{
				var modifierStateInfoEffects = new IEffectStateInfo[_effectsWithModifierInfoWrappers.Length];
				for (int i = 0; i < _effectsWithModifierInfoWrappers.Length; i++)
					modifierStateInfoEffects[i] = (IEffectStateInfo)_effectsWithModifierInfoWrappers[i].GetEffect();
				effectStateInfo = new EffectStateInfo(modifierStateInfoEffects);
			}

			EffectSaveState effectSaveState = default;
			if (_savableEffectsWrappers.Length > 0)
			{
				var savableEffects = new ISavable[_savableEffectsWrappers.Length];
				for (int i = 0; i < _savableEffectsWrappers.Length; i++)
					savableEffects[i] = (ISavable)_savableEffectsWrappers[i].GetEffect();
				effectSaveState = new EffectSaveState(savableEffects);
			}

			//Set the effects arrays on our special effects (callback, event, remove-revert)
			//No need to reset manually special wrappers manually
			//Since they're always fed to effectWrappers, that we reset at the end
			_eventRegisterWrapper?.GetEffectAs<IRecipeFeedEffects>().SetEffects(_eventEffects);
			_callbackUnitRegisterWrapper?.GetEffectAs<IRecipeFeedEffects>().SetEffects(_callbackUnitEffects);
			for (int i = 0; i < _callbackEffectRegisterWrappers?.Length; i++)
				_callbackEffectRegisterWrappers[i].GetEffectAs<IRecipeFeedEffects>()
					.SetEffects(_callbackEffectEffects[i]);

			_callbackEffectUnitsRegisterWrapper?.GetEffectAs<IRecipeFeedEffects>()
				.SetEffects(_callbackEffectUnitsEffects);

			if (_removeEffectWrapper != null && _revertEffects != null)
				_removeEffectWrapper.GetEffectAs<RemoveEffect>().SetRevertibleEffects(_revertEffects);

			_dispelRegisterWrapper?.GetEffectAs<DispelRegisterEffect>()
				.SetRemoveEffect(_removeEffectWrapper.GetEffectAs<RemoveEffect>());

			//Reset all the clones in wrappers
			for (int i = 0; i < _effectWrappers.Length; i++)
				_effectWrappers[i].Reset();

			return new SyncedModifierEffects(_initEffects, _intervalEffects, _durationEffects, _stackEffects,
				effectStateInfo, effectSaveState);
		}
	}

	public readonly ref struct SyncedModifierEffects
	{
		public readonly IEffect[] InitEffects;
		public readonly IEffect[] IntervalEffects;
		public readonly IEffect[] DurationEffects;
		public readonly IStackEffect[] StackEffects;
		public readonly EffectStateInfo EffectStateInfo;
		public readonly EffectSaveState EffectSaveState;

		public SyncedModifierEffects(IEffect[] initEffectsArray, IEffect[] intervalEffectsArray,
			IEffect[] durationEffectsArray, IStackEffect[] stackEffectsArray,
			EffectStateInfo effectStateInfo, EffectSaveState effectSaveState)
		{
			InitEffects = initEffectsArray;
			IntervalEffects = intervalEffectsArray;
			DurationEffects = durationEffectsArray;
			StackEffects = stackEffectsArray;
			EffectStateInfo = effectStateInfo;
			EffectSaveState = effectSaveState;
		}
	}
}
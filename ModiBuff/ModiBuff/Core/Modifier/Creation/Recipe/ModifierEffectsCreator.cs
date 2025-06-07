using System.Collections.Generic;
using System.Linq;

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
		private readonly EffectWrapper[] _callbackUnitRegisterWrappers;
		private readonly EffectWrapper[] _callbackEffectRegisterWrappers;
		private readonly EffectWrapper[] _callbackEffectUnitsRegisterWrappers;

		private IRevertEffect[] _revertEffects;
		private ISetDataEffect[] _setDataEffects;
		private IEffect[] _initEffects;
		private IEffect[] _intervalEffects;
		private IEffect[] _durationEffects;
		private IStackEffect[] _stackEffects;
		private IEffect[][] _callbackUnitEffects;
		private IEffect[][] _callbackEffectEffects;
		private IEffect[][] _callbackEffectUnitsEffects;

		private int _revertEffectsIndex,
			_initEffectsIndex,
			_intervalEffectsIndex,
			_durationEffectsIndex,
			_stackEffectsIndex,
			_callbackUnitEffectsIndex,
			_callbackEffectEffectsIndex,
			_callbackEffectUnitsEffectsIndex,
			_callbackUnitEffectsIndex2,
			_callbackUnitEffectsIndex3,
			_callbackUnitEffectsIndex4,
			_callbackEffectEffectsIndex2,
			_callbackEffectEffectsIndex3,
			_callbackEffectEffectsIndex4,
			_callbackEffectUnitsEffectsIndex2,
			_callbackEffectUnitsEffectsIndex3,
			_callbackEffectUnitsEffectsIndex4;

		public ModifierEffectsCreator(List<EffectWrapper> effectWrappers, EffectWrapper removeEffectWrapper,
			EffectWrapper dispelRegisterWrapper, EffectWrapper[] callbackUnitRegisterWrappers,
			EffectWrapper[] callbackEffectRegisterWrappers, EffectWrapper[] callbackEffectUnitsRegisterWrappers)
		{
			var effectsWithModifierInfoWrappers = new List<EffectWrapper>();
			var savableEffectsWrappers = new List<EffectWrapper>();
			_effectWrappers = effectWrappers.ToArray();
			_removeEffectWrapper = removeEffectWrapper;
			_dispelRegisterWrapper = dispelRegisterWrapper;
			_callbackUnitRegisterWrappers = callbackUnitRegisterWrappers;
			_callbackEffectRegisterWrappers = callbackEffectRegisterWrappers;
			_callbackEffectUnitsRegisterWrappers = callbackEffectUnitsRegisterWrappers;

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
				if ((effectWrapper.EffectOn & EffectOn.CallbackUnit) != 0)
					_callbackUnitEffectsIndex++;
				if ((effectWrapper.EffectOn & EffectOn.CallbackEffect) != 0)
					_callbackEffectEffectsIndex++;
				if ((effectWrapper.EffectOn & EffectOn.CallbackEffectUnits) != 0)
					_callbackEffectUnitsEffectsIndex++;
				if ((effectWrapper.EffectOn & EffectOn.CallbackUnit2) != 0)
					_callbackUnitEffectsIndex2++;
				if ((effectWrapper.EffectOn & EffectOn.CallbackUnit3) != 0)
					_callbackUnitEffectsIndex3++;
				if ((effectWrapper.EffectOn & EffectOn.CallbackUnit4) != 0)
					_callbackUnitEffectsIndex4++;
				if ((effectWrapper.EffectOn & EffectOn.CallbackEffect2) != 0)
					_callbackEffectEffectsIndex2++;
				if ((effectWrapper.EffectOn & EffectOn.CallbackEffect3) != 0)
					_callbackEffectEffectsIndex3++;
				if ((effectWrapper.EffectOn & EffectOn.CallbackEffect4) != 0)
					_callbackEffectEffectsIndex4++;
				if ((effectWrapper.EffectOn & EffectOn.CallbackEffectUnits2) != 0)
					_callbackEffectUnitsEffectsIndex2++;
				if ((effectWrapper.EffectOn & EffectOn.CallbackEffectUnits3) != 0)
					_callbackEffectUnitsEffectsIndex3++;
				if ((effectWrapper.EffectOn & EffectOn.CallbackEffectUnits4) != 0)
					_callbackEffectUnitsEffectsIndex4++;
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

			if (_callbackUnitEffectsIndex > 0 || _callbackUnitEffectsIndex2 > 0 ||
			    _callbackUnitEffectsIndex3 > 0 || _callbackUnitEffectsIndex4 > 0)
				_callbackUnitEffects = new IEffect[4][];

			if (_callbackEffectEffectsIndex > 0 || _callbackEffectEffectsIndex2 > 0 ||
			    _callbackEffectEffectsIndex3 > 0 || _callbackEffectEffectsIndex4 > 0)
				_callbackEffectEffects = new IEffect[4][];

			if (_callbackEffectUnitsEffectsIndex > 0 || _callbackEffectUnitsEffectsIndex2 > 0 ||
			    _callbackEffectUnitsEffectsIndex3 > 0 || _callbackEffectUnitsEffectsIndex4 > 0)
				_callbackEffectUnitsEffects = new IEffect[4][];

			if (_callbackUnitEffectsIndex > 0)
			{
				_callbackUnitEffects[0] = new IEffect[_callbackUnitEffectsIndex];
				_callbackUnitEffectsIndex = 0;
			}

			if (_callbackEffectEffectsIndex > 0)
			{
				_callbackEffectEffects[0] = new IEffect[_callbackEffectEffectsIndex];
				_callbackEffectEffectsIndex = 0;
			}

			if (_callbackEffectUnitsEffectsIndex > 0)
			{
				_callbackEffectUnitsEffects[0] = new IEffect[_callbackEffectUnitsEffectsIndex];
				_callbackEffectUnitsEffectsIndex = 0;
			}

			if (_callbackUnitEffectsIndex2 > 0)
			{
				_callbackUnitEffects[1] = new IEffect[_callbackUnitEffectsIndex2];
				_callbackUnitEffectsIndex2 = 0;
			}

			if (_callbackUnitEffectsIndex3 > 0)
			{
				_callbackUnitEffects[2] = new IEffect[_callbackUnitEffectsIndex3];
				_callbackUnitEffectsIndex3 = 0;
			}

			if (_callbackUnitEffectsIndex4 > 0)
			{
				_callbackUnitEffects[3] = new IEffect[_callbackUnitEffectsIndex4];
				_callbackUnitEffectsIndex4 = 0;
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

			if (_callbackEffectUnitsEffectsIndex2 > 0)
			{
				_callbackEffectUnitsEffects[1] = new IEffect[_callbackEffectUnitsEffectsIndex2];
				_callbackEffectUnitsEffectsIndex2 = 0;
			}

			if (_callbackEffectUnitsEffectsIndex3 > 0)
			{
				_callbackEffectUnitsEffects[2] = new IEffect[_callbackEffectUnitsEffectsIndex3];
				_callbackEffectUnitsEffectsIndex3 = 0;
			}

			if (_callbackEffectUnitsEffectsIndex4 > 0)
			{
				_callbackEffectUnitsEffects[3] = new IEffect[_callbackEffectUnitsEffectsIndex4];
				_callbackEffectUnitsEffectsIndex4 = 0;
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
				if ((effectOn & EffectOn.CallbackUnit) != 0)
					_callbackUnitEffects[0][_callbackUnitEffectsIndex++] = effect;
				if ((effectOn & EffectOn.CallbackEffect) != 0)
					_callbackEffectEffects[0][_callbackEffectEffectsIndex++] = effect;
				if ((effectOn & EffectOn.CallbackEffectUnits) != 0)
					_callbackEffectUnitsEffects[0][_callbackEffectUnitsEffectsIndex++] = effect;
				if ((effectOn & EffectOn.CallbackUnit2) != 0)
					_callbackUnitEffects[1][_callbackUnitEffectsIndex2++] = effect;
				if ((effectOn & EffectOn.CallbackUnit3) != 0)
					_callbackUnitEffects[2][_callbackUnitEffectsIndex3++] = effect;
				if ((effectOn & EffectOn.CallbackUnit4) != 0)
					_callbackUnitEffects[3][_callbackUnitEffectsIndex4++] = effect;
				if ((effectOn & EffectOn.CallbackEffect2) != 0)
					_callbackEffectEffects[1][_callbackEffectEffectsIndex2++] = effect;
				if ((effectOn & EffectOn.CallbackEffect3) != 0)
					_callbackEffectEffects[2][_callbackEffectEffectsIndex3++] = effect;
				if ((effectOn & EffectOn.CallbackEffect4) != 0)
					_callbackEffectEffects[3][_callbackEffectEffectsIndex4++] = effect;
				if ((effectOn & EffectOn.CallbackEffectUnits2) != 0)
					_callbackEffectUnitsEffects[1][_callbackEffectUnitsEffectsIndex2++] = effect;
				if ((effectOn & EffectOn.CallbackEffectUnits3) != 0)
					_callbackEffectUnitsEffects[2][_callbackEffectUnitsEffectsIndex3++] = effect;
				if ((effectOn & EffectOn.CallbackEffectUnits4) != 0)
					_callbackEffectUnitsEffects[3][_callbackEffectUnitsEffectsIndex4++] = effect;
			}

			_setDataEffects = _effectWrappers.Select(x => x.GetEffect() as ISetDataEffect).Where(x => x != null)
				.ToArray(); //TODO

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
			for (int i = 0; i < _callbackUnitRegisterWrappers?.Length; i++)
				_callbackUnitRegisterWrappers[i].GetEffectAs<IRecipeFeedEffects>().SetEffects(_callbackUnitEffects[i]);
			for (int i = 0; i < _callbackEffectRegisterWrappers?.Length; i++)
				_callbackEffectRegisterWrappers[i].GetEffectAs<IRecipeFeedEffects>()
					.SetEffects(_callbackEffectEffects[i]);
			for (int i = 0; i < _callbackEffectUnitsRegisterWrappers?.Length; i++)
				_callbackEffectUnitsRegisterWrappers[i].GetEffectAs<IRecipeFeedEffects>()
					.SetEffects(_callbackEffectUnitsEffects[i]);

			if (_removeEffectWrapper != null && _revertEffects != null)
				_removeEffectWrapper.GetEffectAs<RemoveEffect>().SetRevertibleEffects(_revertEffects);

			_dispelRegisterWrapper?.GetEffectAs<DispelRegisterEffect>()
				.SetRemoveEffect(_removeEffectWrapper.GetEffectAs<RemoveEffect>());

			//Reset all the clones in wrappers
			for (int i = 0; i < _effectWrappers.Length; i++)
				_effectWrappers[i].Reset();

			return new SyncedModifierEffects(_setDataEffects, _initEffects, _intervalEffects, _durationEffects,
				_stackEffects, effectStateInfo, effectSaveState);
		}
	}

	public readonly ref struct SyncedModifierEffects
	{
		public readonly ISetDataEffect[] SetDataEffects;
		public readonly IEffect[] InitEffects;
		public readonly IEffect[] IntervalEffects;
		public readonly IEffect[] DurationEffects;
		public readonly IStackEffect[] StackEffects;
		public readonly EffectStateInfo EffectStateInfo;
		public readonly EffectSaveState EffectSaveState;

		public SyncedModifierEffects(ISetDataEffect[] dataEffects, IEffect[] initEffectsArray,
			IEffect[] intervalEffectsArray, IEffect[] durationEffectsArray, IStackEffect[] stackEffectsArray,
			EffectStateInfo effectStateInfo, EffectSaveState effectSaveState)
		{
			SetDataEffects = dataEffects;
			InitEffects = initEffectsArray;
			IntervalEffects = intervalEffectsArray;
			DurationEffects = durationEffectsArray;
			StackEffects = stackEffectsArray;
			EffectStateInfo = effectStateInfo;
			EffectSaveState = effectSaveState;
		}
	}
}
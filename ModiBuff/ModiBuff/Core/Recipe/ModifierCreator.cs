using System;
using System.Collections.Generic;

namespace ModiBuff.Core
{
	/// <summary>
	///		Class responsible for handling the correct cloning and sync of modifiers
	/// </summary>
	public sealed class ModifierCreator
	{
		private readonly EffectWrapper[] _effectWrappersArray;
		private readonly EffectWrapper _removeEffectWrapper;

		private readonly IRevertEffect[] _revertListArray;
		private readonly IEffect[] _initEffectsArray;
		private readonly IEffect[] _intervalEffectsArray;
		private readonly IEffect[] _durationEffectsArray;
		private readonly IStackEffect[] _stackEffectsArray;

		private int _revertListIndex, _initEffectsIndex, _intervalEffectsIndex, _durationEffectsIndex, _stackEffectsIndex;

		public ModifierCreator(List<EffectWrapper> effectWrappers, EffectWrapper removeEffectWrapper)
		{
			_effectWrappersArray = effectWrappers.ToArray();
			_removeEffectWrapper = removeEffectWrapper;

			SetupArrays(out int initEffectsCount, out int intervalEffectsCount, out int durationEffectsCount,
				out int stackEffectsCount, out int revertListCount);
			_revertListArray = new IRevertEffect[revertListCount];
			_initEffectsArray = new IEffect[initEffectsCount];
			_intervalEffectsArray = new IEffect[intervalEffectsCount];
			_durationEffectsArray = new IEffect[durationEffectsCount];
			_stackEffectsArray = new IStackEffect[stackEffectsCount];
		}

		private void SetupArrays(out int initEffectsCount, out int intervalEffectsCount, out int durationEffectsCount,
			out int stackEffectsCount, out int revertEffectsCount)
		{
			initEffectsCount = 0;
			intervalEffectsCount = 0;
			durationEffectsCount = 0;
			stackEffectsCount = 0;
			revertEffectsCount = 0;

			if (_removeEffectWrapper != null)
			{
				if ((_removeEffectWrapper.EffectOn & EffectOn.Init) != 0) //Probably never a thing, but added just in case
					initEffectsCount++;
				if ((_removeEffectWrapper.EffectOn & EffectOn.Interval) != 0)
					intervalEffectsCount++;
				if ((_removeEffectWrapper.EffectOn & EffectOn.Duration) != 0)
					durationEffectsCount++;
			}

			for (int i = 0; i < _effectWrappersArray.Length; i++)
			{
				var effectWrapper = _effectWrappersArray[i];

				if (effectWrapper.GetEffect() is IRevertEffect revertEffect && revertEffect.IsRevertible)
					revertEffectsCount++;

				if ((effectWrapper.EffectOn & EffectOn.Init) != 0)
					initEffectsCount++;
				if ((effectWrapper.EffectOn & EffectOn.Interval) != 0)
					intervalEffectsCount++;
				if ((effectWrapper.EffectOn & EffectOn.Duration) != 0)
					durationEffectsCount++;
				if ((effectWrapper.EffectOn & EffectOn.Stack) != 0)
					stackEffectsCount++;
			}
		}

		public ModifierCreation Create(int genId)
		{
			if (_removeEffectWrapper != null)
			{
				_removeEffectWrapper.UpdateGenId(genId);

				if ((_removeEffectWrapper.EffectOn & EffectOn.Init) != 0) //Probably never a thing, but added just in case
					_initEffectsArray[_initEffectsIndex++] = _removeEffectWrapper.GetEffect();
				if ((_removeEffectWrapper.EffectOn & EffectOn.Interval) != 0)
					_intervalEffectsArray[_intervalEffectsIndex++] = _removeEffectWrapper.GetEffect();
				if ((_removeEffectWrapper.EffectOn & EffectOn.Duration) != 0)
					_durationEffectsArray[_durationEffectsIndex++] = _removeEffectWrapper.GetEffect();
			}

			for (int i = 0; i < _effectWrappersArray.Length; i++)
			{
				var effectWrapper = _effectWrappersArray[i];
				var effect = effectWrapper.GetEffect();
				var effectOn = effectWrapper.EffectOn;

				if (effect is IRevertEffect revertEffect && revertEffect.IsRevertible)
					_revertListArray[_revertListIndex++] = revertEffect;

				if ((effectOn & EffectOn.Init) != 0)
					_initEffectsArray[_initEffectsIndex++] = effect;
				if ((effectOn & EffectOn.Interval) != 0)
					_intervalEffectsArray[_intervalEffectsIndex++] = effect;
				if ((effectOn & EffectOn.Duration) != 0)
					_durationEffectsArray[_durationEffectsIndex++] = effect;
				if ((effectOn & EffectOn.Stack) != 0)
					_stackEffectsArray[_stackEffectsIndex++] = (IStackEffect)effect;
			}

			if (_removeEffectWrapper != null)
			{
				var clonedArray = new IRevertEffect[_revertListIndex];
				Array.Copy(_revertListArray, clonedArray, _revertListIndex);
				((RemoveEffect)_removeEffectWrapper.GetEffect()).SetRevertibleEffects(clonedArray);
				_removeEffectWrapper.Reset();
			}

			for (int i = 0; i < _effectWrappersArray.Length; i++)
				_effectWrappersArray[i].Reset();

			return new ModifierCreation(_initEffectsIndex, _intervalEffectsIndex, _durationEffectsIndex, _stackEffectsIndex,
				_initEffectsArray, _intervalEffectsArray, _durationEffectsArray, _stackEffectsArray);
		}

		public void Reset()
		{
			_revertListIndex = 0;
			_initEffectsIndex = 0;
			_intervalEffectsIndex = 0;
			_durationEffectsIndex = 0;
			_stackEffectsIndex = 0;
		}
	}

	public readonly ref struct ModifierCreation
	{
		public readonly IEffect[] InitEffects;
		public readonly IEffect[] IntervalEffects;
		public readonly IEffect[] DurationEffects;
		public readonly IStackEffect[] StackEffects;

		public ModifierCreation(int initEffectsIndex, int intervalEffectsIndex, int durationEffectsIndex, int stackEffectsIndex,
			IEffect[] initEffectsArray, IEffect[] intervalEffectsArray, IEffect[] durationEffectsArray, IStackEffect[] stackEffectsArray)
		{
			if (initEffectsIndex > 0)
			{
				InitEffects = new IEffect[initEffectsIndex];
				Array.Copy(initEffectsArray, InitEffects, initEffectsIndex);
			}
			else
				InitEffects = null;

			if (intervalEffectsIndex > 0)
			{
				IntervalEffects = new IEffect[intervalEffectsIndex];
				Array.Copy(intervalEffectsArray, IntervalEffects, intervalEffectsIndex);
			}
			else
				IntervalEffects = null;

			if (durationEffectsIndex > 0)
			{
				DurationEffects = new IEffect[durationEffectsIndex];
				Array.Copy(durationEffectsArray, DurationEffects, durationEffectsIndex);
			}
			else
				DurationEffects = null;

			if (stackEffectsIndex > 0)
			{
				StackEffects = new IStackEffect[stackEffectsIndex];
				Array.Copy(stackEffectsArray, StackEffects, stackEffectsIndex);
			}
			else
				StackEffects = null;
		}
	}
}
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

				if (effectWrapper.GetEffect() is IRevertEffect revertEffect && revertEffect.IsRevertible)
					_revertListArray[_revertListIndex++] = (IRevertEffect)effectWrapper.GetEffect();

				if ((effectWrapper.EffectOn & EffectOn.Init) != 0)
					_initEffectsArray[_initEffectsIndex++] = effectWrapper.GetEffect();
				if ((effectWrapper.EffectOn & EffectOn.Interval) != 0)
					_intervalEffectsArray[_intervalEffectsIndex++] = effectWrapper.GetEffect();
				if ((effectWrapper.EffectOn & EffectOn.Duration) != 0)
					_durationEffectsArray[_durationEffectsIndex++] = effectWrapper.GetEffect();
				if ((effectWrapper.EffectOn & EffectOn.Stack) != 0)
					_stackEffectsArray[_stackEffectsIndex++] = (IStackEffect)effectWrapper.GetEffect();
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

			return new ModifierCreation(_initEffectsArray, _intervalEffectsArray, _durationEffectsArray, _stackEffectsArray);
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

	public readonly struct ModifierCreation
	{
		public readonly IEffect[] InitEffects;
		public readonly IEffect[] IntervalEffects;
		public readonly IEffect[] DurationEffects;
		public readonly IStackEffect[] StackEffects;

		public ModifierCreation(IEffect[] initEffects, IEffect[] intervalEffects, IEffect[] durationEffects, IStackEffect[] stackEffects)
		{
			InitEffects = new IEffect[initEffects.Length];
			Array.Copy(initEffects, InitEffects, initEffects.Length);
			IntervalEffects = new IEffect[intervalEffects.Length];
			Array.Copy(intervalEffects, IntervalEffects, intervalEffects.Length);
			DurationEffects = new IEffect[durationEffects.Length];
			Array.Copy(durationEffects, DurationEffects, durationEffects.Length);
			StackEffects = new IStackEffect[stackEffects.Length];
			Array.Copy(stackEffects, StackEffects, stackEffects.Length);
		}
	}
}
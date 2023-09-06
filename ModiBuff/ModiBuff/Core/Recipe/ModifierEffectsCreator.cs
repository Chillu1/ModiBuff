using System.Collections.Generic;

namespace ModiBuff.Core
{
	/// <summary>
	///		Class responsible for handling the correct cloning and sync of modifiers
	/// </summary>
	public sealed class ModifierEffectsCreator
	{
		private readonly EffectWrapper[] _effectWrappersArray;
		private readonly EffectWrapper _removeEffectWrapper;
		private readonly int _revertEffectsCount, _initEffectsCount, _intervalEffectsCount, _durationEffectsCount, _stackEffectsCount;

		private IRevertEffect[] _revertEffects;
		private IEffect[] _initEffectsArray;
		private IEffect[] _intervalEffectsArray;
		private IEffect[] _durationEffectsArray;
		private IStackEffect[] _stackEffectsArray;

		private int _revertEffectsIndex, _initEffectsIndex, _intervalEffectsIndex, _durationEffectsIndex, _stackEffectsIndex;

		public ModifierEffectsCreator(List<EffectWrapper> effectWrappers, EffectWrapper removeEffectWrapper)
		{
			_effectWrappersArray = effectWrappers.ToArray();
			_removeEffectWrapper = removeEffectWrapper;

			SetupArrays(out int initEffectsCount, out int intervalEffectsCount, out int durationEffectsCount,
				out int stackEffectsCount, out int revertEffectsCount);
			_revertEffectsCount = revertEffectsCount;
			_initEffectsCount = initEffectsCount;
			_intervalEffectsCount = intervalEffectsCount;
			_durationEffectsCount = durationEffectsCount;
			_stackEffectsCount = stackEffectsCount;
			//_revertEffects = new IRevertEffect[revertEffectsCount];
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

		public SyncedModifierEffects Create(int genId)
		{
			if (_initEffectsCount > 0)
			{
				_initEffectsIndex = 0;
				_initEffectsArray = new IEffect[_initEffectsCount];
			}

			if (_intervalEffectsCount > 0)
			{
				_intervalEffectsIndex = 0;
				_intervalEffectsArray = new IEffect[_intervalEffectsCount];
			}

			if (_durationEffectsCount > 0)
			{
				_durationEffectsIndex = 0;
				_durationEffectsArray = new IEffect[_durationEffectsCount];
			}

			if (_stackEffectsCount > 0)
			{
				_stackEffectsIndex = 0;
				_stackEffectsArray = new IStackEffect[_stackEffectsCount];
			}

			if (_revertEffectsCount > 0)
			{
				_revertEffectsIndex = 0;
				_revertEffects = new IRevertEffect[_revertEffectsCount];
			}

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
					_revertEffects[_revertEffectsIndex++] = revertEffect;

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
				if (_revertEffects != null)
					((RemoveEffect)_removeEffectWrapper.GetEffect()).SetRevertibleEffects(_revertEffects);
				_removeEffectWrapper.Reset();
			}

			for (int i = 0; i < _effectWrappersArray.Length; i++)
				_effectWrappersArray[i].Reset();

			return new SyncedModifierEffects(_initEffectsArray, _intervalEffectsArray, _durationEffectsArray, _stackEffectsArray);
		}
	}

	public readonly ref struct SyncedModifierEffects
	{
		public readonly IEffect[] InitEffects;
		public readonly IEffect[] IntervalEffects;
		public readonly IEffect[] DurationEffects;
		public readonly IStackEffect[] StackEffects;

		public SyncedModifierEffects(IEffect[] initEffectsArray, IEffect[] intervalEffectsArray, IEffect[] durationEffectsArray,
			IStackEffect[] stackEffectsArray)
		{
			InitEffects = initEffectsArray;
			IntervalEffects = intervalEffectsArray;
			DurationEffects = durationEffectsArray;
			StackEffects = stackEffectsArray;
		}
	}
}
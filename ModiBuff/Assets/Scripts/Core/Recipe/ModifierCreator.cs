using System.Collections.Generic;

namespace ModiBuff.Core
{
	public sealed class ModifierCreator
	{
		private readonly List<EffectWrapper> _effectWrappers;
		private List<IRevertEffect> _revertList;
		private List<IEffect> _initEffects;
		private List<IEffect> _intervalEffects;
		private List<IEffect> _durationEffects;
		private List<IStackEffect> _stackEffects;

		public ModifierCreator(List<IEffect>[] effectBinds)
		{
			_effectWrappers = new List<EffectWrapper>(effectBinds.Length);
			for (int i = 0; i < effectBinds.Length; i++)
			{
				var effectOn = (EffectOn)(1 << i);
				//Debug.Log(effectOn);//TODO Sometimes we get 16? How?
				for (int j = 0; j < effectBinds[i].Count; j++)
				{
					if (_effectWrappers.Exists(w => w.IsSameEffect(effectBinds[i][j], effectOn)))
						continue;

					_effectWrappers.Add(new EffectWrapper(effectBinds[i][j], effectOn));
				}
			}

			_revertList = new List<IRevertEffect>();

			_initEffects = new List<IEffect>();
			_intervalEffects = new List<IEffect>();
			_durationEffects = new List<IEffect>();
			_stackEffects = new List<IStackEffect>();
		}

		public ModifierCreation Create(EffectWrapper removeEffectWrapper)
		{
			if (removeEffectWrapper != null)
			{
				if ((removeEffectWrapper.EffectOn & EffectOn.Init) != 0) //Probably never a thing, but added just in case
					_initEffects.Add(removeEffectWrapper.GetEffect());
				if ((removeEffectWrapper.EffectOn & EffectOn.Interval) != 0)
					_intervalEffects.Add(removeEffectWrapper.GetEffect());
				if ((removeEffectWrapper.EffectOn & EffectOn.Duration) != 0)
					_durationEffects.Add(removeEffectWrapper.GetEffect());
			}

			for (int i = 0; i < _effectWrappers.Count; i++)
			{
				var effectWrapper = _effectWrappers[i];

				if (effectWrapper.GetEffect() is IRevertEffect revertEffect && revertEffect.IsRevertible)
					_revertList.Add((IRevertEffect)effectWrapper.GetEffect());

				if ((effectWrapper.EffectOn & EffectOn.Init) != 0)
					_initEffects.Add(effectWrapper.GetEffect());
				if ((effectWrapper.EffectOn & EffectOn.Interval) != 0)
					_intervalEffects.Add(effectWrapper.GetEffect());
				if ((effectWrapper.EffectOn & EffectOn.Duration) != 0)
					_durationEffects.Add(effectWrapper.GetEffect());
				if ((effectWrapper.EffectOn & EffectOn.Stack) != 0)
					_stackEffects.Add((IStackEffect)effectWrapper.GetEffect());
			}

			if (removeEffectWrapper != null)
			{
				((RemoveEffect)removeEffectWrapper.GetEffect()).SetRevertibleEffects(_revertList.ToArray());
				removeEffectWrapper.Reset();
			}

			for (int i = 0; i < _effectWrappers.Count; i++)
				_effectWrappers[i].Reset();

			return new ModifierCreation(_revertList, _initEffects, _intervalEffects, _durationEffects, _stackEffects);
		}

		public void Clear()
		{
			_revertList.Clear();
			_initEffects.Clear();
			_intervalEffects.Clear();
			_durationEffects.Clear();
			_stackEffects.Clear();
		}
	}

	public struct ModifierCreation
	{
		public List<IRevertEffect> revertList;

		public List<IEffect> initEffects;
		public List<IEffect> intervalEffects;
		public List<IEffect> durationEffects;
		public List<IStackEffect> stackEffects;

		public ModifierCreation(List<IRevertEffect> revertList, List<IEffect> initEffects, List<IEffect> intervalEffects,
			List<IEffect> durationEffects, List<IStackEffect> stackEffects)
		{
			this.revertList = revertList;

			this.initEffects = initEffects;
			this.intervalEffects = intervalEffects;
			this.durationEffects = durationEffects;
			this.stackEffects = stackEffects;
		}
	}

	public sealed class EffectWrapper
	{
		private readonly IEffect _effect;
		public EffectOn EffectOn { get; private set; }

		private IEffect _effectClone;

		public EffectWrapper(IEffect effect, EffectOn effectOn)
		{
			_effect = effect;
			EffectOn = effectOn;
		}

		public bool IsSameEffect(IEffect effect, EffectOn effectOn)
		{
			bool isSameEffect = _effect == effect;
			if (isSameEffect)
				EffectOn |= effectOn;
			return isSameEffect;
		}

		public IEffect GetEffect()
		{
			//If IEffect inherits IShallowClone, then clone
			if (_effect is IStackEffect stackEffect)
			{
				if (_effectClone == null)
					_effectClone = (IEffect)stackEffect.ShallowClone();
				return _effectClone;
			}

			//if (_effect is IRevertEffect revertEffect)
			//{
			//	if (_effectClone == null)
			//		_effectClone = (IEffect)revertEffect.ShallowClone();
			//	return _effectClone;
			//}

			if (_effect is RemoveEffect removeEffect)
			{
				if (_effectClone == null)
					_effectClone = removeEffect.ShallowClone();
				return _effectClone;
			}

			return _effect;
		}

		public void Reset()
		{
			_effectClone = null;
		}
	}
}
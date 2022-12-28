using System.Collections.Generic;

namespace ModiBuff.Core
{
	public sealed class ModifierCreator
	{
		private readonly List<EffectWrapper> _effectWrappers;
		private readonly List<IRevertEffect> _revertList;
		private readonly List<IEffect> _initEffects;
		private readonly List<IEffect> _intervalEffects;
		private readonly List<IEffect> _durationEffects;
		private readonly List<IStackEffect> _stackEffects;

		public ModifierCreator(List<EffectWrapper> effectWrappers)
		{
			_effectWrappers = effectWrappers;

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
}
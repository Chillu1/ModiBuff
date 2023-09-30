using System;
using ModiBuff.Core;

namespace ModiBuff.Examples.SimpleSolo
{
	/// <summary>
	///		Full example of a custom effect implementation
	/// </summary>
	public sealed class FullBlockEffect : IEffect, ITargetEffect, IStackEffect, IStateEffect
	{
		private readonly int _baseBlock;
		private readonly StackBlockEffectType _stackEffect;
		private Targeting _targeting;

		private int _extraBlock;

		public FullBlockEffect(int block, StackBlockEffectType stackEffect = StackBlockEffectType.Effect) :
			this(block, stackEffect, Targeting.TargetSource)
		{
		}

		private FullBlockEffect(int block, StackBlockEffectType stackEffect, Targeting targeting)
		{
			_baseBlock = block;
			_stackEffect = stackEffect;
			_targeting = targeting;
		}

		public void SetTargeting(Targeting targeting) => _targeting = targeting;

		public void Effect(IUnit target, IUnit source)
		{
#if DEBUG && !MODIBUFF_PROFILE
			if (!(target is IBlockOwner))
				throw new ArgumentException("Target must implement IBlockOwner");
			if (!(source is IBlockOwner) && _targeting == Targeting.SourceTarget || _targeting == Targeting.SourceSource)
				throw new ArgumentException("Source must implement IBlockOwner when targeting source");
#endif
			_targeting.UpdateTarget(ref target, source);
			((IBlockOwner)target).AddBlock(_baseBlock + _extraBlock);
		}

		public void StackEffect(int stacks, float block, IUnit target, IUnit source)
		{
			if ((_stackEffect & StackBlockEffectType.Add) != 0)
				_extraBlock += (int)block;

			if ((_stackEffect & StackBlockEffectType.AddStacksBased) != 0)
				_extraBlock += (int)block * stacks;

			if ((_stackEffect & StackBlockEffectType.Effect) != 0)
				Effect(target, source);
		}

		//TODO Revertible mechanic missing

		public void ResetState() => _extraBlock = 0;

		public IEffect ShallowClone() => new FullBlockEffect(_baseBlock, _stackEffect, _targeting);
		object IShallowClone.ShallowClone() => ShallowClone();
	}

	[Flags]
	public enum StackBlockEffectType
	{
		None = 0,
		Effect = 1,
		Add = 2,
		AddStacksBased = 4,
	}
}
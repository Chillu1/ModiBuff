using System;
using ModiBuff.Core;

namespace ModiBuff.Examples.SimpleSolo
{
	/// <summary>
	///		Example simple custom effect implementation
	/// </summary>
	public sealed class BlockEffect : BaseEffect
	{
		private readonly int _blockAmount;

		public BlockEffect(int blockAmount) => _blockAmount = blockAmount;

		public override void Effect(IUnit target, IUnit source)
		{
#if DEBUG
			if (target is not IBlockOwner)
				throw new ArgumentException("Target must implement IBlockOwner");
#endif

			((IBlockOwner)target).AddBlock(_blockAmount);
		}
	}
}
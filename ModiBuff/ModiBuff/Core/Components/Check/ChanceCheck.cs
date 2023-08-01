using System;

namespace ModiBuff.Core
{
	public sealed class ChanceCheck
	{
		private readonly float _chance;

		public ChanceCheck(float chance) => _chance = chance;

		public bool Roll() => Random.Value <= _chance;
	}
}
using UnityEngine;

namespace ModifierLibraryLite.Core
{
	public sealed class ChanceCheck
	{
		private readonly float _chance;

		public ChanceCheck(float chance) => _chance = chance;

		public bool Roll() => Random.value <= _chance;
	}
}
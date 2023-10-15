namespace ModiBuff.Core.Units
{
	public sealed class ChanceCheck : INoUnitCheck, IDataCheck<float>
	{
		private readonly float _chance;

		public ChanceCheck(float chance) => _chance = chance;

		public bool Check() => Random.Value <= _chance;

		public float GetData() => _chance;
	}
}
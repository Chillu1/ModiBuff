namespace ModiBuff.Core.Units
{
	public readonly struct ModifierApplierReference
	{
		public int Id { get; init; }
		public ApplierType Type { get; init; }
		public ICheck[] Checks { get; init; }

		public ModifierApplierReference(int id, ApplierType type, ICheck[] checks = null)
		{
			Id = id;
			Type = type;
			Checks = checks;
		}
	}
}
namespace ModiBuff.Core
{
	/// <summary>
	///		Used to store the data how a modifier should be added
	/// </summary>
	public sealed class ModifierAddReference
	{
		public int Id { get; }

		public bool IsApplierType => ApplierType != null;
		public bool HasApplyChecks { get; }
		public ApplierType? ApplierType { get; }

		public ModifierAddReference(IModifierGenerator generator, ApplierType? applierType = null)
		{
			Id = generator.Id;
			if (generator is IModifierApplyCheckGenerator applyCheckGenerator)
				HasApplyChecks = applyCheckGenerator.HasApplyChecks;
			ApplierType = applierType;
		}

		public ModifierAddReference(IModifierApplyCheckGenerator generator, ApplierType? applierType = null)
		{
			Id = generator.Id;
			HasApplyChecks = generator.HasApplyChecks;
			ApplierType = applierType;
		}
	}
}
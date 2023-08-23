namespace ModiBuff.Core
{
	/// <summary>
	///		Used to store the data how a modifier should be added
	/// </summary>
	public sealed class ModifierAddReference
	{
		public int Id { get; }

		public bool IsApplierType => ApplierType != ApplierType.None;
		public bool HasApplyChecks { get; }
		public ApplierType ApplierType { get; }

		public ModifierAddReference(IModifierRecipe recipe, ApplierType applierType = ApplierType.None)
		{
			Id = recipe.Id;
			HasApplyChecks = recipe.HasApplyChecks;
			ApplierType = applierType;
		}
	}
}
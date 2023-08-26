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
			if (recipe is IModifierApplyCheckRecipe applyCheckRecipe)
				HasApplyChecks = applyCheckRecipe.HasApplyChecks;
			ApplierType = applierType;
		}

		public ModifierAddReference(IModifierApplyCheckRecipe recipe, ApplierType applierType = ApplierType.None)
		{
			Id = recipe.Id;
			HasApplyChecks = recipe.HasApplyChecks;
			ApplierType = applierType;
		}
	}
}
namespace ModiBuff.Core
{
	/// <summary>
	///		Used to store the data how a modifier should be added
	/// </summary>
	public sealed class ModifierAddReference //TODO Rename
	{
		public int Id { get; }
		public IModifierRecipe Recipe { get; } //TODO Temporary
		public ApplierType ApplierType { get; }

		public ModifierAddReference(int id, IModifierRecipe recipe, ApplierType applierType = ApplierType.None)
		{
			Id = id;
			Recipe = recipe;
			ApplierType = applierType;
		}
	}
}
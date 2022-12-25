namespace ModiBuff.Core
{
	/// <summary>
	///		Used to store the data how a modifier should be added
	/// </summary>
	public sealed class ModifierAddReference //TODO Rename
	{
		public int Id { get; }
		public ModifierRecipe Recipe { get; } //TODO Temporary
		public ApplierType ApplierType { get; }

		public ModifierAddReference(int id, ModifierRecipe recipe, ApplierType applierType = ApplierType.None)
		{
			Id = id;
			Recipe = recipe;
			ApplierType = applierType;
		}
	}
}
namespace ModiBuff.Core
{
	public interface IModifierRecipe
	{
		int Id { get; }
		string Name { get; }

		ModifierAddData CreateAddData();
		IModifierGenerator CreateModifierGenerator();
	}
}
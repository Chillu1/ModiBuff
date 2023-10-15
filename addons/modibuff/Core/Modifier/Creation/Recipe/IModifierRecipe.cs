namespace ModiBuff.Core
{
	public interface IModifierRecipe
	{
		int Id { get; }
		string Name { get; }

		IModifierGenerator CreateModifierGenerator();
		ModifierInfo CreateModifierInfo();
		TagType GetTag();
	}
}
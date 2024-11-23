namespace ModiBuff.Core
{
	public interface IModifierRecipe
	{
		int Id { get; }
		string Name { get; }

		IModifierGenerator CreateModifierGenerator();
		ModifierInfo CreateModifierInfo();
		TagType GetTag();
		int GetAuraId();
		object GetData();

		//TODO Move/refactor
		ModifierRecipe.SaveData SaveState();
		//void LoadState(ModifierRecipe.SaveData saveData);
	}
}
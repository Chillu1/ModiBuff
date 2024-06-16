namespace ModiBuff.Core
{
	public sealed class EmptyModifierRecipes : ModifierRecipes
	{
		public EmptyModifierRecipes(ModifierIdManager idManager, EffectTypeIdManager effectTypeIdManager)
			: base(idManager, effectTypeIdManager)
		{
		}
	}
}